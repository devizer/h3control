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
        var isLast = i === (usage.length - 1);
        if (isLast) w = parentWidth - wTotal;
        if (w < 1) w = 1;
        wTotal += w;
        var minWidth = 60;
        var sectionClass = (w >= minWidth) ? "CpuUsageSection-WithLabel" : "CpuUsageSection-WithoutLabel";
        var sectionLabel = (w < minWidth) ? "&nbsp;" : ((Math.round(item.Value * 10) / 10) + "%");
        var allClasses = "CpuUsageSection " + UsageStyles[i] + " " + sectionClass + " " + UsageStyles[i] + "Foreground " + sectionClass;
        htm += "<div style='width: " + w + "px;' "
            + "class='" + allClasses + "'>"
            + sectionLabel
            + "</div>";
    }

    el.html(htm);
    el.width(wTotal);
    el.show();
    // alert(htm);
}


