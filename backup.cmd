taskkill /f /im h3control.exe
rd /q /s H3Control\bin
rd /q /s H3Control\obj
rem winrar a "C:\Users\���짮��⥫�\Google ���\backup\H3Control (.rar" -s -m5 -agYYYY-MM-DD,HH-MM-SS) -r
set /p Build=<H3Control\linux-build\_build-number.txt
set /p Version=<H3Control\linux-build\_version-number.txt 
for /f %%i in ('datetime local') do set datetime=%%i
"C:\Program Files\7-Zip\7zG.exe" a -t7z -mx=9 -mfb=128 -md=128m -ms=on ^
  "C:\Users\Backups on Google Drive\H3Control (%datetime%) %Version%.%Build%.7z" .
