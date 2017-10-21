call Prepare-Nuget-and-Build-Tools.cmd
"%BUILD_TOOLS_ONLINE%\nuget.exe" restore
"%MS_BUILD_2017%" H3Control.sln /t:Rebuild /v:m /p:Configuration=Debug
pushd H3Control.Tests\bin\Debug
"%NUNIT_CONSOLE_RUNNER%" --labels=On --workers=1 H3Control.Tests.dll
"%REPORT_UNIT%" .\ 1>report_uit.log 2>&1
popd
