# Setup
- Install Symfony
- Change sql User in .env or Add the user

In this directory use:
``composer install``
``npm install @openapitools/openapi-generator-cli -g``

Setup Db:
``symfony console doctrine:database:create``

# Start
Open Powershell in the subfolder symfony-project
1. ``cd .\backend\symfony-project\``
2. Start the Server: ``symfony server:start``
3. Start MySQL in XAMPP
Stop the Server: ``symfony server:stop``

# Generate OpenApi

generate openAPI:
``npx openapi-generator-cli generate -i http://localhost:8000/api/doc.json -g typescript-angular -o src/app/api``
then copy the generated api

# Dev
Get all CLI commands:
``symfony console list``

Create a new Entity:
``symfony console make:entity _entityname_``

Create a migration:
``symfony console make:migration``

Generate a migration sql:
``symfony console doctrine:migrations:migrate --dry-run --write-sql=./migrations``

Apply migration:
``symfony console doctrine:migrations:migrate``

Create a Controller:
``symfony console make:controller BlocsController``

Routing:
``symfony console debug:router``
