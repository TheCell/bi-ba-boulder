# Bibaboulder

![Build and Deploy](https://github.com/thecell/bi-ba-boulder/actions/workflows/build-and-publish.yml/badge.svg)
[<img alt="Website Deployed for Free with FTP Deploy Action" src="https://img.shields.io/badge/Website deployed for free with-FTP DEPLOY ACTION-%3CCOLOR%3E?style=for-the-badge&color=297FA9">](https://github.com/SamKirkland/FTP-Deploy-Action)

The next todos:

[x] Figure a route dto out  
[] login  
[] 3D scan -> object with LOD process  
[x] Load LOD levels  
[] bloc caching  
[] Offline compatible  
[] setup cdn  
[] lines only load after login  
[] figure out enums for grading  
[] sector overview  
[] line, boulder, sector search  
[] hover over line  
[] height indicator  

# helpful resources
Generate interface etc. from json: https://app.quicktype.io/

to simulate the database locally I use XAMPP

# The Backend
The Backend is build with PHP, MySQL   
I'm using Symfony for the Backend and Composer to create the Files etc.

# Starting the Devtools
1. start xampp
2. npm start
3. in a second terminal `cd .\backend\symfony-project\` and then `symfony server:start`
