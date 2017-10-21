call Prepare-Nuget-and-Build-Tools.cmd
"%BUILD_TOOLS_ONLINE%\nuget.exe" restore
"%MS_BUILD_2017%" H3Control.sln /t:Rebuild /v:m /p:Configuration=Debug
"%NUNIT_CONSOLE_RUNNER%" --labels=On --workers=1 --work=H3Control.Tests\bin\Debug H3Control.Tests\bin\Debug\H3Control.Tests.dll
