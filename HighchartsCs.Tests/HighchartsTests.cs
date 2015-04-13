using NUnit.Framework;

namespace HighchartsCs.Tests
{
    public class HighchartsTests
    {
        [Test]
        public void CanMakeAxis()
        {
            var axis = new AxisRaw
            {
                GridLineWidth = 0,
                LabelsFormat = "{value} mb",
                LabelsStyleColor = "Highcharts.getOptions().colors[1]",
                Opposite = true,
                TitleStyleColor = "Highcharts.getOptions().colors[1]",
                TitleText = "Sea-Level Pressure"
            };
            var result = Highcharts.CreateAxis(new[] {axis});

            Assert.NotNull(result);
        }

        [Test]
        public void CanMakeSeries()
        {
            var series = new SeriesRaw
            {
                Data = "[1,2,3,4,5,6,76]",
                Name = "Sea-Level Pressure",
                Type = "spline",
                YAxis = 2,
                DashStyle = "shortdot",
                TooltipValueSuffix = " mb"
            };

            var result = Highcharts.CreateSeries(new[] {series});
            Assert.NotNull(result);
        }
    }
}
