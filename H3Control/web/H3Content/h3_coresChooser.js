;

function coresChooser_OnReady() {

    var bind_Cores = function(coresCount) {
        for (var i = 1; i <= 4; i++) {
            var container = $("#core-" + i);
            var suffix = i <= coresCount ? "on" : "off";
            var prefix = "single-core-";
            container.removeClass(prefix + "on");
            container.removeClass(prefix + "off");
            container.addClass(prefix + suffix);
        }
    }


    $(".single-core-container").on("click", function (eventObject) {
        var target = $(eventObject.target);
        var button = target.is(".single-core-container") ? target : target.parents(".single-core-container");
        var coreNumber = button.attr("data-id");
        console.log("CORES CHOOSER: " + coreNumber + " core(s)");
        bind_Cores(coreNumber);
    });

    if (h3context && h3context.DeviceInfo && h3context.DeviceInfo.OnlineCoresNumber) {
        // console.log("STARTUP: OnlineCoresNumber is " + h3context.DeviceInfo.OnlineCoresNumber);
        bind_Cores(h3context.DeviceInfo.OnlineCoresNumber);
    }
}


