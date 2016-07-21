@echo off || echo 
git config credential.helper store

echo.
echo ***************** PULL ********************
git pull

echo.
echo ********** ADD --all and COMPPIT **********
git add --all
git commit -am "try mono 2.11.4"

echo.
echo **************** PUSH *********************
git push

echo.
echo *************** STATUS ********************
git status
