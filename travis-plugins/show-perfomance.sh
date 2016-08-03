#!/bin/bash
END=40
echo ""
echo "[t][tr][th]Threads[/th][th]Perfomance[/th][/tr]"
for ((i=1;i<=END;i++)); do
    perf=$(sysbench --test=cpu --cpu-max-prime=20000 --max-time=1 --num-threads=$i run | grep "total number of events" | awk '{print $NF}')
    printf "  [tr][td] %5s [/td] [td] %10s [/td][/tr]\n" "$i" "$perf"
done
echo "[/t]"
echo ""

