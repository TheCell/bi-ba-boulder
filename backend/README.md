# Setup
- Install Symfony [https://symfony.com/download](https://symfony.com/download)
- Change sql User in .env or Add the user

In this directory use:
Copy the .env to .env.local file and insert the missing variables   
`composer install` or `symfony composer install`   
`npm install @openapitools/openapi-generator-cli -g`

## Important PHP configuration
if you are using xampp you need to enable sopme extensions. Go to `xampp/php/php.ini` look for `;extension=gd2`, `;extension=openssl` and `;extension=sodium`. Uncomment all.   

Setup Db:  
- Set up a user (on the database (start xampp) [http://localhost/phpmyadmin/](http://localhost/phpmyadmin/) > User accounts > Add user account)  
- set the database user, password and database you want for develop in the env file.
- If you copy .env to .env.local you already have an example
- Then run: `symfony console doctrine:database:create` you should see a new database named `bibaboulder` in phpMyAdmin now  
- apply the migrations `symfony console doctrine:migrations:migrate`   

Generating JWT Tokens:
- `symfony console lexik:jwt:generate-keypair` to generate a private.pem and public.pem. Add them to `/config/jwt/` and add the passphrase you chose to `.env.local` (JWT_PASSPHRASE=your_secure_passphrase)

# Start
Open Powershell in the subfolder symfony-project
1. `cd .\backend\`
2. Start the Server: `symfony server:start`
3. Start MySQL in XAMPP
Stop the Server: `symfony server:stop`

# Generate API with OpenApi
generate openAPI:
1. Start the symfony server
2. Run in the root directory: `npm run generate:api` (You must have the java binary executable available on your PATH for this to work.)
3. then copy the generated api

# Releasing
`composer dump-env prod` copy the lines from the generated file over to the deploy workflow. I don't deploy all to the server and therefore I build the env in the pipeline
I use composer on the server to install symfony there

# Dev
Get all CLI commands: `symfony console list`   

## Create
Manually generate 16 bit binary Guids: [https://robobunny.com/cgi-bin/guid](https://robobunny.com/cgi-bin/guid)   
`symfony console make:entity _entityname_` // Create a new Entity   
`symfony console make:migration` // Create a migration   
`symfony console make:controller BlocsController` // Create a Controller    

## Migration
Generate a migration sql:  
`symfony console doctrine:migrations:migrate --write-sql=./migrations` (create sql. You do not want the new migration to be applied before running this command)  
`symfony console doctrine:migrations:status`  
`symfony console doctrine:migrations:migrate` (apply migration)  
`symfony console doctrine:migrations:migrate DoctrineMigrations\Version20241020215213` (apply migration down)   

## Debug
`symfony console debug:config security` // Debug routing security   
`symfony console debug:router` // Routing   
`symfony console debug:event` // Events   
`symfony console debug:event --dispatcher=security.event_dispatcher.login` // listen to events on the firewall 'login'   

## Startup
Clearing cache out
`symfony console cache:clear`   
`symfony server:start`   
`symfony server:stop`   
