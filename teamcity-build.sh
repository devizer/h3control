#!/bin/bash
set -e
exit 1;
echo MY DIR is `pwd`
printenv
nuget restore H3Control.sln 
xbuild H3Control.sln /t:Rebuild /p:Configuration=Debug /verbosity:minimal
mono --debug --desktop packages/NUnit.ConsoleRunner.3.4.1/tools/nunit3-console.exe --labels=On --workers=1 --work=H3Control.Tests/bin/Debug H3Control.Tests/bin/Debug/H3Control.Tests.dll
# bash H3Control.tests.sh
