﻿;
var gauge_CpuFreq;          // #cpuContainer
var label_CpuFreq;          // #cpuValue
var label_CpuRange;         // #cpuLimits

function cpuFreq_OnReady() {

    gauge_CpuFreq = $("#cpuContainer");
    label_CpuRange = $("#cpuLimits");
    label_CpuFreq = $("#cpuValue");

    gauge_CpuFreq.jqxGauge({
        ranges: [
            { startValue: 100, endValue: 700, style: { fill: '#4bb648', stroke: '#4bb648' }, endWidth: 5, startWidth: 1 },
            { startValue: 700, endValue: 1000, style: { fill: '#fbd109', stroke: '#fbd109' }, endWidth: 10, startWidth: 5 },
            { startValue: 1000, endValue: 1300, style: { fill: '#ff8000', stroke: '#ff8000' }, endWidth: 13, startWidth: 10 },
            { startValue: 1300, endValue: 1600, style: { fill: '#e02629', stroke: '#e02629' }, endWidth: 16, startWidth: 13 }
        ],
        ticksMinor: { interval: 100, size: '4%' },
        ticksMajor: { interval: 200, size: '8%' },
        width: 259,
        height: 259,
        min: 100,
        max: 1600,
        value: 10,
        colorScheme: 'scheme05',
        animationDuration: 700,
        labels: {
            distance: '43%',
            position: 'none',
            interval: 200,
            offset: [0, -10],
            visible: true,
            formatValue: function (value) { return value + ""; }
        }
    });

    gauge_CpuFreq.on('valueChanging', function (e) {
        label_CpuFreq.html("CPU: <b>" + Math.round(e.args.value) + '</b> MHz');
    });

    gauge_CpuFreq.jqxGauge('value', 50);

}
