using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Chronos;
using Nustache.Core;

namespace HighchartsCs
{
    public static class HighchartExtensions
    {
        public static HtmlString IncludeHighcharts(this HtmlHelper helper)
        {
            const string include = @"
<script type='text/javascript' src='https://code.jquery.com/jquery-1.11.2.min.js'></script>
<script type='text/javascript' src='http://code.highcharts.com/highcharts.js'></script>
<script src='http://code.highcharts.com/modules/exporting.js'></script>";
            return new HtmlString(include);
        }

        public static HtmlString MakeChart(this HtmlHelper helper, MultiAxesTemplateRaw template)
        {
            var series = new List<SeriesRaw>
            {
                new SeriesRaw
                {
                    Name = "Rainfall",
                    Type = "column",
                    YAxis = 1,
                    Data = "[49.9, 71.5, 106.4, 129.2, 144.0, 176.0, 135.6, 148.5, 216.4, 194.1, 95.6, 54.4]",
                    TooltipValueSuffix = " mm"
                },
                new SeriesRaw
                {
                    Name = "Sea-Level Pressure",
                    Type = "spline",
                    YAxis = 2,
                    Data =
                        "[1016, 1016, 1015.9, 1015.5, 1012.3, 1009.5, 1009.6, 1010.2, 1013.1, 1016.9, 1018.2, 1016.7]",
                    MarkerEnabled = true,
                    DashStyle = "shortdot",
                    TooltipValueSuffix = " mb"
                },
                new SeriesRaw
                {
                    Name = "Temperature",
                    Type = "spline",
                    Data = "[7.0, 6.9, 9.5, 14.5, 18.2, 21.5, 25.2, 26.5, 23.3, 18.3, 13.9, 9.6]",
                    TooltipValueSuffix = " C"
                }
            };

            var axis = new List<AxisRaw>
            {
                new AxisRaw
                {
                    LabelsFormat = "{value} C",
                    TitleText = "Temperature",
                    Opposite = true
                },
                new AxisRaw
                {
                    GridLineWidth = 0,
                    TitleText = "Rainfall",
                    LabelsFormat = "{value} mm"
                },
                new AxisRaw
                {
                    GridLineWidth = 0,
                    TitleText = "Sea-Level Pressure",
                    LabelsFormat = "{value} mb",
                    Opposite = true
                }
            };

            var template1 = new MultiAxesTemplateRaw
            {
                Series = series.ToArray(),
                YAxis = axis.ToArray(),
                SubtitleText = "Source: WorldClimate.com",
                TitleText = "Average Monthly Weather Data for Tokyo",
                XAxisCategories =
                    new[] {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}
            };

            return new HtmlString(Highcharts.CreateChart(template));
        }
    }
    public class Highcharts
    {
        public static string CreateChart(MultiAxesTemplateRaw templateObjs)
        {
            var template = EmbeddedResource.Get("multi-axes.js");

            if (templateObjs.YAxis == null || !templateObjs.YAxis.Any())
            {
                throw new InvalidOperationException("Must include at least 1 YAxis");
            }

            // fix possible null values
            templateObjs.YAxis = templateObjs
                .YAxis
                .Select(ValidateAndFixAxis)
                .ToArray();

            var result = Render.StringToString(template, new
            {
                YAxis = CreateAxis(templateObjs.YAxis),
                TitleText = templateObjs.TitleText.StrEscape(),
                SubtitleText = templateObjs.SubtitleText.StrEscape(),
                XAxisCategories = string.Join(",", templateObjs.XAxisCategories.Select(x => x.StrEscape())),
                Series = CreateSeries(templateObjs.Series)
            });
            return result;
        }

        private static AxisRaw ValidateAndFixAxis(AxisRaw rawAxis)
        {
            if (string.IsNullOrEmpty(rawAxis.LabelsFormat))
            {
                rawAxis.LabelsFormat = "{value}";
            }
            if (string.IsNullOrEmpty(rawAxis.TitleStyleColor))
            {
                rawAxis.TitleStyleColor = "Highcharts.getOptions().colors[2]";
            }
            if (string.IsNullOrEmpty(rawAxis.LabelsStyleColor))
            {
                rawAxis.LabelsStyleColor = "Highcharts.getOptions().colors[2]";
            }
            return rawAxis;
        }

        public static string CreateSeries(SeriesRaw[] series)
        {
            const string template = @"
{
    name: {{Name}},
    type: {{Type}},
    yAxis: {{YAxis}},
    data: {{Data}},
    tooltip: {
        {{TooltipJs}}
    },
    marker: {
        {{MarkerJs}}
    },
    dashStyle: {{DashStyle}}
}";

            var results = new List<string>();
            foreach (var s in series)
            {
                var tooltipJs = "";
                if (s.TooltipValueSuffix != null)
                {
                    tooltipJs = "valueSuffix: " + s.TooltipValueSuffix.StrEscape() + Environment.NewLine;
                }

                var markerJs = "";
                if (s.MarkerEnabled != null)
                {
                    markerJs = "enabled: " + s.MarkerEnabled.ToString().ToLower() + Environment.NewLine;
                }
                var r = Render.StringToString(template, new
                {
                    Name = s.Name.StrEscape(),
                    Type = s.Type.StrEscape(),
                    YAxis = s.YAxis,
                    Data = s.Data,
                    MarkerJs = markerJs,
                    tooltipJs = tooltipJs,
                    DashStyle = s.DashStyle.StrEscape()
                });
                results.Add(r);
            }

            return string.Join(",", results.ToArray());
        }


        public static string CreateAxis(AxisRaw[] axes)
        {
            const string js = @"
{
    gridLineWidth: {{GridLineWidth}},
    labels: { 
        format: {{LabelsFormat}},
        style: {
            color: {{LabelsStyleColor}},
        }
    },
    title: {
        text: {{TitleText}},
        style: {
            color: {{TitleStyleColor}},
        }
    },
    {{Extra}}
}";

            var a = new List<string>();

            foreach (var axis in axes)
            {
                var extra = "";
                if (axis.Opposite != null)
                {
                    extra = "opposite: " + axis.Opposite.Value.ToString().ToLower() + Environment.NewLine;
                }
                var result = Render.StringToString(js, new
                {
                    GridLineWidth = axis.GridLineWidth,
                    LabelsFormat = axis.LabelsFormat.StrEscape(),
                    LabelsStyleColor = axis.LabelsStyleColor,
                    TitleText = axis.TitleText.StrEscape(),
                    TitleStyleColor = axis.TitleStyleColor,
                    Extra = extra
                });

                a.Add(result);
            }
            return string.Join(",", a.ToArray());
        }

    }
}