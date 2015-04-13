using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighchartsCs
{
    public class MultiAxesTemplateRaw
    {
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
