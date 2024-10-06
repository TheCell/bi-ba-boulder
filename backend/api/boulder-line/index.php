<?php
header("Access-Control-Allow-Origin: *");
header("Content-Type: application/json; charset=UTF-8");

include_once '../database.php';
include_once '../class/boulder-line.php';
include_once "../utility.php";

$database = new Database();
$db = $database->getConnection();

$items = new BoulderLine($db);

$stmt = $items->getBoulderLines();
$itemCount = $stmt->rowCount();

if($itemCount > 0)
{
  $boulderLineArr = array();
  $boulderLineArr["body"] = array();
  $boulderLineArr["itemCount"] = $itemCount;

  while ($row = $stmt->fetch(PDO::FETCH_ASSOC))
  {
    extract($row);
    $e = array(
      "id" => bin_to_uuid($Id),
      "name" => $Name,
      "color" => $Color,
      "description" => $Description,
      "identifier" => $Identifier,
      "boulderBlocId" => bin_to_uuid($BoulderBlocId),
      "grade" => $Grade
    );

    array_push($boulderLineArr["body"], $e);
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
