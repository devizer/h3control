﻿var appH3 = angular.module('h3control', []);
appH3.filter('unsafe', function ($sce) { return $sce.trustAsHtml; });


appH3.controller('processesCtrl', function ($scope, $http) {

    var ProcessesRefreshRate = 3000;

    var fixProcesses = function (procList, maxCount) {
        while (procList.length < maxCount)
            procList.push({ Pid: null, CpuUsage: null, Rss: null, Size: null, Swapped: null, Args: null });

        while (procList.length > maxCount)
            procList.pop();

        return procList;
    }

    $scope.getHeaderClass = function (column) {
        return column === $scope.order ? "sorted" : "sortable";
    }

    $scope.getSelectedSortMark = function (column) {
        var sign;
        sign = "&#9679;";
        sign = '<span class="glyphicon glyphicon-sort-by-attributes-alt" style="float:right"></span>';
        sign = '<span class="glyphicon glyphicon-arrow-down SelectedSortMark"></span>';
        return column === $scope.order ? sign : "";
    }

    var isNumeric = function (n) {
        if ((typeof n) === "undefined" || n === null) return false;
        return !isNaN(parseFloat(n)) && isFinite(n);
    }


    $scope.reSize = function(delta) {

        $(".PList-resize button").each(function(index) {
            $(this).blur();
        });

        var prev = $scope.topN;
        var next = prev;
        if (isNumeric(delta)) {
            next = parseInt(delta);
        } else if (delta === 'inc')
            next = prev + 1;
        else if (delta === 'dec')
            next = prev - 1;

        if (next > 999) next = 999;
        if (next < 3) next = 3;
        if (next < prev) {
            $scope.topN = next;
            $scope.Processes = fixProcesses($scope.Processes, $scope.topN);
        }
        else if (next > prev) {
            $scope.topN = next;
            $scope.Processes = fixProcesses($scope.Processes, $scope.topN);
            // TODO: roundtrip to 
            $http.get("api/json/processes/by-" + $scope.order + "/top-" + $scope.topN)
                .then(function(response) {
                    var processes = response.data.Processes === null ? [] : response.data.Processes;
                    $scope.Processes = fixProcesses(processes, $scope.topN);
                });

        }

        // this.blur();
    }

    var launchRefresh = function() {
    }

    $scope.changeOrder = function (newOrder) {
        $scope.order = newOrder;
        $http.get("api/json/processes/by-" + $scope.order + "/top-" + $scope.topN)
            .then(function (response) {
                var processes = response.data.Processes === null ? [] : response.data.Processes;
                $scope.Processes = fixProcesses(processes, $scope.topN);
            });
    }

    $scope.formatKb = function (num) {
        return isNumeric(num) ? (Math.round(num / 1000 * 10) / 10).toString() : "&nbsp;";
    }

    $scope.formatSwapped = function (num) {
        if (!isNumeric(num)) return "&nbsp;";
        if (num < 0) return "<span color='gray'></span>";
        var ret = (Math.round(num / 1000 * 10) / 10).toString();
        return ret === "0" ? "&mdash;" : ret;
    }


    $scope.refresh = function () {
        $http.get("api/json/processes/by-" + $scope.order + "/top-" + $scope.topN)
            .then(function (response) {
                var processes = response.data.Processes === null ? [] : response.data.Processes;
                $scope.Processes = fixProcesses(processes, $scope.topN);
                $scope.Visible = true || (processes.length > 0);
                $("#processesCtrl").show();
                window.setTimeout($scope.refresh, ProcessesRefreshRate);
            }, function (response) {
                $scope.Visible = true;
                $scope.Processes = fixProcesses([], $scope.topN);
                $("#processesCtrl").show();
                window.setTimeout($scope.refresh, ProcessesRefreshRate);
            });

    };

    var zero = h3context.Processes;
    /* orderColumn values are from enum PsSortOrder */
    $scope.order = "Rss";
    $scope.topN = 5;
    $scope.Processes = [];
    var hasInitBinding = zero !== undefined;
    if (hasInitBinding) {
        $scope.order = zero.order;
        $scope.topN = zero.topN;
        $scope.Processes = zero.Processes;
        console.log("[Startup] So, initial processes order=" + $scope.order + ", topN=" + $scope.topN + ", Processes.length=" + $scope.Processes.length);
    }

    $scope.Processes = fixProcesses($scope.Processes, $scope.topN);
    $scope.Visible = hasInitBinding;
    if (hasInitBinding) $("#processesCtrl").show();
    window.setTimeout($scope.refresh, hasInitBinding ? ProcessesRefreshRate : 50);
});
