# Setup
Install XAMPP locally and setup the Database in the database folder.

Then symlink this Folder in your XAMPP PHP directory.
Under windows it looks like this:
``mklink Link Target``

Example in cmd:
``mklink /d "C:\xampp\htdocs\bi-ba-boulder-backend" "C:\dev\Git\bi-ba-boulder\backend"``
and in Powershell:
``cmd /c mklink /d "C:\xampp\htdocs\bi-ba-boulder-backend" "C:\dev\Git\bi-ba-boulder\backend"``

Removing is working like this:
``del symlink``

Example in Powershell:
``del "C:\xampp\htdocs\bi-ba-boulder"``

## Db credentials
Add a `api/dbsettings.php` file with the following content and replace the fields according:
```
<?php
define("DB_HOST", "localhost");
define("DB_NAME", "bi-ba-boulder-db");
define("DB_CHARSET", "utf8");
define("DB_USER", "root");
define("DB_PASSWORD", "");
?>
```
# Developing
Start Apache and MySQL in XAMPP. Visit http://localhost/bi-ba-boulder-backend/ and you should get a response.
