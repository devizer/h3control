#!/bin/bash

mkdir -p /sys/devices/system/cpu/cpu{0,1,2,3}
mkdir -p /sys/devices/virtual/hwmon/hwmon1
mkdir -p /sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq


echo 1200000 > /sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq
echo 47 >/sys/devices/virtual/hwmon/hwmon1/temp1_input
echo 672000 >/sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq/cur_freq



echo 1576000 >/sys/devices/system/cpu/cpu0/cpufreq/scaling_max_freq
echo 480000 >/sys/devices/system/cpu/cpu0/cpufreq/scaling_min_freq
echo interactive >/sys/devices/system/cpu/cpu0/cpufreeq/scaling_governor


echo 672000 >/sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq/scaling_max_freq
echo 408000 >/sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq/scaling_min_freq
echo userspace >/sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq/governor

