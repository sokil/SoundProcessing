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

using Sound.Core.Soundprocessing;
using SignalProcessing.Signal;
using SignalProcessing.Filter;

using System.Windows.Forms.DataVisualization.Charting;

using System.Threading;

namespace Sound
{
    /// <summary>
    /// Логика взаимодействия для frmPeriodogram.xaml
    /// </summary>
    public partial class frmPeriodogram : Window
    {
        Spectrum _spectrum = new Spectrum();

        public frmPeriodogram()
        {
            InitializeComponent();

            // add legent
            Legend legend = new Legend();
            legend.Docking = Docking.Bottom;
            chartPeriodogram.Legends.Add(legend);

            // chart area
            chartPeriodogram.ChartAreas.Add(new ChartArea("Default"));

            chartPeriodogram.ChartAreas["Default"].AxisY.IsInterlaced = true;
            chartPeriodogram.ChartAreas["Default"].AxisY.InterlacedColor = System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gold);

            chartPeriodogram.ChartAreas["Default"].AxisX.Minimum = 0;
            chartPeriodogram.ChartAreas["Default"].AxisX.Interval = 200;
        }

        public frmPeriodogram setSignal(Wav signal)
        {
            _spectrum
                .setSampleFrequency(signal.getSampleFrequency())
                .setSignal(signal.getComplexSignal());

            txtVariance.Text = Convert.ToString(signal.getVariance());

            return this;
        }

        public frmPeriodogram setSignal(Noise signal)
        {
            _spectrum
                .setSampleFrequency(signal.getSampleFrequency())
                .setSignal(signal.getComplexSignal());

            txtVariance.Text = Convert.ToString(signal.getVariance());

            return this;
        }

        public void drawChart()
        {
            // detele old series line
            if (chartPeriodogram.Series.IndexOf("Periodogram") >= 0)
            {
                chartPeriodogram.Series.Remove(chartPeriodogram.Series["Periodogram"]);
            }

            chartPeriodogram.Series.Add(new Series("Periodogram"));
            chartPeriodogram.Series["Periodogram"].ChartArea = "Default";
            chartPeriodogram.Series["Periodogram"].ChartType = SeriesChartType.FastLine;
            chartPeriodogram.Series["Periodogram"].BorderWidth = 1;

            chartPeriodogram.ChartAreas["Default"].AxisX.Minimum = 0;
            chartPeriodogram.ChartAreas["Default"].AxisX.Maximum = _spectrum.getSampleFrequency() / 2;

            // print 
            double[] points = _spectrum.getPeriodogram();
            for (int f = 0; f < points.Length; f++)
            {
                chartPeriodogram.Series["Periodogram"].Points.AddXY(
                    f * _spectrum.getSampleFrequency() / points.Length,
                    //points[f]
                    10 * Math.Log10(points[f])
                );
            }

            chartPeriodogram.DataBind();

           
        }
    }
}
