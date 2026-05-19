function initApexChartsColumnChart(containerId, title, subtitle, yAxisTitle, categories, series) {
    var options = {
        title: {
            text: title,
            align: 'center'
        },
        subtitle: {
            text: subtitle,
            align: 'center'
        },
        chart: {
            type: 'bar',
            height: 500
        },
        plotOptions: {
            bar: {
                horizontal: false,
                columnWidth: '55%',
                endingShape: 'rounded'
            },
        },
        dataLabels: {
            enabled: false
        },
        stroke: {
            show: true,
            width: 2,
            colors: ['transparent']
        },
        xaxis: {
            categories: categories,
        },
        yaxis: {
            title: {
                text: yAxisTitle
            }
        },
        series: series,
        fill: {
            opacity: 1
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val + " mm"
                }
            }
        }
    };

    var chart = new ApexCharts(document.querySelector("#" + containerId), options);
    chart.render();
}
