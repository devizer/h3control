# h3control [![travis status](https://travis-ci.org/devizer/h3control.svg?branch=master)](https://travis-ci.org/devizer/h3control)  <img src='https://github.com/devizer/h3control-bin/blob/master/public/status.png?raw=true' width='199' height='32' style='float: right' alt='public' title='public'></img><img src='https://github.com/devizer/h3control-bin/blob/master/staging/status.png?raw=true' width='199' height='32' style='float: right' alt='staging' title='staging'></img>
h3control is a console/daemon for H3 based PI boards. It displays temperature, frequency and usage via built-in http server. It allows to control min/max cpu and ddr frequency. This repository holds installers of h3control only

### installation
Short answer:
```bash
wget -q -nv -O - https://github.com/devizer/h3control-bin/raw/master/public/h3control.sh | bash
```
This installer also suitable for upgrade.  More installation options, for example h3control-daemon which starts during boot, are described on ![h3control-bin repository](https://github.com/devizer/h3control-bin/blob/master/README.md)

### what does **build passed** mean
- Latest source push compiled
- Tests passed. During tests h3control http server is instantiated **twice** using random port and API is called via http. First, h3control server is automatically tested using mono 4.4.1 runtime (latest mono). Second round - the same tests using the oldest mono: 3.2.8

### what does public and staging banners means.
Staging version is built automatically. Ususally it works fine, but it MAY not be tested. Thats why staging build isnt recommended for download/upgrade.

Public build - is a copy of corresponding staging build after some manual tests on real board running Ubuntu 15.04. Rarely i test staging or public builds on x64 environment using another linux distributions (Fedora 24, OpenSUSE 42 and debian 7)

### configuration
By default h3control listen browser requests on the all IP adresses at port 5000. By default h3control allows full access to CPU and DDR frequency.

IP adresses can by restricted by **white-list**. Also changes of CPU & DDR frequency can be protected by **a password**.

All the options are specified using command line parameters:
```
root@OrangePI ~/bin/h3control $ ./h3control-console.sh --help

H3Control 1.23.573 is a console/daemon which
  * "Displays" temperature, frequency and usage via built-in http server.
  * Allows to control CPU & DDR frequency

  -b, --binding=VALUE         Http binding, e.g. ip:port. Default is *:5000 (asterisk means all IPs)
  -w, --white-list=VALUE      Comma separated IPs. Default or empty arg turns restrictions off
  -g, --generate-pwd=VALUE    Generate password hash (encrypt) and exit
  -p, --password=VALUE        HASH of the desired password
  -v, --version               Show version
  -h, -?, --help              Display this help
  -n, --nologo                Hide logo
```


### Screenshot: h3control just works
![h3control in normal](https://github.com/devizer/h3control/raw/master/images/h3control_v1.21_normal.jpg "h3control in normal")


### Screenshot: h3control in readonly mode
![h3control in readonly mode](https://github.com/devizer/h3control/raw/master/images/h3control_v1.21_readonly.jpg "h3control in readonly mode")


### Screenshot: h3control is offline
![h3control is offline](https://github.com/devizer/h3control/raw/master/images/h3control_v1.21_offline.jpg "h3control is offline")

