#!/bin/bash
echo MY DIR is `pwd`
printenv
nuget restore H3Control.sln 
xbuild H3Control.sln /t:Rebuild /p:Configuration=Debug /verbosity:minimal
bash H3Control.tests.sh
