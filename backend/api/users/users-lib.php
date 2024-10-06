<?php

require "../dbsettings.php";

class Users {
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
      $this->query("INSERT INTO `users` (`user_email`, `user_password`) VALUES (?,?)", $data);
    } else {
      $data[] = $id;
      $this->query("UPDATE `users` SET `user_email`=?, `user_password`=? WHERE `user_id`=?", $data);
    }
    return true;
  }

  function del ($id) {
    $this->query("DELETE FROM `users` WHERE `user_id`=?", [$id]);
    return true;
  }

  function get ($id) {
    $this->query("SELECT * FROM `users` WHERE `user_".(is_numeric($id)?"id":"email")."`=?", [$id]);
    return $this->stmt->fetch();
  }

  function verify ($email, $pass) {
    $user = $this->get($email);
    if (!is_array($user)) { return false; }

    if (password_verify($pass, $user["user_password"])) {
      $_SESSION["user"] = [
        "id" => $user["user_id"],
        "email" => $user["user_email"]
      ];
      return true;
    } else { return false; }
  }
}

session_start();
$USR = new Users();
?>
