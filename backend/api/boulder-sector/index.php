<?php
header("Access-Control-Allow-Origin: *");
header("Content-Type: application/json; charset=UTF-8");

include_once '../database.php';
include_once '../class/boulder-sector.php';
include_once "../utility.php";

$database = new Database();
$db = $database->getConnection();

$items = new BoulderSector($db);

$stmt = $items->getBoulderSectors();
$itemCount = $stmt->rowCount();

if($itemCount > 0)
{
  $boulderLineArr = array();

  while ($row = $stmt->fetch(PDO::FETCH_ASSOC))
  {
    extract($row);
    $e = array(
      "id" => bin_to_uuid($Id),
      "name" => $Name,
      "description" => $Description
    );

    array_push($boulderLineArr, $e);
  }

  echo json_encode($boulderLineArr, JSON_PRETTY_PRINT);
}
else
{
    http_response_code(404);
    echo json_encode(
        array("message" => "No record found.")
    );
}

?>
