# Setup
- Install Symfony
- Change sql User in .env or Add the user

Setup Db:
``symfony console doctrine:database:create``

# Start
Open Powershell in the subfolder symfony-project
Start the Server: ``symfony server:start``
Stop the Server: ``symfony server:stop``
Check out the documentation: ``http://localhost:8000/api``

# Dev
Get all CLI commands:
``symfony console list``

Create a new Entity:
``symfony console make:entity _entityname_``

Create a migration:
``symfony console make:migration``
``symfony console doctrine:migrations:migrate``
``symfony console doctrine:migrations:migrate --dry-run --write-sql=./migrations``

Create a Controller:
``symfony console make:controller BlocsController``

Routing:
``symfony console debug:router``

generate openAPI todo:
``npx openapi-generator-cli generate -i http://localhost:8000/api/doc.json -g typescript-angular -o src/app/core/api``

# all outdated:
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
