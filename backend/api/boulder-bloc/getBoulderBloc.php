<?php
  header("Access-Control-Allow-Origin: *");
  header("Content-Type: application/json; charset=UTF-8");
  header("Access-Control-Allow-Methods: POST");
  header("Access-Control-Max-Age: 3600");
  header("Access-Control-Allow-Headers: Content-Type, Access-Control-Allow-Headers, Authorization, X-Requested-With");

  include_once '../database.php';
  include_once '../class/boulder-bloc.php';
  include_once "../utility.php";

  $database = new Database();
  $db = $database->getConnection();

  $item = new BoulderBloc($db);

  $item->Id = isset($_GET['id']) ? bin2hex(uuid_to_bin($_GET['id'])) : die();

  $item->getBoulderBloc();

  if($item->Name != null)
  {
    // create array
    $emp_arr = array(
      "id" => bin_to_uuid($item->Id),
      "name" => $item->Name,
      "description" => $item->Description
    );

    http_response_code(200);
    echo json_encode($emp_arr);
  }
  else
  {
    http_response_code(404);
    echo json_encode("BoulderBloc not found.");
  }
?>
