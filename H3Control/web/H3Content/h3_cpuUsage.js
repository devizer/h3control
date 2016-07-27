;

var div_CpuUsage;           // #cpuUsageSample
var panel_CpuUsage;         // #cpuUsageContainer
var UsageStyles = ['UserTimeStyle', 'SystemTimeStyle', 'IdleTimeStyle'];


function cpuUsage_OnReady() {

    panel_CpuUsage = $("#cpuUsageContainer");
    div_CpuUsage = $("#cpuUsageSample");


    // panel_CpuUsage.hide();
    panel_CpuUsage.show();
    BindCpuUsage_Disconnected(div_CpuUsage, "Looking forward for ...");

}

function BindCpuUsage_Disconnected(el, label) {
    var parentWidth = $(el).parent().width();
    var w = parentWidth;
    var sectionLabel = label || "H3-board is disconnected";
    var allClasses = "CpuUsageSection CpuUsageSection-WithLabel CpuUsageSection-Disconnected";
    var htm = "<div style='width: " + w + "px;' "
        + "class='" + allClasses + "'>"
        + sectionLabel
        + "</div>";

    el.html(htm);
    el.width(parentWidth);
    el.show();
    // alert(htm);
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
        if (!isLast) sectionClass += " CpuUsageSection-NonLast";
        var sectionLabel = (w < minWidth) ? "&nbsp;" : ((Math.round(item.Value * 10) / 10) + "%");
        var allClasses = "CpuUsageSection " + UsageStyles[i] + " " + UsageStyles[i] + "Foreground " + sectionClass;
        htm += "<div style='width: " + (w + (isLast ? 0 : 0)) + "px;' "
            + "class='" + allClasses + "'>"
            + sectionLabel
            + "</div>";
    }

    el.html(htm);
    el.width(wTotal);
    el.show();
    // alert(htm);
}


