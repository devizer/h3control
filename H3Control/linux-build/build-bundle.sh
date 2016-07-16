#!/bin/bash
work=`pwd`/h3control-standalone/temp/h3control
mymono=/opt/mono/4.4.1.0
rm -rf "$work"
mkdir -p "$work/bin"
cd h3control/bin
bins=$(ls -1 *.dll *.exe)
echo bins=$bins
MONO_PATH=. mkbundle -c -o /tmp/h3control-static --deps $bins \
  |  grep -E "^   embedding: " | awk '{ print $2 }' > .h3control-list

# we don't need assemblies from h3 folder
old=`pwd`
cd ~
MONO_PATH=. mkbundle -c -o /tmp/mcs-static --deps  $mymono/lib/mono/4.5/mcs.exe \
  |  grep -E "^   embedding: " | awk '{ print $2 }' > $old/.mcs-list
cd $old

cat .h3control-list .mcs-list > .binary-list

cat .binary-list | while read line; do
  if [ -f $line ]; then
    # echo FOUND: $line
    echo "$line" | rev | cut -d"/" -f1 | rev > .tmp1
    f=`cat .tmp1`
    echo "$f"
    cp "$line" "$work/bin"
  fi
done


cp "$mymono/bin/mono-sgen" "$work/bin" 
for file in $mymono/lib/libmonosgen-2.0.so*; do cp -P $file $work/bin ; done
for file in $work/bin/libmonosgen-2.0.so $work/bin/mono-sgen; do strip $file ; done
cp H3Control.exe.config "$work/bin"
cp -R web "$work/bin/web"

# exit 0

cat << GOGO > "$work/h3control-console.sh"
#!/bin/bash
export PATH="/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/usr/games:/usr/local/games:/root/bin"
export PATH="./bin:\$PATH"
mono --version
MONO_PATH=./bin LD_LIBRARY_PATH=./bin bin/mono-sgen --desktop bin/H3Control.exe "\$@"
GOGO
chmod +x "$work/h3control-console.sh"


echo '#!/bin/sh
MONO_PATH=./bin LD_LIBRARY_PATH=./bin bin/mono-sgen --desktop bin/mcs.exe "$@"
' > $work/bin/mcs
chmod +x $work/bin/mcs


cd "$work/.."
echo PACK GZIP
# GZIP=-9 tar czf h3control.tar.gz h3control
GZIP=-9 tar czf - h3control | pv > h3control.tar.gz
echo PACK BZIP
# tar cjf h3control.tar.bz h3control
tar cjf - h3control | pv > h3control.tar.bz2




