cd /tmp; work=/tmp/h3-temp; rm -rf $work; mkdir -p $work; cd $work; git clone https://github.com/devizer/H3Control; \
  cd H3Control; bash teamcity-build.sh; \
  cd H3Control/bin/Debug; mono --debug --desktop H3Control.exe --pid-file=temp.pid --binding=*:5001
