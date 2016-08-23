rem mono packages\NUnit.ConsoleRunner.3.4.1\tools\nunit3-console.exe --labels=On --workers=1 --work=H3Control.Tests\bin\Debug H3Control.Tests\bin\Debug\H3Control.Tests.dll
cd H3Control.Tests\bin\Debug
nunit-console4 --labels=On H3Control.Tests.dll
cd ..\..\..