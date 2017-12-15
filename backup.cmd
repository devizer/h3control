pushd "%LOCALAPPDATA%"
echo [System.DateTime]::Now.ToString("yyyy-MM-dd,HH-mm-ss") | powershell -command - > .backup.timestamp
for /f %%i in (.backup.timestamp) do set datetime=%%i
popd

rem taskkill /f /im h3control.exe
rd /q /s H3Control\bin
rd /q /s H3Control\obj
rd /q /s H3Control.Tests\bin
rd /q /s H3Control.Tests\obj
set /p Build=<H3Control\linux-build\_build-number.txt
set /p Version=<H3Control\linux-build\_version-number.txt 
"C:\Program Files\7-Zip\7zG.exe" a -t7z -mx=9 -mfb=128 -md=128m -ms=on -xr!.git -xr!.vs ^
  "C:\Users\Backups on Google Drive\H3Control (%datetime%) %Version%.%Build%.7z" .
