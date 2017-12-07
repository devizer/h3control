@echo off || echo 
git config credential.helper store

echo.
echo ***************** PULL ********************
git pull

echo.
echo ********** ADD --all and COMMIT **********
git add --all
git commit -am "updated"

echo.
echo **************** PUSH *********************
git push

echo.
echo *************** STATUS ********************
git status
