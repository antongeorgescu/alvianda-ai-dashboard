using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using ChartJs.Blazor.Charts;
using ChartJs.Blazor.ChartJS.PieChart;
using ChartJs.Blazor.ChartJS.BarChart;
using ChartJs.Blazor.ChartJS.LineChart;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Enums;
using TData = ChartJs.Blazor.ChartJS.Common.Wrappers;
using ChartJs.Blazor.Util;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using ChartJs.Blazor.ChartJS.Common;
using System.Security.Cryptography.X509Certificates;
using ChartJs.Blazor.ChartJS.BubbleChart;
using ChartJs.Blazor.ChartJS.BarChart.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Handlers;
using Alvianda.AI.Dashboard.Datapayload;
using Alvianda.AI.Dashboard.Models.Settings;
using Newtonsoft.Json;
using System.Text;
using Alvianda.AI.Dashboard.Settings;

namespace Alvianda.AI.Dashboard.Pages.EventLogPrediction
{

    [Authorize]
    public partial class EventCharts : ComponentBase
    {
        protected PieConfig _pieConfig { get; set; }
        protected ChartJsPieChart _pieChartJs { get; set; }

        protected BarConfig _barConfig;
        protected ChartJsBarChart _barChartJs;

        protected LineConfig _lineConfig;
        protected ChartJsLineChart _lineChartJs;

        protected string MachineIdentity { get; set; }
        private string MachineFullName { get; set; }

        [Inject] IConfiguration Config { get; set; }
        [Inject] HttpClient Http { get; set; }

        [Inject] IEventViewerMachineListConfig EventViewerMachineListConfig { get; set; }

        private bool isError = false;
        private bool isChartData = false;
        private string errMessage = string.Empty;
        private string loadingMessage = string.Empty;

        private string toDate;
        private string fromDate;
        //private string SelectedChartType { get; set; }

        private IList<string> labels;
        private IList<double> values;
        
        protected string pieChartVisible { get; set; }
        protected string barChartVisible { get; set; }
        //private string debugInfo;

        private IList<EventViewerMachine> EventViewerMachineList { get; set; }

        protected override async void OnInitialized()
        {
            //SelectedChartType = "None";

            var charts = new List<Charts.ChartSettings>();
            charts.Add(new Charts.ChartSettings()
            {
                ChartId = "pieChart",
                Visibility = "hidden",
                Height=0,
                Width=0
            });
            charts.Add(new Charts.ChartSettings()
            {
                ChartId = "barChart",
                Visibility = "hidden",
                Height = 0,
                Width = 0
            });
            Charts.Initialize(charts);

            EventViewerMachineList = EventViewerMachineListConfig.MachineList();

            MachineIdentity = EventViewerMachineList.Select(x => x.Name).First();
            MachineFullName = EventViewerMachineList.Select(x => x.Description).First();

            var serviceEndpoint = $"{Config.GetValue<string>("LoggerServicesAPI:BaseURI")}{Config.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}/logs/machinename";
            MachineSettings payload = new MachineSettings() { MachineName = MachineIdentity };
            string jsonpayload = await Task.Run(() => JsonConvert.SerializeObject(payload));
            HttpContent c = new StringContent(jsonpayload, Encoding.UTF8, "application/json");

            var response = await Http.PostAsync(serviceEndpoint, c);
            response.EnsureSuccessStatusCode();
        }

        protected async Task BuildChartLogCategsPerDateRange()
        {
            try
            {
                isChartData = false;
                loadingMessage = "Loading data. Please wait...";
                isError = true;
                errMessage = string.Empty;

                string chartTitle;
                string urlEndPoint;

                if ((fromDate == null) && (toDate == null))
                {
                    chartTitle = $"Event Logs on machine {MachineIdentity}{Environment.NewLine}Log Categories Distribution";
                    urlEndPoint = @"/logs/categories/null/null";
                }
                else if (toDate == null)
                {
                    toDate = DateTime.Now.ToShortDateString();
                    chartTitle = $"Event Logs on machine {MachineIdentity}{Environment.NewLine}Log Categories Distribution between {fromDate} and {toDate}";
                    urlEndPoint = $"/logs/categories/{fromDate}/null";
                }
                else if (fromDate == null)
                {
                    isError = true;
                    errMessage = "Start Date cannot be null, while End Date has a value. Please correct and resubmit.";
                    loadingMessage = string.Empty;
                    return;
                }
                else
                {
                    chartTitle = $"Event Logs on machine {MachineIdentity}{Environment.NewLine}Log Categories Distribution between {fromDate} and {toDate}";
                    urlEndPoint = $"/logs/categories/{fromDate}/{toDate}";
                }

                //var serviceEndpoint = $"{Config.GetValue<string>("LoggerServicesAPI:BaseURI")}{Config.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}/logs/categories/{sFromDate}/{sToDate}";
                var serviceEndpoint = $"{Config.GetValue<string>("LoggerServicesAPI:BaseURI")}{Config.GetValue<string>("LoggerServicesAPI:EventViewerRouting")}{urlEndPoint}";
                IList<LogCategs> chartData = await Http.GetFromJsonAsync<IList<LogCategs>>(serviceEndpoint);

                this.labels = new List<string>();
                chartData.ToList().ForEach(x => labels.Add(x.DisplayName));
                this.values = new List<double>();
                chartData.ToList().ForEach(x => values.Add((double)x.EntriesCount));

                var colorList = new List<string>();
                for (int k = 0; k < labels.Count; k++)
                    colorList.Add(ColorUtil.RandomColorString());

                var chartName = "Distributed events per log category";

                /// Pie Chart
                _pieConfig = new PieConfig
                {
                    Options = new PieOptions
                    {
                        Title = new OptionsTitle
                        {
                            Display = true,
                            Text = new IndexableOption<string>(chartTitle)
                        },
                        Responsive = true,
                        Animation = new ArcAnimation
                        {
                            AnimateRotate = true,
                            AnimateScale = true
                        }
                    },
                };
                _pieConfig.Data.Labels.AddRange(labels);

                var pieSet = new PieDataset
                {
                    BackgroundColor = colorList.ToArray<string>(),
                    BorderWidth = 0,
                    HoverBackgroundColor = ColorUtil.RandomColorString(),
                    HoverBorderColor = ColorUtil.RandomColorString(),
                    HoverBorderWidth = 1,
                    BorderColor = "#ffffff"
                };
                pieSet.Data.AddRange(values);
                _pieConfig.Data.Datasets.Add(pieSet);

                _barConfig = new BarConfig
                {
                    Options = new BarOptions
                    {
                        Title = new OptionsTitle
                        {
                            Display = true,
                            Text = new IndexableOption<string>(chartTitle)
                        },
                        Scales = new BarScales
                        {
                            XAxes = new List<CartesianAxis>
                                    {
                                        new BarCategoryAxis
                                        {
                                            BarPercentage = 0.5,
                                            BarThickness = BarThickness.Flex
                                        }
                                    },
                            YAxes = new List<CartesianAxis>
                                    {
                                        new BarLinearCartesianAxis
                                        {
                                            Ticks = new LinearCartesianTicks
                                            {
                                                BeginAtZero = false
                                                //SuggestedMax = (int?)values.Max(),
                                                //SuggestedMin = (int?)values.Min()
                                            }
                                        }
                                    }
                        },
                        Responsive = true,
                        Animation = new ArcAnimation
                        {
                            AnimateRotate = true,
                            AnimateScale = true
                        }
                    }
                };

                _barConfig.Data.Labels.AddRange(labels);

                var _barDataSet = new BarDataset<TData.DoubleWrapper>(ChartType.Bar)
                {
                    BackgroundColor = colorList.ToArray(),
                    BorderWidth = 0,
                    HoverBackgroundColor = ColorUtil.RandomColorString(),
                    HoverBorderColor = ColorUtil.RandomColorString(),
                    HoverBorderWidth = 1,
                    BorderColor = "#ffffff"

                };

                var xlstvals = new TData.DoubleWrapper[values.Count];
                int i = 0;
                values.ToList().ForEach(x => { xlstvals[i++] = x; });

                _barDataSet.AddRange(xlstvals);
                _barConfig.Data.Datasets.Add(_barDataSet);

                isChartData = true;
            }
            catch (Exception exception)
            {
                isError = true;
                errMessage = $"Error:{exception.Message}";
            }
            finally
            {
                loadingMessage = string.Empty;
            }
        }
        
        public void SelectStartDate(ChangeEventArgs e)
        {
            fromDate = Convert.ToString(e.Value);
        }

        public void SelectEndDate(ChangeEventArgs e)
        {
            toDate = Convert.ToString(e.Value);
        }

        public void SelectChartType(ChangeEventArgs e)
        {
            var chartId = Convert.ToString(e.Value);
            if (!(chartId == "None"))
                Charts.Show(chartId);
        }
    }

    public class LogCategs
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int EntriesCount { get; set; }
    }

    public static class Charts
    {
        public static IList<ChartSettings> ChartList;
        
        public static void Initialize(IList<ChartSettings> charts)
        {
            ChartList = charts;
            foreach (var chart in ChartList)
            {
                chart.Visibility = "hidden";
                chart.Height = 0;
                chart.Width = 0;
            }
        }
        
        public static void Show(string chartId)
        {
            var selectedChartId = ChartList.Select(x => x).First(x => x.ChartId == chartId).ChartId;

            ChartList.ToList<ChartSettings>().ForEach(x => {
                if (x.ChartId == selectedChartId)
                {
                    x.Visibility = "visible";
                    x.Height = 300;
                    x.Width = 600;
                }
                else
                {
                    x.Visibility = "hidden";
                    x.Height = 0;
                    x.Width = 0;
                }
            });
        }

        public class ChartSettings
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public string Visibility { get; set; }
            public string ChartId { get; set; }
        }
    }
}
