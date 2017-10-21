@echo off || echo 
git config credential.helper store

echo.
echo ********** ADD --all and COMPPIT **********
git commit -am "deleted: some files"

echo.
echo **************** PUSH *********************
git push

echo.
echo *************** STATUS ********************
git status
