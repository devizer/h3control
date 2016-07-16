;

var div_CpuUsage;           // #cpuUsageSample
var panel_CpuUsage;         // #cpuUsageContainer
var UsageStyles = ['UserTimeStyle', 'SystemTimeStyle', 'IdleTimeStyle'];


function cpuUsage_OnReady() {

    div_CpuUsage = $("#cpuUsageSample");
    panel_CpuUsage = $("#cpuUsageContainer");
    panel_CpuUsage.hide();

}

function BindCpuUsage(usage, el) {
    var htm = "";
    var wTotal = 0;
    var parentWidth = $(el).parent().width();
    var onePerCentWidth = parentWidth / 100.0;
    for (var i = 0; i < usage.length; i++) {
        var item = usage[i];
        var w = parseInt(Math.round(item.Value * onePerCentWidth));
        if (w < 2) w = 2;
        if (i == usage.length - 1) w = parentWidth - wTotal;
        if (w < 1) w = 1;
        wTotal += w;
        var style2 = "";
        if (i + 1 < usage.length) style2 = "border-right: 1px solid #DDDDDD";
        var labelWidth = 60;
        var sectionClass = (w >= labelWidth) ? "CpuUsageLabel" : "CpuUsageNoLabel";
        var sectionLabel = (w < labelWidth) ? "&nbsp;" : ((Math.round(item.Value * 10) / 10) + "%");
        htm += "<div class='" + UsageStyles[i]
            + " CpuUsageBarHeight " + sectionClass + " " +
            UsageStyles[i] + "Foreground' style='float: left; width: " + w + "px; "
            + style2 + "'>" + sectionLabel + "</div>";
    }

    el.html(htm);
    el.width(wTotal);
    el.show();
    // alert(htm);
}


