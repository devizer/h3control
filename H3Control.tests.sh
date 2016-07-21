mono --debug --desktop packages/NUnit.ConsoleRunner.3.4.1/tools/nunit3-console.exe --labels=On --workers=1 --work=H3Control.Tests/bin/Debug H3Control.Tests/bin/Debug/H3Control.Tests.dll
echo
echo TESTS RAN ON: `mono --version | head -1`