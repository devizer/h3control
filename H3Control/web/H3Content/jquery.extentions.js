if (window && window.jQuery) {
    (function (jQuery) {

        jQuery.fn.getElementPath = function() {

            var rightArrowParents = [];
            $(this).parents().addBack().not('html').each(function () {
                var entry = this.tagName.toLowerCase();
                if (this.className) {
                    entry += "." + this.className.replace(/ /g, '.');
                }
                rightArrowParents.push(entry);
            });
            var path = rightArrowParents.join(" ");
            return "'" + path + "'";

        }

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


            // console.log("applyDisplay(isVisible=" + isVisible + ") " + (isVisible ? " " : "") + "on: " + log);
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
    if (!window.console.warn) {
        window.console.warn = function() {
        }
    }
} else {
    console = {
        log: function() {
        },
        warn: function() {
        }
    }
}
