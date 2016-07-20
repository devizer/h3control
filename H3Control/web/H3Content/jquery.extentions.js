if (window && window.jQuery) {
    (function(jQuery) {
        jQuery.fn.applyDisplay = function(isVisible) {
            // console.log("applyDisplay(isVisible): " + isVisible);
            var log = "";
            var ret = this.each(function () {
                var i = $(this);
                if (isVisible) {
                    i.show();
                } else {
                    i.hide();
                }

                var id = i.attr('id');
                var hasId = id !== undefined;
                var p = (!hasId) && this.parentNode ? (this.parentNode.tagName + (this.parentNode.id !== undefined && this.parentNode.id != "" ? ('#' + this.parentNode.id) : '') + ' > ') : '';
                log = log
                    + (log.length === 0 ? '' : ', ')
                    + "'"
                    + p
                    + this.tagName
                    + (id ? ('#' + id) : '')
                    + "'";
            });


            console.log("applyDisplay(isVisible=" + isVisible + ") " + (isVisible ? " " : "") + "on: " + log);
            return ret;
        };
    }(jQuery));
}

if (window) {
    if (!window.console)
        window.console = {
            log: function() {}
        }

    if (!window.console.log) {
        window.console.log = function() {
        }
    }
} else {
    console = {
        log: function () {
        }
    }
}
