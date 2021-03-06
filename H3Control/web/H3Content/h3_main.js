﻿
// CACHE of frequently accessed widgets

var gauge_DdrFreq;           // #ddrContainer
var label_DdrFreq;           // #ddrValue
var gauge_Temperature;       // #gauge
var label_Temperature;       // #tempValue
var buttonList_UpdateSpeed;  // .UpdateSpeedButton
var label_DdrRange;          // #ddrLimits
var label_Error;             // #error

function updateGauges(values) {
    $('#cpuContainer').jqxGauge('value', values.CpuCur);
    $('#ddrContainer').jqxGauge('value', values.DdrCur);
    $('#gauge').jqxLinearGauge('value', values.Tempr);
}


$(document).ready(function () {

    $(".PList")
        .on("mouseover", function (eventObject) {

            var target = $(eventObject.target);
            console.log("PList.mouse-over: " + target.getElementPath());

            var td = target.is("td.pid") ? target : target.parents("td.pid");
            console.log("              td: " + td.getElementPath());
            // td.css("background-color", "#89E1FF");
            var idProcess = td.attr("data-process-id");
            if (idProcess)
                td.find(".kill-button").show();

        }).on("mouseout", function (eventObject) {

            var target = $(eventObject.target);
            console.log("PList.mouse-out:  " + target.getElementPath());

            var td = target.is("td.pid") ? target : target.parents("td.pid");
            console.log("             td:  " + td.getElementPath());
            // td.css("background-color", "#FFF");
            td.find(".kill-button").hide();

        });

    $(".PList").on("click", function (eventObject) {
        var target = $(eventObject.target);
        if (!target.hasClass("kill-button")) return;
        console.log("PList.kill-click: " + target.getElementPath());
        var td = target.is("td.pid") ? target : target.parents("td.pid");
        var idProcess = td.attr("data-process-id");
        if (idProcess) {
            // td.find(".kill-button").show();
            console.log("Killing process: " + idProcess);
            td.parent().find("td").css("color", "#DDDDDD").html("&nbsp;");
        }

    });

    

    // First, initializing non-jqwidgets
    $("#BrowserInfo").text(BrowserDetect.browser + ' v:' + BrowserDetect.version);

    $("#labelNewVersion").text("n/a");
    $("#feedback_close").on('click', function (event) {
        $('#feedback_popup').popup("hide");
    });

    $(".Restricted").hide();
    BindEmptyMemoryUsage();

    $("#newVersionInfo, #oldVersionInfo").on('click', function (event) {
        LaunchWatsNewLoader();
        $('#whatsnew_popup').popup({
            transition: 'all 0.3s',
            pagecontainer: ".container"
        });
        $('#whatsnew_popup').popup("show");
        // alert('#newVersionInfo');
    });

    cpuUsage_OnReady();

    // Finally, initialize jq-widgets
    cpuMenu_OnReady();
    cpuFreq_OnReady();
    refreshRate_OnReady();
    coresChooser_OnReady();

    gauge_DdrFreq = $("#ddrContainer");
    label_DdrRange = $("#ddrLimits");
    label_DdrFreq = $("#ddrValue");

    gauge_Temperature = $("#gauge");
    buttonList_UpdateSpeed = $(".UpdateSpeedButton");
    label_Error = $("#error");
    label_Temperature = $("#tempValue");

    // DDR
    gauge_DdrFreq.jqxGauge({
        ranges: [
            { startValue: 400, endValue: 500, style: { fill: '#4bb648', stroke: '#4bb648' }, endWidth: 4, startWidth: 2 },
            { startValue: 500, endValue: 600, style: { fill: '#fbd109', stroke: '#fbd109' }, endWidth: 7, startWidth: 4 },
            { startValue: 600, endValue: 700, style: { fill: '#ff8000', stroke: '#ff8000' }, endWidth: 10, startWidth: 7 }
        ],
        ticksMinor: { interval: 10, size: '5%' },
        ticksMajor: { interval: 50, size: '9%' },
        width: 259,
        height: 259,
        min: 400,
        max: 700,
        value: 380,
        colorScheme: 'scheme05',
        animationDuration: h3context.GuageAnimationDuration,
        labels: {
            distance: '43%',
            position: 'none',
            interval: 50,
            offset: [0, -10],
            visible: true,
            formatValue: function (value) { return value + ""; }
        }
    });
    gauge_DdrFreq.on('valueChanging', function (e) {
        var cur_Ddr_Label_Text = "DDR: <b>" + Math.round(e.args.value) + '</b> MHz';
        if (!h3context.DeviceInfo.CanManageDdrFreq)
            cur_Ddr_Label_Text = "DDR (MHz)";

        label_DdrFreq.html(cur_Ddr_Label_Text);
    });
    gauge_DdrFreq.jqxGauge('value', 400);

    // TEMPERETURE
    var majorTicks = { size: '10%', interval: 10 },
        minorTicks = { size: '5%', interval: 5, style: { 'stroke-width': 1, stroke: '#aaaaaa' } },
        labels = {
            interval: 20 /* ° */
        };

    gauge_Temperature.jqxLinearGauge({
        labels: labels,
        ticksMajor: majorTicks,
        ticksMinor: minorTicks,
        min: 0,
        max: 120,
        value: 20,
        pointer: { size: '10%' },
        colorScheme: 'scheme05',
        animationDuration: h3context.GuageAnimationDuration,
        width: 930,
        height: 100,
        orientation: 'horizontal',
        rangeSize: '2%',
        ranges: [
            { startValue: 0, endValue: 70, style: { fill: '#FFF157', stroke: '#FFF157' } },
            { startValue: 70, endValue: 90, style: { fill: '#FFA200', stroke: '#FFA200' } },
            { startValue: 90, endValue: 120, style: { fill: '#FF4800', stroke: '#FF4800' } }
        ]
    });
    gauge_Temperature.jqxLinearGauge('value', 40);
    gauge_Temperature.on('valueChanging', function (e) {
        label_Temperature.html("TEMPERATURE: <b>" + Math.round(e.args.value) + "</b>°");
    });


    $("#newVersionNotification").jqxNotification({
        width: "370px",
        position: "top-center",
        opacity: 0.7,
        autoOpen: true,
        autoClose: false,
        template: "mail"
    });

    var memUsageSource = [
        { Kind: 'Cache', Mem: 587 },
        { Kind: 'Used', Mem: 175 },
        { Kind: 'Free', Mem: 238 },
    ];

    var swapUsageSource = [
        { Kind: '', Mem: 0 },
        { Kind: 'Used', Mem: 175 },
        { Kind: 'Free', Mem: 825 },
    ];

/*
    var settings = {
        title: "",
        description: "",
        enableAnimations: false,
        showLegend: false,
        showBorderLine: false,
        legendPosition: { left: 520, top: 140, width: 100, height: 100 },
        padding: { left: 5, top: 5, right: 5, bottom: 5 },
        titlePadding: { left: 0, top: 20, right: 0, bottom: 10 },
        source: memUsageSource,
        colorScheme: 'scheme01',
        seriesGroups:
        [
            {
                type: 'donut',
                showLabels: false,
                series: [
                    {
                        dataField: 'Mem',
                        displayText: 'Kind',
                        labelRadius: 35,
                        initialAngle: 0,
                        radius: 60,
                        innerRadius: 30,
                        centerOffset: 0,
                        formatSettings: { sufix: '', decimalPlaces: 0 }
                    }
                ]
            }
        ]
    };
    // setup the chart
    $('#memChart, #swapChart').jqxChart(settings);
    $('#swapChart').jqxChart("source", swapUsageSource);
*/

    // INIT RATE ME
    $("#rating").jqxRating({
        width: 85,
        height: 17,
        itemWidth: 17,
        itemHeight: 17,
        theme: 'classic'
    });
    $("#rating").on('change', function (event) {
        var v = parseInt(event.value);
        if (v > 0) {
            // alert(v);
            // console.log("rate V=" + v);
            var answers = ["What the hell?", "Take another short", "Really?", "You are not bad today", "You are genius"];
            $("#feedback_reply").html(answers[v - 1]);
            $("#feedback_close_text").text(v == 5 ? "OK" : "Try again");

            $('#feedback_popup').popup({
                transition: 'all 0.3s',
                pagecontainer: ".container"
            });
            $('#feedback_popup').popup("show");
            if (v != 5) $("#rating").jqxRating("setValue", 0);
        }
    });
    $("#rating_panel").show();



    var dinfo = h3context.DeviceInfo;
    if (dinfo !== undefined && dinfo.IsSuccess) {
        window.setTimeout(bindInitialDeviceInfo, 600);
        window.setTimeout(nextNeverendingUpdate, 1600);
    } else {
        window.setTimeout(nextNeverendingUpdate, 500);
    }

});

function LaunchWatsNewLoader() {
    var loading = $("#whatsnew_loading");
    var content = $("#whatsnew_html");
    loading.html("formatting markdown ...");
    loading.show();
    content.hide();

    var req = jQuery.ajax({
        url: "whatsnew/html-include?" + $.now(),
        method: "GET",
        dataType: "html"
    });

    req.done(function (data) {
        content.html(data);
        loading.hide();
        content.show();
    });

    req.fail(function (jqXHR, textStatus) {
        loading.html("<a href='https://github.com/devizer/h3control-bin/blob/master/WHATS-NEW.md' target='_blank'>WHATS-NEW.md</a>");
    });

}


function BindEmptyMemoryUsage() {
    var usage3 = [
        { Label: "TOTAL MEMORY", Value: null },
        { Label: "FREE MEMORY", Value: null },
        { Label: "BUFFERS &amp; CACHE", Value: null },
        { Label: "AVAILABLE MEM", Value: null },
        { Label: 'SWAP TOTAL', Value: null },
        { Label: 'SWAP USED', Value: null }
    ];

    BindMemoryUsage(usage3);
}

function BindMemoryUsage(mem) {
    var html = '';
    var template = ''
        + '<div class="InfoBlock-Div ${CLASS}">'
        + '<div class="InfoBlock-Label truncate">${LABEL}</div>'
        + '<div class="InfoBlock-Value truncate">${VALUE}</div>'
        + '</div>';

    for (var i = 0; i < mem.length; i++) {
        var item = mem[i];
        var str = "&nbsp;";
        var cls = item.Label.indexOf("BUFFERS") >= 0 ? "flush-cache" : "";
        if (item.Value !== null) {
            var mb = Math.round(item.Value / 1024);
            str = '<span class="InfoBlock-Mb">&mdash;</span>';
            if (mb != 0)
                str = '<span class="InfoBlock-Mb MyNumber">' + mb + '</span>&nbsp;<small>Mb</small>';
        }

        var htmlItem = template.replace("${LABEL}", item.Label).replace("${VALUE}", str).replace("${CLASS}", cls);
        html = html + htmlItem;
    }

    var memUsage = $("#memUsage");
    memUsage.show();
    memUsage.html(html);

    $(".flush-cache").on('click', function (event) {
        var req = jQuery.ajax({
            url: "flush/kernel/cache",
            method: "POST",
            dataType: "json"
        });

        LaunchFlushBuffers();
    });

}

function LaunchFlushBuffers() {

    var divFlush = $('#flushing_kernel_popup');
    divFlush.popup({
        transition: 'all 0.3s',
        pagecontainer: ".container"
    });
    divFlush.popup("show");
    console.log("flushing cache animation: " + divFlush.getElementPath());

    var content = $("#flushing_kernel_popup_content");
    var content0 = content[0];
    var anchor = {};
    jQuery.data(content0, "anchor", anchor);
    var text = "flushing kernel buffers ";
    var n = 0;
    var callBack;
    var startAt = +new Date();
    callBack = function () {
        if (anchor === jQuery.data(content0, "anchor")) {
            n++;
            text += "<span style='font-size:1px;'> </span>.";

            content.html(text);
            // console.log(text);
            var now = +new Date();
            var itsTime = now - startAt > 7000;
            if (n === 500 || itsTime) {
                divFlush.popup("hide");
            } else {
                window.setTimeout(callBack, 1);
            }
        }
    }

    callBack();
}

function BindHasAccess(hasAccess) {
    if (hasAccess === null)
        return;

    $(".Restricted").applyDisplay(!hasAccess);

    var isIE8 = (BrowserDetect.browser === "Explorer" && BrowserDetect.version <= 8.9);
    if (isIE8) {
        $(".Restricted").hide();
        $("#cpuMaxMenu").jqxMenu('disabled', !hasAccess);
        $("#cpuMinMenu").jqxMenu('disabled', !hasAccess);
    }

    $("#ddrMenu").jqxMenu('disabled', !hasAccess);
}
