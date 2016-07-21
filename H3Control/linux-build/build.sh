#!/bin/bash
set -e
# VER 6

src=/m/v/_GIT/h3control

rm -rf {tmp,dist}
mkdir -p tmp/h3control
time (cp -R $src ./tmp/src)
cp build.sh $src/H3Control/linux-build/build.sh
echo COPIED!
sync
echo 3 | sudo tee /proc/sys/vm/drop_caches >/dev/null

# INCREMENT VERSION
pwd
vers=`cat tmp/src/H3Control/linux-build/_version-number.txt`
build=$(( `cat tmp/src/H3Control/linux-build/_build-number.txt` + 1 ))
fullver=$vers.$build
builddate=$(date --utc +"%a, %d %b %Y %T GMT")
echo $build > tmp/src/H3Control/linux-build/_build-number.txt
echo NEW VERSION IS $fullver
echo $fullver > tmp/h3control/VERSION
echo "
   [assembly: System.Reflection.AssemblyVersion(\"$fullver.0\")]
   [assembly: Universe.AssemblyBuildDateTime(\"$builddate\")]
" > tmp/src/H3Control/Properties/AssemblyVersion.cs

cp tmp/src/H3Control/linux-build/_version-number.txt $src/H3Control/linux-build/_version-number.txt
cp tmp/src/H3Control/linux-build/_build-number.txt $src/H3Control/linux-build/_build-number.txt
cp tmp/src/H3Control/Properties/AssemblyVersion.cs $src/H3Control/Properties/AssemblyVersion.cs
find tmp -exec touch {} \;

# BUILDING
cd tmp/src
rm -rf H3Control/{bin,obj}
export XBUILD_COLORS=errors=brightred,warnings=blue
time (xbuild H3Control.sln /t:Rebuild /p:Configuration=Release /verbosity:normal)
# pdb,mdb
rm -rf H3Control/bin/Release/*.{pdb2,mdb2,xml}
cd ..

# PACK
cp -R src/H3Control/bin/Release h3control/bin
cd h3control/bin
rm -f web/jqwidgets/jqx-all.js
chmod -R 644 .
cd ../..
cp src/H3Control/linux-dist/* h3control
chmod 755 h3control/*.sh
chmod 644 h3control/TROUBLESHOOTING
find . -type d -exec chmod 755 {} \;

mkdir -p ../dist
echo PACK h3control.tar.gz
GZIP=-9 tar czf ../dist/h3control.tar.gz h3control
echo PACK h3control.tar.bz2
tar cjf ../dist/h3control.tar.bz2 h3control

echo COPY tar.gz TO THE CLOUD
cd ../dist
ls h3control.tar.* -l
cd ..
cp dist/h3control.tar.gz /m/c/Users/Пользователь/Dropbox/h3control/work
dt=`date +%s`
echo "{ version: '$fullver', date: $dt }" > /m/c/Users/Пользователь/Dropbox/h3control/work/h3control-version.json

echo ------------------------------
echo H3CONTROL $fullver HAD BUILDED


# restart local console
killall -q mono || echo "stop of h3control is skipped"
rm -rf /ssd/bin/h3control
tar xzf dist/h3control.tar.gz -C /ssd/bin
cd /ssd/bin/h3control
./h3control-console.sh

exit 0
