<?php
class Points {
  private $pdo = null;
  private $stmt = null;
  public $error = "";
  function __construct () {
    $this->pdo = new PDO(
      "mysql:host=".DB_HOST.";dbname=".DB_NAME.";charset=".DB_CHARSET,
      DB_USER, DB_PASSWORD, [
      PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
      PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC
    ]);
  }

  function __destruct () {
    if ($this->stmt!==null) { $this->stmt = null; }
    if ($this->pdo!==null) { $this->pdo = null; }
  }

  function query ($sql, $data) : void {
    $this->stmt = $this->pdo->prepare($sql);
    $this->stmt->execute($data);
  }

  function save ($email, $pass, $id=null) {
    $data = [$email, password_hash($pass, PASSWORD_BCRYPT)];
    if ($id===null) {
      $this->query("INSERT INTO `point` (`BoulderLineId `, `X`, `Y`, `Z`) VALUES (?,?,?,?)", $data);
    } else {
      $data[] = $id;
      $this->query("UPDATE `point` SET `BoulderLineId`=?, `X`=?, `Y`=?, `Z`=? WHERE `Id`=?", $data);
    }
    return true;
  }

  function del ($id) {
    $this->query("DELETE FROM `point` WHERE `Id`=?", [$id]);
    return true;
  }

  function get ($id) {
    $this->query("SELECT * FROM `point` WHERE `Id`=?", [$id]);
    return $this->stmt->fetch();
  }
}

include  '../dbsettings.php';

session_start();
$POINT = new Points();

?>
