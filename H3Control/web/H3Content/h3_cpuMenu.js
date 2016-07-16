;

var cpuValues = [240, 312, 408, 480, 720, 1008, 1104, 1200, 1344, 1440, 1536, 1728];
cpuValues = [60, 240, 408, 480, 720, 1008, 1104, 1200, 1344, 1440, 1536, 1728];
var ddrValues = [
    408, 432, 456, 480,
    504, 528, 552, 576,
    600, 624, 648, 672
];
var cpuMinSelected = 0;
var cpuMaxSelected = 10000;
var ddrSelected = -1;

var menu_CpuMax;            // #cpuMaxMenu
var menu_CpuMix;            // #cpuMinMenu
var menu_Ddr;               // #ddrMenu
var menuList_Cpu;           // .cpuMenu

function cpuMenu_OnReady() {

    menu_CpuMax = $("#cpuMaxMenu");
    menu_CpuMix = $("#cpuMinMenu");
    menu_Ddr = $("#ddrMenu");
    menuList_Cpu = $(".cpuMenu");

    $("#cpuMinExpander, #cpuMaxExpander, #ddrExpander").jqxExpander({
        width: 110,
        height: 357,
        theme: 'arctic',
        showArrow: false,
        toggleMode: 'none'
    });

    $("#ddrExpander").jqxExpander("width", 90);




    menuList_Cpu.jqxMenu({ width: '106', mode: 'vertical', source: [] });
    BindCpuMenu();
    BindDdrMenu();
    menuList_Cpu.css('visibility', 'visible');
    menuList_Cpu.css('border', 'none');
    $("#ddrMenu").jqxMenu("width", "86");

    var cpuOnly = $("#cpuMinExpander, #cpuMaxExpander");
    cpuOnly.on('itemclick', function (event) {
        var arr = event.args.id.toString().split("_");
        var idMenu = arr[0];
        var freq = arr[1];
        var prevMax = cpuMaxSelected;
        var prevMin = cpuMinSelected;
        // alert("{" + idMenu + ': ' + freq + "}");
        if (idMenu == "min")
            cpuMinSelected = freq;
        else
            cpuMaxSelected = freq;

        BindCpuMenu();

        var req = $.ajax("api/control/cpu-" + idMenu + "/" + freq, { method: "POST" });
        forceRefreshBySomeClick(req);
        req.fail(function (jqXHR, textStatus, error) {
            cpuMinSelected = prevMin;
            cpuMaxSelected = prevMax;
            BindCpuMenu();
            // alert("changing freq failed: " + error + ", " + textStatus + ", " + jqXHR);

        });
    });

    $("#ddrExpander").on('itemclick', function (event) {
        var arr = event.args.id.toString().split("_");
        var idMenu = arr[0];
        var freq = arr[1];
        var prev = ddrSelected;
        ddrSelected = freq;
        BindDdrMenu();

        var req = $.ajax("api/control/ddr/" + freq, { method: "POST" });
        forceRefreshBySomeClick(req);
        req.fail(function (jqXHR, textStatus, error) {
            ddrSelected = prev;
            BindDdrMenu();
        });
    });

}

function BindDdrMenu() {
    var source = [];
    for (var i = 0; i < ddrValues.length; i++) {
        var mhz = ddrValues[i];
        var selectedHtml = "<b>&rarr;<span style='width:60px; border: 1px dotted grey; height: 100%'>&nbsp;" + mhz + "&nbsp;</span>&larr;</b>";

        var normalHtml = '' + mhz;
        source[i] = {
            /*label: mhz,*/ value: mhz,
            id: 'ddr_' + mhz,
            disabled: false,
            html: (mhz == ddrSelected ? selectedHtml : normalHtml)
        };
    }

    $("#ddrMenu").jqxMenu("source", source);
}

function BindCpuMenu() {
    var cpuBindingSourceMin = [];
    var cpuBindingSourceMax = [];
    for (var i = 0; i < cpuValues.length; i++) {
        var mhz = cpuValues[i];
        var selectedHtml = "<b>&rarr;<span style='width:80px; border: 1px dotted grey; height: 100%'>&nbsp;" + mhz + "&nbsp;</span>&larr;</b>";

        // MIN item
        var isDisabled = (mhz > cpuMaxSelected);
        var normalHtml = '' + mhz;
        if (isDisabled) normalHtml = "<span style='color: #A0A0A0'>" + mhz + "</span>";
        cpuBindingSourceMin[i] = {
            /*label: mhz,*/ value: mhz,
            id: 'min_' + mhz,
            disabled: isDisabled,
            html: (mhz == cpuMinSelected ? selectedHtml : normalHtml)
        };

        // MAX item
        isDisabled = (mhz < cpuMinSelected);
        normalHtml = '' + mhz;
        if (isDisabled) normalHtml = "<span style='color: #A0A0A0'>" + mhz + "</span>";
        cpuBindingSourceMax[i] = {
            /*label: mhz,*/ value: mhz,
            id: 'max_' + mhz,
            disabled: isDisabled,
            html: ((mhz == cpuMaxSelected) ? selectedHtml : normalHtml)
        };
    }

    $("#cpuMinMenu").jqxMenu("source", cpuBindingSourceMin);
    $("#cpuMaxMenu").jqxMenu("source", cpuBindingSourceMax);

}
