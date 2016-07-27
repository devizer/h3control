:again
set ffl=V:\NoVCS\FirefoxLab\FirefoxLab\bin\Debug\FirefoxLab.exe
%ffl% close
sleep 4
rd /q /s C:\Users\Пользователь\AppData\Local\Mozilla\Firefox\Profiles\ah9felbe.default\cache2
rem C:\Users\username\AppData\Local\Microsoft\Windows\Temporary Internet Files
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?1
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?2 
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?3 
goto skip
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?4 
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?5   
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?6 
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?7 
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?8 
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?9 
start "FireFox h3contol " "C:\Program Files (x86)\Mozilla Firefox\firefox.exe" -new-window http://192.168.0.11:5000/?A

:skip
sleep 35
%ffl% enum 555
%ffl% screenshot V:\NoVCS\FirefoxLab\Screenshots\ h3control
%ffl% close

goto again