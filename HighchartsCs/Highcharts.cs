using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Chronos;
using Humanizer;
using Nustache.Core;
using ServiceStack;

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
            return new HtmlString(Highcharts.CreateChart(template));
        }
    }
    public class Highcharts
    {
        public static string MakeMultiAxisChart(string json, string title, string subtitle, string yAxis1Fmt = null, string yAxis2Fmt = null,
            string yAxis1Title = null, string yAxis2Title = null, string series1Type = "column", string series2Type = "spline")
        {
            var dict = json.FromJson<List<Dictionary<string, object>>>();
            var keys = new List<string>();
            if (dict != null && dict.FirstOrDefault() != null)
            {
                var firstRecord = dict.FirstOrDefault();
                keys = firstRecord.Keys.ToList();
            }

            yAxis1Fmt = yAxis1Fmt ?? "{value}";
            yAxis2Fmt = yAxis2Fmt ?? "{value}";
            yAxis1Title = yAxis1Title ?? keys[1].Humanize();
            yAxis2Title = yAxis2Title ?? keys[2].Humanize();


            //consider x axis to be the first column
            var columns = dict.Select(x => x[keys[0]].ToString());

            var axis = new[]
            {
                new AxisRaw
                {
                    Opposite = false,
                    GridLineWidth = 1,
                    LabelsFormat = yAxis1Fmt,
                    LabelsStyleColor = "'#6D869F'",
                    TitleStyleColor = "'#707070'",
                    TitleText =yAxis1Title 
                },
                new AxisRaw
                {
                    Opposite = true,
                    GridLineWidth = 1,
                    LabelsFormat = yAxis2Fmt,
                    LabelsStyleColor = "'#6D869F'",
                    TitleStyleColor = "'#707070'",
                    TitleText = yAxis2Title 
                }
            };

            var series = new[]
            {
                new SeriesRaw
                {
                    Name = keys[1].Humanize(),
                    Data = "[" + string.Join(",", dict.Select(x => x[keys[1]].ToString())) + "]",
                    Type = series1Type,
                    YAxis = 0
                },
                new SeriesRaw
                {
                    Name = keys[2].Humanize(),
                    Data = "[" + string.Join(",", dict.Select(x => x[keys[2]].ToString())) + "]",
                    Type = series2Type,
                    YAxis = 1
                }
            };

            var template = new MultiAxesTemplateRaw
            {
                Series = series,
                XAxisCategories = columns.ToArray(),
                YAxis = axis,
                SubtitleText = subtitle,
                TitleText = title
            };

            return Highcharts.CreateChart(template);
        }
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

            var dt = default(DateTime); 
            var isDate = DateTime.TryParse(templateObjs.XAxisCategories[0], out dt);

            var xAxisCategories = isDate 
                ? string.Join(",",templateObjs.XAxisCategories.Select(x => "new Date(Date.parse('" + x + "'))").ToArray()) 
                : string.Join(",", templateObjs.XAxisCategories.Select(x => x.StrEscape()));

            var result = Render.StringToString(template, new
            {
                YAxis = CreateAxis(templateObjs.YAxis),
                TitleText = templateObjs.TitleText.StrEscape(),
                SubtitleText = templateObjs.SubtitleText.StrEscape(),
                XAxisCategories = xAxisCategories,
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