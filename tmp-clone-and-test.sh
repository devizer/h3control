cd /tmp
work=/tmp/h3-temp
rm -rf $work
mkdir -p $work
cd $work
git clone https://github.com/devizer/H3Control
cd H3Control
bash teamcity-build.sh
