;

function refreshRate_OnReady() {

    // UPDATE SPEED
    $(".UpdateSpeedButton").jqxButton({ theme: 'fresh', /*width: '70', */ template: 'default' });
    $(".UpdateSpeedButton[value='" + h3context.UpdateSpeed + "']").jqxButton("template", 'success');
    $('.UpdateSpeedButton').jqxButton().on('click', function (event) {
        var id = this.id;
        var newVal = $(this).val();
        // alert("id=" + id + ", val()=" + v);
        // var id = event.args.id.toString();
        $(".UpdateSpeedButton").each(function (index, button) {
            var $this = $(this);
            var iVal = $this.val();
            var isSelected = iVal == newVal;
            $this.jqxButton("template", isSelected ? 'success' : 'default');
        });
        var arr = id.split("_");
        var idPrefix = arr[0];
        var msecs = parseInt(arr[1]);
        h3context.UpdateSpeed = msecs;
        $.ajax("api/control/updatespeed/" + msecs, { method: "POST" });
    });

    $("#updateSpeedContainer").show();
}

