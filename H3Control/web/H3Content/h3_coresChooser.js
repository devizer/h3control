;

var _CanNotManageOnlineCores_WarningShowed = false;
function coresChooser_OnReady() {

    $(".single-core-container").on("click", function (eventObject) {
        var target = $(eventObject.target);
        var button = target.is(".single-core-container") ? target : target.parents(".single-core-container");
        var coresNumber = button.attr("data-id");
        console.log("CORES CHOOSER: " + coresNumber + " core(s)");
        bind_OnlineCores(coresNumber);

        if (!h3context.DeviceInfo.CanManageOnlineCores) {
            if (!_CanNotManageOnlineCores_WarningShowed) {
                _CanNotManageOnlineCores_WarningShowed = true;
                alert("The kernel of the '" + h3context.DeviceInfo.Host + "' does not allow to manage online cores");
            }
        }

        var postUri = "api/control/set-cores/" + coresNumber;
        var req = jQuery.ajax({
            url: postUri,
            method: "POST",
            dataType: "json"
        });

        forceRefreshBySomeClick(req);

        req.fail(function (jqXHR, textStatus) {
            // nothing to do
        });

    });

    if (h3context && h3context.DeviceInfo && h3context.DeviceInfo.OnlineCoresNumber) {
        // console.log("STARTUP: OnlineCoresNumber is " + h3context.DeviceInfo.OnlineCoresNumber);
        bind_OnlineCores(h3context.DeviceInfo.OnlineCoresNumber);
    }
}

function bind_OnlineCores(coresCount) {
    for (var i = 1; i <= 4; i++) {
        var container = $("#core-" + i);
        var suffix = i <= coresCount ? "on" : "off";
        var prefix = "single-core-";
        container.removeClass(prefix + "on");
        container.removeClass(prefix + "off");
        container.addClass(prefix + suffix);
    }
}


