<?php
require "utility.php";
?>

random uuid:
<?php
echo "<textarea rows=\"20\" cols=\"100\">";
for ($x = 0; $x <= 10; $x++) {
    $v4_uuid = v4_UUID();
    echo "'" . $v4_uuid . "' - '" . bin2hex(uuid_to_bin($v4_uuid)) . "'\n";
}
echo "</textarea>"
?>

<form method="post" action="">
  <input type="text" name="uuid" placeholder="uuid" value="<?php if (isset($_POST["binary"])) echo bin_to_uuid($_POST["binary"]) ?>">
  <input type="text" name="binary" placeholder="binary" value="<?php if (isset($_POST["uuid"])) echo uuid_to_bin($_POST["uuid"]) ?>">
  <input type="submit" value="Get">
</form>

<?php
echo var_dump();



?>
