# Setup
- Install Symfony
- Change sql User in .env or Add the user

In this directory use:
Copy the .env to .env.local file and insert the missing variables
``composer install``
``npm install @openapitools/openapi-generator-cli -g``

Setup Db:
Set up a user (on the database > Privileges > Add user account)  
set the database user, password and database you want for develop in the env file.  
Then run: ``symfony console doctrine:database:create``  
you should see a new database in phpMyAdmin now  

# Start
Open Powershell in the subfolder symfony-project
1. ``cd .\backend\``
2. Start the Server: ``symfony server:start``
3. Start MySQL in XAMPP
Stop the Server: ``symfony server:stop``

# Generate OpenApi
generate openAPI:
1. Start the symfony server
2. Run in the root directory: ``npx openapi-generator-cli generate -i http://localhost:8000/api/doc.json -g typescript-angular -o src/app/api``
3. then copy the generated api

# Releasing
``composer dump-env prod`` copy the lines from the generated file over to the deploy workflow. I don't deploy all to the server and therefore I build the env in the pipeline
I use composer on the server to install symfony there

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

Clearing cache out
``symfony console cache:clear``
