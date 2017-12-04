#!/bin/bash
set -e
pushd packages/NUnit.ConsoleRunner.*/tools; export RUNNER_PATH=$(pwd); popd; echo RUNNER_PATH: $RUNNER_PATH;
echo MY DIR is `pwd`
nuget=nuget3; (command -v $nuget 1>/dev/null 2>&1) || nuget=nuget
$nuget | head -n 1
# printenv
(rm -rf packages || true)
$nuget restore H3Control.sln
xbuild H3Control.sln /t:Rebuild /p:Configuration=Debug /verbosity:minimal
mono --debug --desktop $RUNNER_PATH/nunit3-console.exe --labels=On --workers=1 --work=H3Control.Tests/bin/Debug H3Control.Tests/bin/Debug/H3Control.Tests.dll
# bash H3Control.tests.sh
