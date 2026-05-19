function initHighchartsLineChart(containerId, title, subtitle, xAxisTitle, xAxisCategories, yAxisTitle, yAxisMin, yAxisMax, series) {
    Highcharts.chart(containerId, {
        title: {
            text: title
        },

        subtitle: {
            text: subtitle
        },

        xAxis: {
            categories: xAxisCategories,
            title: {
                text: xAxisTitle
            }
        },

        yAxis: {
            title: {
                text: yAxisTitle
            },
            min: yAxisMin,
            max: yAxisMax
        },

        legend: {
            layout: 'vertical',
            align: 'right',
            verticalAlign: 'middle'
        },

        plotOptions: {
            series: {
                label: {
                    connectorAllowed: false
                }
                /*pointStart: 2010*/
            }
        },

        series: series,

        responsive: {
            rules: [{
                condition: {
                    maxWidth: 500
                },
                chartOptions: {
                    legend: {
                        layout: 'horizontal',
                        align: 'center',
                        verticalAlign: 'bottom'
                    }
                }
            }]
        }
    });
}
