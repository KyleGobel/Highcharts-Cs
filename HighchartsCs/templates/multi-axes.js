{
    chart: {
            zoomType: 'xy'
    },
    title: {
            text: {{TitleText}}
    },
    subtitle: {
            text: {{SubtitleText}} 
    },
    credits: {
        enabled : false
    },
    xAxis: [{
        categories: [{{XAxisCategories}}],
        crosshair: true
    }],
    yAxis: [{{YAxis}}],
    tooltip: {
        shared: true
    },
    legend: {
            layout: 'vertical',
            align: 'left',
            x: 80,
            verticalAlign: 'top',
            y: 55,
            floating: true,
            backgroundColor: (Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'
    },
    series: [{{Series}}]
}