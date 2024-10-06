<?php
require "../lib-files/points-lib.php";

function respond ($status, $message, $more=null, $http=null) {
  if ($http !== null) { http_response_code($http); }
  exit(json_encode([
    "status" => $status,
    "message" => $message,
    "more" => $more
  ]));
}

function lcheck () {
  // if (!isset($_SESSION["user"])) {
  //   respond(0, "Please sign in first", null, 403);
  // }
}

if (isset($_POST["req"])) { switch ($_POST["req"]) {
  default:
    respond(false, "Invalid request", null, null, 400);
    break;

  case "save": lcheck();
    $pass = $POINT->save(
      $_POST["email"], $_POST["password"],
      isset($_POST["id"]) ? $_POST["id"] : null
    );
    respond($pass, $pass?"OK":$POINT->error);
    break;

  case "del": lcheck();
    $pass = $POINT->del($_POST["id"]);
    respond($pass, $pass?"OK":$POINT->error);
    break;

  case "get": lcheck();
    respond(true, "OK", $POINT->get($_POST["id"]));
    break;

  // (D5) LOGIN
  case "in":
    // ALREADY SIGNED IN
    if (isset($_SESSION["user"])) { respond(true, "OK"); }

    // CREDENTIALS CHECK
    $pass = $POINT->verify($_POST["email"], $_POST["password"]);
    respond($pass, $pass?"OK":"Invalid email/password");
    break;

  // (D6) LOGOUT
  case "out":
    unset($_SESSION["user"]);
    respond(true, "OK");
    break;
}
?>
