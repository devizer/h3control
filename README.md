# h3control [![travis status](https://travis-ci.org/devizer/h3control.svg?branch=master)](https://travis-ci.org/devizer/h3control)  <img src='https://github.com/devizer/h3control-bin/blob/master/public/status.png?raw=true' width='199' height='32' style='float: right' alt='public' title='public'></img><img src='https://github.com/devizer/h3control-bin/blob/master/staging/status.png?raw=true' width='199' height='32' style='float: right' alt='staging' title='staging'></img>
h3control is a console/daemon for H3 based PI board. It displays temperature, frequency and usage via built-in http server. It allows to control min/max cpu and ddr frequency. This repository holds installers of h3control only

### installation
Short answer:
```bash
wget -q -nv -O - https://github.com/devizer/h3control-bin/raw/master/public/h3control.sh | bash
```
More installation options, for example h3control-daemon, are described on ![h3control-bin repository](https://github.com/devizer/h3control-bin)

### what does **build passed** mean
- Latest source push compiled
- Tests passed. During tests h3control http server is instantiated **twice** using random port and API is called via http. First, h3control server is automatically tested using mono 4.4.1 runtime (latest mono). Second raund - the same using the oldest mono: 3.2.8



### Screenshot: h3control just works
![h3control in normal](https://github.com/devizer/h3control/raw/master/images/h3control_v1.21_normal.jpg "h3control in normal")


### Screenshot: h3control in readonly mode
![h3control in readonly mode](https://github.com/devizer/h3control/raw/master/images/h3control_v1.21_readonly.jpg "h3control in readonly mode")


### Screenshot: h3control is offline
![h3control is offline](https://github.com/devizer/h3control/raw/master/images/h3control_v1.21_offline.jpg "h3control is offline")

