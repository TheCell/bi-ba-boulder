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

function uuid_to_bin($uuid){
  return pack("H*", str_replace('-', '', $uuid));
}

function bin_to_uuid($bin){
  return join("-", unpack("H8time_low/H4time_mid/H4time_hi/H4clock_seq_hi/H12clock_seq_low", $bin));
}

function v4_UUID() {
    return sprintf('%04x%04x-%04x-%04x-%04x-%04x%04x%04x',
      // 32 bits for the time_low
      mt_rand(0, 0xffff), mt_rand(0, 0xffff),
      // 16 bits for the time_mid
      mt_rand(0, 0xffff),
      // 16 bits for the time_hi,
      mt_rand(0, 0x0fff) | 0x4000,

      // 8 bits and 16 bits for the clk_seq_hi_res,
      // 8 bits for the clk_seq_low,
      mt_rand(0, 0x3fff) | 0x8000,
      // 48 bits for the node
      mt_rand(0, 0xffff), mt_rand(0, 0xffff), mt_rand(0, 0xffff)
    );
  }


?>
