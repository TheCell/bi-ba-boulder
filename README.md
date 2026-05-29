# Bibaboulder

![Build and Deploy](https://github.com/thecell/bi-ba-boulder/actions/workflows/build-and-publish.yml/badge.svg)
[<img alt="Website Deployed for Free with FTP Deploy Action" src="https://img.shields.io/badge/Website deployed for free with-FTP DEPLOY ACTION-%3CCOLOR%3E?style=for-the-badge&color=297FA9">](https://github.com/SamKirkland/FTP-Deploy-Action)


[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-blue.svg
[![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]

Bi Ba Boulder © 2026 by Simon Hischier is licensed under CC BY-NC-SA 4.0. To view a copy of this license, visit https://creativecommons.org/licenses/by-nc-sa/4.0/ 

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
[] Refresh token   
[-] logout   
[-] rate limiting   
[x] upload only while logged in and verified (controller blocks, frontend still shows)   
[] Version Number   
[x] make http failure cases generic   
[] user profile page   
[x] feedback form   
[x] style spraywall list   
[-] filter spraywall list   
[x] reset Create Spraywall after saving   
[] split production and develop   
[-] revert und redo function   
[x] don't rerender when not in active use   
[?] fix crashes on mobile   
[] predefined camera positions and reference character position   
[] check spraywall upload for 1 start 1 top hold   
[] make colors grayscale and changeable in shader   
[] high dpi logo   
[] rock as loading animation    

# helpful resources
Generate interface etc. from json: [https://app.quicktype.io/](https://app.quicktype.io/)

# The Backend
The Backend is built with .NET 10 and MSSQL using EF Core.  
Check out the Readme in the backend_net folder for more information.

# Starting the Devtools
1. npm start
2. in a second terminal `cd backend_net\BiBaBoulder` and then `dotnet run`

# Deployment
The deployment works over Github Actions  
The .htaccess won't be copied you have to do that on your own.

# Bruno
I've added a Bruno collection to test some endpoints.   
For emails I use Papercut [https://www.papercut-smtp.com/](https://www.papercut-smtp.com/)

# Generate the API
We could generate the API on the fly by changing the input line in `openapi-generator-net.yaml` to `inputSpec: http://localhost:5088/openapi/v1.json` but I prefer the written json spec to see changes whenever I start the backend.

1. start the backend to generate the new `backend_net/BiBaBoulder/Thecell.Bibaboulder.BiBaBoulder.json`
```bash
cd C:\dev\git\bi-ba-boulder\backend_net\BiBaBoulder
dotnet run
```
2. Check the OpenApi here during runtime: `http://localhost:5088/openapi/v1.json`
3. run the generate npm package
```bash
cd C:\dev\git\bi-ba-boulder
npm run generate:api-net
```