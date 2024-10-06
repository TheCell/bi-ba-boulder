<?php
require "./users-lib.php";

function respond ($status, $message, $more=null, $http=null) {
  if ($http !== null) { http_response_code($http); }
  exit(json_encode([
    "status" => $status,
    "message" => $message,
    "more" => $more
  ]));
}

function lcheck () {
  if (!isset($_SESSION["user"]))
  {
    respond(0, "Please sign in first", null, 403);
  }
}

echo (var_dump($_POST));
echo (var_dump($_GET));
echo (var_dump($_SESSION));
if (isset($_POST["req"]))
{
  switch ($_POST["req"])
  {
    default:
      respond(false, "Invalid request", null, null, 400);
      break;

    case "save": lcheck();
      $pass = $USR->save(
        $_POST["email"], $_POST["password"],
        isset($_POST["id"]) ? $_POST["id"] : null
      );
      respond($pass, $pass?"OK":$USR->error);
      break;

    case "del": lcheck();
      $pass = $USR->del($_POST["id"]);
      respond($pass, $pass?"OK":$USR->error);
      break;

    case "get": lcheck();
      respond(true, "OK", $USR->get($_POST["id"]));
      break;

    case "in":
      if (isset($_SESSION["user"])) { respond(true, "OK"); }

      $pass = $USR->verify($_POST["email"], $_POST["password"]);
      respond($pass, $pass?"OK":"Invalid email/password");
      break;

    case "out":
      unset($_SESSION["user"]);
      respond(true, "OK");
      break;
  }
} else {
  echo "schade schokolade";
}

?>
