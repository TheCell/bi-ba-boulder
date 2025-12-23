# Bibaboulder

![Build and Deploy](https://github.com/thecell/bi-ba-boulder/actions/workflows/build-and-publish.yml/badge.svg)
[<img alt="Website Deployed for Free with FTP Deploy Action" src="https://img.shields.io/badge/Website deployed for free with-FTP DEPLOY ACTION-%3CCOLOR%3E?style=for-the-badge&color=297FA9">](https://github.com/SamKirkland/FTP-Deploy-Action)


[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-blue.svg
[![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]

Bi Ba Boulder © 2025 by Simon Hischier is licensed under CC BY-NC-SA 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by-nc-sa/4.0/ 

[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

The next todos:

[x] Figure a route dto out   
[x] login (needs frontend)   
[-] verify email (needs redirect to success page)   
[] resend verify email
[x] 3D scan -> object with LOD process   
[x] Load LOD levels   
[] bloc caching   
[] Offline compatible  
[] setup cdn   
[] lines only load after login   
[x] figure out enums for grading   
[] sector overview   
[] line, boulder, sector search   
[] hover over line   
[] height indicator   
[x] switch to uuid for id's   
[x] split model into parts   
[x] highlight of parts   
[x] texture rework   
[x] custom highlighting texture   
[x] build highlight texture optimizer   
[] streamline spraywall + rgb texture download not hardcoded   
[] Refresh token [https://github.com/markitosgv/JWTRefreshTokenBundle](https://github.com/markitosgv/JWTRefreshTokenBundle)   
[] logout   
[-] rate limiting   
[] upload only while logged in and verified   
[] Version Number   
[x] make http failure cases generic   
[] user profile page   
[] feedback form   
[x] style spraywall list   
[-] filter spraywall list   
[x] reset Create Spraywall after saving   
[] split production and develop   
[-] revert und redo function   
[] don't rerender when not in active use   
[] fix crashes on mobile   
[] feedback form   
[] predefined camera positions and reference character position   
[] check spraywall upload for 1 start 1 top hold   

# helpful resources
Generate interface etc. from json: [https://app.quicktype.io/](https://app.quicktype.io/)

to simulate the database locally I use XAMPP

# The Backend
The Backend is build with PHP, MySQL   
I'm using Symfony for the Backend and Composer to create the Files etc.  
Check out the Readme in the backend folder to set it up

# Setup Fileshare
1. Open an elevated PowerShell
2. ``New-Item -ItemType SymbolicLink -Path C:\xampp\htdocs\boulders -Target C:\dev\Git\bi-ba-boulder\fileshare``
3. start the Devtools
4. Now you can download the files with ``http://localhost/boulders/...`` (example ``http://localhost/boulders/bimano/bimano_high.glb``)

# Starting the Devtools
1. start xampp
2. npm start
3. in a second terminal `cd .\backend\symfony-project\` and then `symfony server:start`

# Deployment
The deployment works over Github Actions  
The .htaccess won't be copied you have to do that on your own.

# Postman
I've added a Postman collection to test some endpoints.   
For emails I use Papercut [https://www.papercut-smtp.com/](https://www.papercut-smtp.com/)