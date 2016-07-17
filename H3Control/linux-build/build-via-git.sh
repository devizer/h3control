#!/bin/bash
set -e
nuget2=$HOME/bin/NuGet-2.8.6.exe
if [ ! -f $nuget2 ]; then
    wget -O $nuget3 https://dist.nuget.org/win-x86-commandline/v2.8.6/nuget.exe
fi

nuget3=$HOME/bin/NuGet-3.4.4.exe
if [ ! -f $nuget3 ]; then
    wget -O $nuget3 https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
fi


mkdir -p ~/.build
cd ~/.build
rm -rf h3control
git clone https://github.com/devizer/h3control.git

# root of repo
cd h3control
src=/m/v/_GIT/h3control/packages
cp -R $src . 
rm -rf H3Control/{bin,obj}

time ( xbuild H3Control.sln /t:Rebuild /p:Configuration=Release /verbosity:normal )



