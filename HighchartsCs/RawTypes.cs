using System.Linq;

namespace HighchartsCs
{
    public class MultiAxesTemplateRaw
    {
        public MultiAxesTemplateRaw SetXAxis(string[] categories)
        {
            this.XAxisCategories = categories;
            return this;
        }

        public MultiAxesTemplateRaw SetYAxis(AxisRaw[] yAxis)
        {
            YAxis = yAxis;
            return this;
        }

        public MultiAxesTemplateRaw AddSeries(SeriesRaw series)
        {
            if (Series == null || Series.Any() == false)
            {
                Series = new[] {series};
            }
            else
            {
                var list = Series.ToList();
                list.Add(series);
                Series = list.ToArray();
            }
            return this;
        }
        public string TitleText { get; set; }
        public string SubtitleText { get; set; }
        public string[] XAxisCategories { get; set; } 
        public AxisRaw[] YAxis { get; set; }
        public SeriesRaw[] Series { get; set; }
    }

    public class AxisRaw
    {
        public string LabelsFormat { get; set; }
        public string LabelsStyleColor { get; set; }
        public string TitleText { get; set; }
        public string TitleStyleColor { get; set; }
        public bool? Opposite { get; set; }
        public int GridLineWidth { get; set; }
    }

    public class SeriesRaw
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int YAxis { get; set; }
        public string Data { get; set; }
        public string TooltipValueSuffix { get; set; }
        public bool? MarkerEnabled { get; set; }
        public string DashStyle { get; set; }

    }
}
