<?php
require "dbsettings.php";

class Database {

  public $conn;

  public function getConnection(){
      $this->conn = null;
      try{
          $this->conn = new PDO("mysql:host=".DB_HOST.";dbname=".DB_NAME.";charset=".DB_CHARSET, 
            DB_USER, DB_PASSWORD, [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC
          ]);
      }catch(PDOException $exception){
          echo "Database could not be connected: " . $exception->getMessage();
      }
      return $this->conn;
  }
}
?>
