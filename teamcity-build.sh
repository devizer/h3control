#!/bin/bash
set -e
echo MY DIR is `pwd`
nuget=nuget3; (command -v $nuget 1>/dev/null 2>&1) || nuget=nuget
$nuget | head -n 1
# printenv
(rm -rf packages || true)
$nuget restore H3Control.sln
pushd packages/NUnit.ConsoleRunner.*/tools; export RUNNER_PATH=$(pwd); popd; echo RUNNER_PATH: $RUNNER_PATH;
xbuild H3Control.sln /t:Rebuild /p:Configuration=Debug /verbosity:minimal
pushd H3Control.Tests/bin/Debug
mono --debug --desktop $RUNNER_PATH/nunit3-console.exe --labels=On --workers=1 H3Control.Tests.dll
popd
# bash H3Control.tests.sh
