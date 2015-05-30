using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


using System.IO;

using System.Windows.Forms.DataVisualization.Charting;

using SignalProcessing.Filter;

namespace Sound
{
    /// <summary>
    /// Логика взаимодействия для frmFilterTransmitFunction.xaml
    /// </summary>
    public partial class frmFilterTransmitFunction : Window
    {
        private KaiserBandpass _filter;

        public frmFilterTransmitFunction()
        {
            InitializeComponent();

            // add legent
            Legend legend = new Legend();
            legend.Docking = Docking.Bottom;
            chartFilterTransmitFunction.Legends.Add(legend);

            // chart area
            chartFilterTransmitFunction.ChartAreas.Add(new ChartArea("Default"));

            chartFilterTransmitFunction.ChartAreas["Default"].AxisY.IsInterlaced = true;
            chartFilterTransmitFunction.ChartAreas["Default"].AxisY.InterlacedColor = System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gold);

            chartFilterTransmitFunction.ChartAreas["Default"].AxisX.Minimum = 0;
            chartFilterTransmitFunction.ChartAreas["Default"].AxisX.Interval = 100;
        }

        private KaiserBandpass getFilter()
        {
            if (_filter != null)
                return _filter;

            int filterOrder = Convert.ToInt32(txtFilterOrder.Text);
            int sampleFrequency = Convert.ToInt32(txtSampleFrequency.Text);

            double leftBoundFrequency = Convert.ToDouble(txtLeftBoundFrequency.Text);
            double rightBoundFrequency = Convert.ToDouble(txtRightBoundFrequency.Text);
            

            // filter
            _filter = new KaiserBandpass();
            _filter
                .setFilterOrder(filterOrder)
                .setLeftBoundFrequency(leftBoundFrequency)
                .setRightBoundFrequency(rightBoundFrequency)
                .setSampleFrequency(sampleFrequency);

            return _filter;
        }

        private void buildChartSeries()
        {
            chartFilterTransmitFunction.ChartAreas["Default"].AxisX.Maximum = 4000;

            if((bool) chkFilterRack.IsChecked)
            {
                int filterOrder = Convert.ToInt32(txtFilterOrder.Text);
                int sampleFrequency = Convert.ToInt32(txtSampleFrequency.Text);

                KaiserBandpass filter = new KaiserBandpass();
                filter
                    .setFilterOrder(filterOrder)
                    .setSampleFrequency(sampleFrequency);

                int[] noiseFrequencyRange = new int[] { 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150 };

                double leftBoundFrequency, rightBoundFrequency;

                string seriesName;

                for (int i = 0; i < noiseFrequencyRange.Length; i++)
                {
                    seriesName = "Filter " + Convert.ToString(noiseFrequencyRange[i]);

                    // detele old series line
                    if (chartFilterTransmitFunction.Series.IndexOf(seriesName) >= 0)
                    {
                        chartFilterTransmitFunction.Series.Remove(chartFilterTransmitFunction.Series[seriesName]);
                    }

                    chartFilterTransmitFunction.Series.Add(new Series(seriesName));
                    chartFilterTransmitFunction.Series[seriesName].ChartArea = "Default";
                    chartFilterTransmitFunction.Series[seriesName].ChartType = SeriesChartType.FastLine;
                    chartFilterTransmitFunction.Series[seriesName].BorderWidth = 3;

                    leftBoundFrequency = noiseFrequencyRange[i] / 1.12;
                    rightBoundFrequency = noiseFrequencyRange[i] * 1.12;

                    filter
                        .setLeftBoundFrequency(leftBoundFrequency)
                        .setRightBoundFrequency(rightBoundFrequency);

                    for (double f = 0; f < sampleFrequency; f = f + 5)
                    {
                        chartFilterTransmitFunction.Series[seriesName].Points.AddXY(f, alglib.math.abscomplex(filter.getFreqCharValue(f)));
                    }
                }
            }
            else
            {
                // detele old series line
                if (chartFilterTransmitFunction.Series.IndexOf("Transfer Function") >= 0)
                {
                    chartFilterTransmitFunction.Series.Remove(chartFilterTransmitFunction.Series["Transfer Function"]);
                }

                chartFilterTransmitFunction.Series.Add(new Series("Transfer Function"));
                chartFilterTransmitFunction.Series["Transfer Function"].ChartArea = "Default";
                chartFilterTransmitFunction.Series["Transfer Function"].ChartType = SeriesChartType.FastLine;
                chartFilterTransmitFunction.Series["Transfer Function"].BorderWidth = 3;

                KaiserBandpass filter = getFilter();

                // print 
                for (double f = 0; f < filter.getSampleFrequency(); f = f + 5)
                {
                    chartFilterTransmitFunction.Series["Transfer Function"].Points.AddXY(f, alglib.math.abscomplex(filter.getFreqCharValue(f)));
                }
            }
            

            chartFilterTransmitFunction.DataBind();
        }

        private void cmdRebuildChart_Click(object sender, RoutedEventArgs e)
        {
            _filter = null;

            // build new line
            buildChartSeries();
        }

        private void cmdSaveAkValuesToFile_Click(object sender, RoutedEventArgs e)
        {
            _filter = null;

            buildChartSeries();

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".csv";
            dlg.Filter = "Файл CSV (.csv)|*.csv";

            KaiserBandpass filter = getFilter();

            Nullable<bool> result = dlg.ShowDialog();
            if (result == false)
                return;

            StreamWriter log = new StreamWriter(dlg.FileName);

            log.WriteLine(_filter.getA0().x);

            for (int k = 1; k <= filter.getFilterOrder(); k++)
            {
                log.WriteLine( _filter.getAk(k).x );
            }

            log.Close();
        }

        private void chkFilterRack_Checked(object sender, RoutedEventArgs e)
        {
            stackFilterParams.IsEnabled = false;

            cmdSaveAkValuesToFile.IsEnabled = false;
        }

        private void chkFilterRack_UnChecked(object sender, RoutedEventArgs e)
        {
            stackFilterParams.IsEnabled = true;

            cmdSaveAkValuesToFile.IsEnabled = true;
        }
    }
}
