function initApexChartsPieChart(containerId, title, labels, series) {
    var options = {
        title: {
            text: title,
            align: 'center'
        },
        labels: labels,
        series: series,
        chart: {
            type: 'pie',
            height: 400
        },
        responsive: [{
            breakpoint: 800,
            options: {
                chart: {
                    width: 200
                },
                legend: {
                    position: 'bottom'
                }
            }
        }]
    };

    var chart = new ApexCharts(document.querySelector("#" + containerId), options);
    chart.render();
}
