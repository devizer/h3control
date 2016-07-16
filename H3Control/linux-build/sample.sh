cd /tmp
wget -nv -O h3control.tar.gz --no-check-certificate 'https://www.dropbox.com/s/o8t38f4yszi06jm/h3control.tar.gz?dl=1'
killall -q mono || echo "stop of h3control is skipped"
rm -rf h3control
tar xzf h3control.tar.gz
cd h3control 
./h3control-console.sh --help
./h3control-console.sh --nologo --binding=*:5000
# enjoy http://orange-pi-plus-ip:5000/ :)
# P.S. mono 3+ is required. 
# Please apt-get install mono-complete or something similar

# for f in *.exe *.dll; do mono --aot $f; strip $f.so; done