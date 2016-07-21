@echo off || echo 
git config credential.helper store

echo.
echo ***************** PULL ********************
git pull

echo.
echo ********** ADD --all and COMPPIT **********
git add --all
git commit -am "NUnit ConsoleRunner 3.4.1"

echo.
echo **************** PUSH *********************
git push

echo.
echo *************** STATUS ********************
git status
