pushd packages/NUnit.ConsoleRunner.*/tools; export RUNNER_PATH=$(pwd); popd; echo RUNNER_PATH: $RUNNER_PATH;
mono --debug --desktop $RUNNER_PATH/nunit3-console.exe --labels=On --workers=1 --work=H3Control.Tests/bin/Debug H3Control.Tests/bin/Debug/H3Control.Tests.dll
echo -e "\n"; echo TESTS RAN ON: `mono --version | head -1`
