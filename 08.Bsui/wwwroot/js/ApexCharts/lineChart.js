function initApexChartsLineChart(containerId, title, subtitle, xAxisTitle, xAxisCategories, yAxisTitle, yAxisMin, yAxisMax, series) {
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
            type: 'line',
            height: 500,
            dropShadow: {
                enabled: true,
                color: '#000',
                top: 18,
                left: 7,
                blur: 10,
                opacity: 0.2
            },
            toolbar: {
                show: true
            }
        },
        colors: ['#77B6EA', '#545454'],
        dataLabels: {
            enabled: true,
        },
        stroke: {
            curve: 'smooth'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'],
                opacity: 0.5
            },
        },
        markers: {
            size: 1
        },
        xaxis: {
            categories: xAxisCategories,
            title: {
                text: xAxisTitle
            }
        },
        yaxis: {
            title: {
                text: yAxisTitle
            },
            min: yAxisMin,
            max: yAxisMax
        },
        series: series,
        legend: {
            position: 'right',
            offsetY: 200
        },
        responsive: [{
            breakpoint: 1000,
            options: {
                legend: {
                    position: 'bottom',
                    horizontalAlign: 'center',
                    offsetY: 0
                },
            },
        }]
    };

    var chart = new ApexCharts(document.querySelector("#" + containerId), options);
    chart.render();
}
