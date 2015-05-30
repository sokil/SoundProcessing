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

using Sound.Model;
using Sound.Model.Soundinsulation;

using System.Windows.Forms.DataVisualization.Charting;

namespace Sound
{
    /// <summary>
    /// Логика взаимодействия для frmResults.xaml
    /// </summary>
    public partial class frmResults : Window
    {
        double _room2NoiseLevel;

        Wall _separateWall;

        public frmResults()
        {
            InitializeComponent();

            // add legend
            chartWithFlanking.Legends.Add(new Legend() { Docking = Docking.Bottom });
            chartWithoutFlanking.Legends.Add(new Legend() { Docking = Docking.Bottom });

            // chart area
            chartWithFlanking.ChartAreas.Add(new ChartArea("Default"));
            chartWithFlanking.ChartAreas["Default"].AxisX.Interval = 1;

            chartWithoutFlanking.ChartAreas.Add(new ChartArea("Default"));
            chartWithoutFlanking.ChartAreas["Default"].AxisX.Interval = 1;

            // configure axis
            chartWithFlanking.ChartAreas["Default"].AxisY.IsInterlaced = true;
            chartWithFlanking.ChartAreas["Default"].AxisY.InterlacedColor = System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gold);

            chartWithoutFlanking.ChartAreas["Default"].AxisY.IsInterlaced = true;
            chartWithoutFlanking.ChartAreas["Default"].AxisY.InterlacedColor = System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gold);

            // draw frequency characteristic
            chartWithFlanking.Series.Add(new Series("Frequency Chart"));
            chartWithFlanking.Series["Frequency Chart"].ChartArea = "Default";
            chartWithFlanking.Series["Frequency Chart"].ChartType = SeriesChartType.Line;

            chartWithoutFlanking.Series.Add(new Series("Frequency Chart"));
            chartWithoutFlanking.Series["Frequency Chart"].ChartArea = "Default";
            chartWithoutFlanking.Series["Frequency Chart"].ChartType = SeriesChartType.Line;
        }

        public frmResults setRoom2NoiseLevel(double level)
        {
            _room2NoiseLevel = level;

            lblRoom2NoiseLevel.Content = level;

            return this;
        }

        public frmResults setSeparateWall(Wall wall)
        {
            _separateWall = wall;

            switch(wall.getType())
            {
                case Wall.Type.Monolith:
                    lblSeparateWallParams.Content = "Монолітна перегородка";
                    break;

                case Wall.Type.Onelayer:
                    lblSeparateWallParams.Content = "Одношарова перегородка";
                    break;

                case Wall.Type.Frame:
                    lblSeparateWallParams.Content = "Каркасна перегородка";
                    break;
            }

            return this;
        }

        public frmResults setTransmit(Sound.Model.Soundprocessing.SoundTrasmittion transmit)
        {
            lblRoom1SignalLevel.Content = transmit.getRoom1SignalLevel();

            SortedList<double, double> characteristic;

            double[] frequencyList = Core.ServiceLocator.getFrequencyRangeByFilterChainNum(transmit.getElementsInFilterChain());

            /**
             * with
             */
            #if DEBUG
                Console.WriteLine("##### WITH FLANKING TRANSMITTION #####");
            #endif

            characteristic = transmit
                .withFlankingTransmittion(true)
                .getSeparateWallSoundInsulationCharacteristic();

            chartWithFlanking.Series["Frequency Chart"].Points.Clear();

            for (int i = 0; i < frequencyList.Length; i++)
            {
                // add chart pount
                chartWithFlanking.Series["Frequency Chart"].Points.AddXY(Convert.ToString(frequencyList[i]), characteristic[frequencyList[i]]);
            }

            chartWithFlanking.DataBind();

            double articulation = transmit.getArticulation();

            lblWithFlankingResults.Content = "Розбірливість мови:" + Convert.ToString( articulation )
                + ", Rw: " + new EstimateCharacteristic(characteristic).getRw()
                + ", Еквівалентний рівень сигналу в другій кімнаті: " + transmit.getRoom2EquivalentSignalLevel()
                + ", середній: " + transmit.getRoom2MedianSignalLevel()
                + ", SNR: " + transmit.getRoom2EquivalentSignalNoiseRatio()
                ;

            /**
             * without
             */
            #if DEBUG
                Console.WriteLine("##### WITH FLANKING TRANSMITTION #####");
            #endif

            characteristic = transmit
                .withFlankingTransmittion(false)
                .getSeparateWallSoundInsulationCharacteristic();

            chartWithoutFlanking.Series["Frequency Chart"].Points.Clear();

            for (int i = 0; i < frequencyList.Length; i++)
            {
                // add chart pount
                chartWithoutFlanking.Series["Frequency Chart"].Points.AddXY(Convert.ToString(frequencyList[i]), characteristic[frequencyList[i]]);
            }

            chartWithoutFlanking.DataBind();

            articulation = transmit.getArticulation();

            lblWithoutFlankingResults.Content = "Розбірливість мови:" + Convert.ToString(articulation)
                + ", Rw: " + new EstimateCharacteristic(characteristic).getRw()
                + ", Еквівалентний рівень сигналу в другій кімнаті: " + transmit.getRoom2EquivalentSignalLevel()
                + ", середній: " + transmit.getRoom2MedianSignalLevel()
                + ", SNR: " + transmit.getRoom2EquivalentSignalNoiseRatio();

            return this;
        }

        
    }
}
