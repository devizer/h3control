﻿var myCounter = 0;
var TimeInfo = { PrevStartDate: null, TotalMSec: 0, Counter: 0 };
var isFirstRound = true;
var nextIterAnchor = {};

// doesn't rise nextIter
function forceRefreshBySomeClick(req) {
    req.done(function (data) {
        nextIter(false);
    });
}

// always rise nextIter on both Success && Fail
function nextNeverendingUpdate() {
    nextIter(true);
}

function bindInitialDeviceInfo() {
    var dinfo = h3context.DeviceInfo;
    if (dinfo && dinfo.IsSuccess) {
        bindSuccessDeviceInfo(dinfo);
    }
}

/*  called in two cases:
          - document.onready with h3context.DeviceInfo argument;
          - get(api/json/device/me).Done with data argument                 */
function bindSuccessDeviceInfo(data) {
    updateGauges(data);
    // alert('done');
    gauge_CpuFreq.jqxGauge('disabled', false);
    gauge_DdrFreq.jqxGauge('disabled', false);
    gauge_Temperature.jqxLinearGauge('disabled', false);
    menu_CpuMax.jqxMenu('disabled', false);
    menu_CpuMix.jqxMenu('disabled', false);
    buttonList_UpdateSpeed.jqxButton('disabled', false);


    // Always
    $("#rating_panel").show();

    if (data != null && data.VerInfo != null && data != null && data.VerInfo.NewVer != null) {
        var newVer = data.VerInfo.NewVer;
        var newVerText = newVer.Major + '.' + newVer.Minor + '.' + newVer.Build;
        $("#labelNewVersion").text(newVerText);

        // shit!
        var isNew = data.VerInfo.IsNew !== undefined && data.VerInfo.IsNew;
        $("#whatsnew_there_are_new_version").applyDisplay(isNew);
        $("#whatsnew_its_latest_version").applyDisplay(!isNew);
        // #newVersionInfo, #oldVersionInfo
        $("#newVersionInfo").applyDisplay(isNew);
        $("#oldVersionInfo").applyDisplay(!isNew);

        /*
        if (data.VerInfo.IsNew !== undefined) {
            if (data.VerInfo.IsNew) {
                $("#whatsnew_there_are_new_version").show();
                $("#whatsnew_its_latest_version").hide();
            } else {
                $("#whatsnew_there_are_new_version").hide();
                $("#whatsnew_its_latest_version").show();
            }
        } else {
            $("#whatsnew_there_are_new_version").hide();
            $("#whatsnew_its_latest_version").show();
        }
*/
    }


    if (data.IsLimitSuccess) {
        label_CpuRange.html("<small>RANGE: " + data.CpuMin + " ... " + data.CpuMax + "");
        label_DdrRange.html("<small>RANGE: " + data.DdrMin + " ... " + data.DdrMax + "");

        if (isFirstRound) {
            isFirstRound = false;
            cpuMinSelected = data.CpuMin;
            cpuMaxSelected = data.CpuMax;
            ddrSelected = data.DdrCur;
            BindDdrMenu();
            BindCpuMenu();
        }
    }

    if (data.Cpu && data.Cpu.Total) {
        var cpu1Usage = [
            { Kind: 'User time', Value: data.Cpu.Total.User },
            { Kind: 'System time', Value: data.Cpu.Total.System },
            { Kind: 'Idle time', Value: data.Cpu.Total.Idle }
        ];

        BindCpuUsage(cpu1Usage, div_CpuUsage);
        panel_CpuUsage.show();
    } else {
        panel_CpuUsage.hide();
        console.warn("Warning: data.Cpu && data.Cpu.Total are absent");
    }

    if (data.Mem) {

        var freeMem = data.Mem.Free - data.Mem.BuffersAndCache;
        freeMem = freeMem < 0 ? 0 : freeMem;
        var usage2 = [
            { Label: "TOTAL MEMORY", Value: data.Mem.Total },
            { Label: "FREE MEMORY", Value: freeMem },
            { Label: "BUFFERS &amp; CACHE", Value: data.Mem.BuffersAndCache },
            { Label: "AVAILABLE MEM", Value: data.Mem.Free + data.Mem.Buffers },
            { Label: 'SWAP TOTAL', Value: data.Mem.SwapTotal },
            { Label: 'SWAP USED', Value: data.Mem.SwapTotal - data.Mem.SwapFree }
        ];

        BindMemoryUsage(usage2);

/*
        var memUsageSource = [
            { Kind: 'Cache', Mem: data.Mem.BuffersAndCache },
            { Kind: 'Used', Mem: data.Mem.Total - data.Mem.Free },
            { Kind: 'Free', Mem: data.Mem.Free },
        ];

        $('#memChart').jqxChart("source", memUsageSource);

        var swapUsageSource = [
            { Kind: '', Mem: 0 },
            { Kind: 'Swap Used', Mem: data.Mem.SwapTotal - data.Mem.SwapFree },
            { Kind: 'Swap Free', Mem: data.Mem.SwapFree },
        ];

        $('#swapChart').jqxChart("source", swapUsageSource);
*/

    }

    if (data.HasChangeAccess !== undefined) {
        BindHasAccess(data.HasChangeAccess);
    }

    if (data.OsName) {
        $('#osName').text(data.OsName);
    }

    if (data.OnlineCoresNumber)
        bind_OnlineCores(data.OnlineCoresNumber);
}

function nextIter(isNeverEnding) {

    var next = +new Date();
    var timeInfo = "";
    if (isNeverEnding) {
        if (TimeInfo.PrevStartDate != null) {
            var delta = next - TimeInfo.PrevStartDate;
            TimeInfo.Counter++;
            TimeInfo.TotalMSec += delta;
            timeInfo = " (" + delta + " msec, avg is " + Math.round(TimeInfo.TotalMSec / TimeInfo.Counter) + ")";
        }
        TimeInfo.PrevStartDate = next;
    }

    var anchor = {};
    nextIterAnchor = anchor;
    // var idUnique = new Date().getTime();
    var req = jQuery.ajax({
        url: "api/json/device/me?" + $.now(),
        method: "GET",
        dataType: "json"
    });

    var bindFail = function () {
        $('#cpuLimits').html("");
        $('#ddrLimits').html("");
        $('#cpuContainer').jqxGauge('disabled', true);
        $('#ddrContainer').jqxGauge('disabled', true);
        $('#gauge').jqxLinearGauge('disabled', true);
        $("#cpuMaxMenu").jqxMenu('disabled', true);
        $("#cpuMinMenu").jqxMenu('disabled', true);
        $("#ddrMenu").jqxMenu('disabled', true);
        BindEmptyMemoryUsage();
        $(".UpdateSpeedButton").jqxButton('disabled', true);
        $("#cpuUsageContainer").show();
        BindCpuUsage_Disconnected(div_CpuUsage);
        $("#rating_panel").hide();
        bind_OnlineCores(0);
    };

    req.done(function (data) {
        if (anchor !== nextIterAnchor) return;
        if (data.IsSuccess) {
            if (isNeverEnding)
                label_Error.text(TimeInfo.Counter + ': OK' + timeInfo).show();

            bindSuccessDeviceInfo(data);
            if (isNeverEnding) window.setTimeout(nextNeverendingUpdate, h3context.UpdateSpeed);

        } else {
            if (isNeverEnding) {
                $("#error").text(TimeInfo.Counter + ': internal error' + timeInfo).show();
                bindFail();
                window.setTimeout(nextNeverendingUpdate, h3context.UpdateSpeed);
            }
        }
    });

    req.fail(function (jqXHR, textStatus) {
        if (anchor !== nextIterAnchor) return;
        if (isNeverEnding) {
            $("#error").text(TimeInfo.Counter + ' ' + textStatus + timeInfo).show();
            bindFail();
            window.setTimeout(nextNeverendingUpdate, h3context.UpdateSpeed);
        }
    });

}
