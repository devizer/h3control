rem taskkill /f /im h3control.exe
rd /q /s H3Control\bin
rd /q /s H3Control\obj
rd /q /s H3Control.Tests\bin
rd /q /s H3Control.Tests\obj
set /p Build=<H3Control\linux-build\_build-number.txt
set /p Version=<H3Control\linux-build\_version-number.txt 
for /f %%i in ('datetime local') do set datetime=%%i
"C:\Program Files\7-Zip\7zG.exe" a -t7z -mx=9 -mfb=128 -md=128m -ms=on -xr!.git ^
  "C:\Users\Backups on Google Drive\H3Control (%datetime%) %Version%.%Build%.7z" .
