;
var gauge_CpuFreq;          // #cpuContainer
var label_CpuFreq;          // #cpuValue
var label_CpuRange;         // #cpuLimits

function cpuFreq_OnReady() {

    gauge_CpuFreq = $("#cpuContainer");
    label_CpuRange = $("#cpuLimits");
    label_CpuFreq = $("#cpuValue");

    gauge_CpuFreq.jqxGauge({
        ranges: [
            { startValue: 50, endValue: 700, style: { fill: '#4bb648', stroke: '#4bb648' }, endWidth: 4, startWidth: 2 },
            { startValue: 700, endValue: 1000, style: { fill: '#fbd109', stroke: '#fbd109' }, endWidth: 6, startWidth: 4 },
            { startValue: 1000, endValue: 1300, style: { fill: '#ff8000', stroke: '#ff8000' }, endWidth: 8, startWidth: 6 },
            { startValue: 1300, endValue: 1550, style: { fill: '#e02629', stroke: '#e02629' }, endWidth: 10, startWidth: 8 }
        ],
        ticksMinor: { interval: 125/2, size: '4%' },
        ticksMajor: { interval: 250, size: '8%' },
        width: 259,
        height: 259,
        min: 50,
        max: 1550,
        value: 10,
        colorScheme: 'scheme05',
        animationDuration: h3context.GuageAnimationDuration,
        labels: {
            distance: '43%',
            position: 'none',
            interval: 250,
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
