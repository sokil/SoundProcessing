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

using System.Data;

using Sound.Model;
using Sound.Model.WallComposition;
using Sound.Model.Soundinsulation;

using System.Windows.Forms.DataVisualization.Charting;

using Sound.Model.Soundinsulation.FrameWallPanelsInsulation;


namespace Sound
{
    /// <summary>
    /// Логика взаимодействия для Window1.xamlfrmWallSoundInsulation
    /// </summary>
    public partial class frmWallSoundInsulation : Window
    {
        private bool _isCalculated = false;

        private Wall _wall;

        public frmWallSoundInsulation(Wall.Name name)
        {
            InitializeComponent();

            // set wall name
            getWall().setName(name);

            // change System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            // init monolith materials
            Materials materialsModel = new Materials();
            materialsModel.getAll();
            comboMonolithMaterials.DataContext = materialsModel;

            // init monolith pannels
            Panels panelsModel = new Panels();
            panelsModel.getAll();
            comboMonolithPanels.DataContext = panelsModel;

            // init panel materials
            comboOnelayerMaterials.DataContext = panelsModel;

            // init framework materials
            comboFramework1Materials.DataContext = panelsModel;
            comboFramework2Materials.DataContext = panelsModel;


            // init material frecuency characteristic
            double[] frequencyList = AbstractWallInsulation
                .getStandartEstimateFrequencyCharacteristic()
                .Keys.ToArray();
            
            List<List<double>> matValues = new List<List<double>>();
            matValues.Add(new List<double>()
            {
                // 33, 36, 39, 42, 45, 48, 51, 52, 53, 54, 55, 56, 56, 56, 56, 56
                19.94, 28.42, 32.74, 31.70, 31.13, 33.29, 37.45, 36.11, 38.17, 39.25, 42.7, 45.25, 47.09, 48.02, 50.57, 52.69
            });

            dataGridManualMaterialFrequencyCharacteristic.ItemsSource = matValues;

            /**
             * chart init
             */
            _initChart();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // set next element after current in comboWallEnum
            Wall.Name frameWallName = getWall().getName();

            for (int i = 0; i < comboWallEnum.Items.Count; i++)
            {
                System.Xml.XmlElement item = (System.Xml.XmlElement)comboWallEnum.Items[i];

                Wall.Name comboItemWallName = (Wall.Name)Convert.ToInt32(item.GetAttribute("Value"));

                if (comboItemWallName != frameWallName)
                    continue;

                int nextComboItemId = ++i;

                if (nextComboItemId == comboWallEnum.Items.Count)
                {
                    nextComboItemId = 0;
                }

                comboWallEnum.SelectedIndex = nextComboItemId;
                break;
            }
        }

        private void _initChart()
        {
            double[] frequencyList = AbstractWallInsulation
                .getStandartEstimateFrequencyCharacteristic()
                .Keys.ToArray();

            // fill datatable with frequency caracteristic
            for (int i = 0; i < frequencyList.Length; i++)
            {
                dataGridFrequencyCharacteristic.Columns.Add(new DataGridTextColumn()
                {
                    Header = frequencyList[i],
                    Binding = new Binding(".[" + i + "]")
                });

                dataGridManualMaterialFrequencyCharacteristic.Columns.Add(new DataGridTextColumn()
                {
                    Header = frequencyList[i],
                    Binding = new Binding(".[" + i + "]")
                });
            }            

            // draw chart
            Legend legend = new Legend();
            legend.Docking = Docking.Bottom;
            chartFrequencyCharacteristic.Legends.Add(legend);

            chartFrequencyCharacteristic.ChartAreas.Add(new ChartArea("Default"));
            chartFrequencyCharacteristic.ChartAreas["Default"].AxisX.Interval = 1;
            //chartFrequencyCharacteristic.ChartAreas["Default"].AxisX.LogarithmBase = 10;


            chartFrequencyCharacteristic.ChartAreas["Default"].AxisY.IsInterlaced = true;
            chartFrequencyCharacteristic.ChartAreas["Default"].AxisY.Interval = 10;
            chartFrequencyCharacteristic.ChartAreas["Default"].AxisY.Maximum = 100;
            chartFrequencyCharacteristic.ChartAreas["Default"].AxisY.InterlacedColor = System.Drawing.Color.FromArgb(50, System.Drawing.Color.Gold);

            // draw frequency characteristic
            chartFrequencyCharacteristic.Series.Add(new Series("Frequency Chart"));
            chartFrequencyCharacteristic.Series["Frequency Chart"].ChartArea = "Default";
            chartFrequencyCharacteristic.Series["Frequency Chart"].ChartType = SeriesChartType.Line;
        }

        private void _drawChart()
        {
            double[] frequencyList = AbstractWallInsulation
                .getStandartEstimateFrequencyCharacteristic()
                .Keys.ToArray();

            AbstractWallInsulation wallInsulation = _wall.getSoundInsulation();

            SortedList<double, double> estimatedFrequencyCharacteristic = wallInsulation.getEstimateFrequencyCharacteristic();
            SortedList<double, double> frequencyCharacteristic = wallInsulation.getFrequencyCharacteristic();

            // show cal results
            textBlockResults.Text = wallInsulation.getSummary();

            // clear previous data
            chartFrequencyCharacteristic.Series["Frequency Chart"].Points.Clear();

            // fill data
            List<List<double>> fcValues = new List<List<double>>();
            List<double> fcRow1 = new List<double>();
            List<double> fcRow2 = new List<double>();

            for (int i = 0; i < frequencyList.Length; i++)
            {
                // add row1
                fcRow1.Add(frequencyCharacteristic[frequencyList[i]]);

                // add row2
                fcRow2.Add(estimatedFrequencyCharacteristic[frequencyList[i]]);

                // add chart pount
                chartFrequencyCharacteristic.Series["Frequency Chart"].Points.AddXY(Convert.ToString(frequencyList[i]), frequencyCharacteristic[frequencyList[i]]);
            }
            
            // bind data
            fcValues.Add(fcRow1);
            fcValues.Add(fcRow2);

            dataGridFrequencyCharacteristic.ItemsSource = fcValues;

            // draw chart
            chartFrequencyCharacteristic.DataBind();
        }

        public Wall getWall()
        {
            if (null != _wall)
                return _wall;

            _wall = new Wall();

            return _wall;
        }

        private void comboPartitionTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // change wall type
            Wall.Type wallType = (Wall.Type)Convert.ToInt32(comboPartitionTypes.SelectedValue);
            getWall().setType(wallType);

            // set visibility of elements related to wall type
            switch (wallType)
            {
                case Wall.Type.Monolith:
                    panelMonolitPartition.Visibility = System.Windows.Visibility.Visible;
                    panelPanelPartition.Visibility = System.Windows.Visibility.Collapsed;
                    panelFrameworkPartition.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case Wall.Type.Onelayer:
                    panelMonolitPartition.Visibility = System.Windows.Visibility.Collapsed;
                    panelPanelPartition.Visibility = System.Windows.Visibility.Visible;
                    panelFrameworkPartition.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case Wall.Type.Frame:
                    panelMonolitPartition.Visibility = System.Windows.Visibility.Collapsed;
                    panelPanelPartition.Visibility = System.Windows.Visibility.Collapsed;
                    panelFrameworkPartition.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        private void btnShowResultWindow_Click(object sender, RoutedEventArgs e)
        {
            calc();
        }

        public bool isCalculated()
        {
            return _isCalculated;
        }

        public void calc()
        {
            _isCalculated = true;

            switch (getWall().getType())
            {
                case Wall.Type.Monolith:
                    {

                        // prepare wall sound insulation model
                        MonolithWallInsulation wallInsulation = (MonolithWallInsulation)getWall()
                            .getSoundInsulation();

                        if (tabMaterialParams.SelectedIndex == 0)
                        {
                            if (txtMonolithDensity.Text == "")
                            {
                                MessageBox.Show("Заповніть густину", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }

                            if (txtMonolithCoefficientK.Text == "")
                            {
                                MessageBox.Show("Заповніть коефіцієнт К", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }

                            if (txtMonolithThickness.Text == "")
                            {
                                MessageBox.Show("Заповніть товщину", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }

                            double density = double.Parse(txtMonolithDensity.Text);
                            double coefK = double.Parse(txtMonolithCoefficientK.Text);
                            double thickness = double.Parse(txtMonolithThickness.Text) / 1000;

                            wallInsulation
                                .setDensity(density)
                                .setCoefficientK(coefK)
                                .setThickness(thickness);
                        }
                        else
                        {
                            SortedList<double, double> materialFrequencyCharacteristic = new SortedList<double, double>();

                            for (int i = 0; i < dataGridManualMaterialFrequencyCharacteristic.Columns.Count; i++)
                            {
                                double freq = (double) dataGridManualMaterialFrequencyCharacteristic.Columns[i].Header;
                                double val = ((List<double>) dataGridManualMaterialFrequencyCharacteristic.Items[0])[i];

                                materialFrequencyCharacteristic.Add(freq, val);
                            }

                            wallInsulation.setMaterialFrequencyCharacteristic(materialFrequencyCharacteristic);
                        }

                        // add panel delta
                        MonolithWallInsulation.CoveringCalcType monolithPanelCalcType = (MonolithWallInsulation.CoveringCalcType)Convert.ToInt32(comboMonolithPanelsCalcTypes.SelectedValue);
                        switch (monolithPanelCalcType)
                        {
                            case MonolithWallInsulation.CoveringCalcType.None:
                                wallInsulation.setNoCovering();
                                break;

                            case MonolithWallInsulation.CoveringCalcType.Simple:

                                bool isTwoPanels = (bool)checkTwoMonolithPanels.IsChecked;
                                bool isFillerExists = (bool)checkMonolithFillerExist.IsChecked;

                                string type = (bool)radioMonolithPanelmaterialType1.IsChecked
                                    ? "type1"
                                    : "type2";

                                wallInsulation.setSimpleCovering(type, isTwoPanels, isFillerExists);

                                break;

                            case MonolithWallInsulation.CoveringCalcType.Extended:

                                double airFillerThickness = (bool)checkMonolithFillerExist.IsChecked
                                    ? Convert.ToDouble(txtMonolithPanelAirFillerThickness.Text) / 1000
                                    : 0;

                                Sound.Model.Soundinsulation.MonolithWallInsulation.PanelBind bind = new Sound.Model.Soundinsulation.MonolithWallInsulation.PanelBind()
                                {
                                    type = (Sound.Model.Soundinsulation.MonolithWallInsulation.PanelBindType)Convert.ToInt32(checkMonolithPanelBindType.SelectedValue),
                                    count = Convert.ToInt32(txtMonolithPanelSpringBindbindNum.Text)
                                };

                                if (bind.isSpring())
                                {
                                    bind.modulus = Convert.ToDouble(txtMonolithPanelSpringBindElasticityModulus.Text);
                                    bind.thickness = Convert.ToDouble(txtMonolithPanelSpringBindThickness.Text) / 1000;
                                    bind.square = Convert.ToDouble(txtMonolithPanelSpringBindSquare.Text);
                                }

                                if (bind.isLinear())
                                {
                                    bind.isLatitudeLathDirection = (comboLathDirection.SelectedIndex == 0);
                                }

                                wallInsulation.setExtendedCovering
                                (
                                    Convert.ToDouble(txtMonolithPanelDensity.Text),
                                    Convert.ToDouble(txtMonolithPanelVelocity.Text),
                                    Convert.ToDouble(txtMonolithPanelThickness.Text) / 1000,
                                    (bool)checkTwoMonolithPanels.IsChecked,
                                    Convert.ToDouble(txtMonolithPanelAirThickness.Text) / 1000,
                                    airFillerThickness,
                                    bind
                                );
                                break;
                        }

                        break;
                    }

                case Wall.Type.Onelayer:
                    {
                        if (txtOnelayerDensity.Text == "")
                        {
                            MessageBox.Show("Заповніть густину", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }

                        if (txtOnelayerFb.Text == "")
                        {
                            MessageBox.Show("Заповніть коефіцієнт Fb", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }

                        if (txtOnelayerFc.Text == "")
                        {
                            MessageBox.Show("Заповніть коефіцієнт Fc", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }

                        if (txtOnelayerRb.Text == "")
                        {
                            MessageBox.Show("Заповніть коефіцієнт Rb", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }

                        if (txtOnelayerRc.Text == "")
                        {
                            MessageBox.Show("Заповніть коефіцієнт Rc", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }

                        if (txtOnepanelThickness.Text == "")
                        {
                            MessageBox.Show("Заповніть товщину", "Увага", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }

                        double thickness = double.Parse(txtOnepanelThickness.Text) / 1000;
                        double density = double.Parse(txtOnelayerDensity.Text);

                        // prepare wall sound insulation model
                        OnelayerWallInsulation wallInsulation = (OnelayerWallInsulation)getWall().getSoundInsulation();

                        wallInsulation
                            .setThickness(thickness)
                            .setDensity(density)
                            .setFbCoefitient(double.Parse(txtOnelayerFb.Text))
                            .setFcCoefitient(double.Parse(txtOnelayerFc.Text))
                            .setRb(double.Parse(txtOnelayerRb.Text))
                            .setRc(double.Parse(txtOnelayerRc.Text));

                        break;
                    }

                case Wall.Type.Frame:
                    {

                        double frame1PanelThickness = double.Parse(txtFrame1Thickness.Text) / 1000;
                        double framel2PanelThickness = double.Parse(txtFrame2Thickness.Text) / 1000;

                        int frame1PanelNum = int.Parse(txtFrame1PanelNum.Text);
                        int frame2PanelNum = int.Parse(txtFrame2PanelNum.Text);

                        double frame1PanelDensity = double.Parse(txtFrame1Density.Text);
                        double frame2PanelDensity = double.Parse(txtFrame2Density.Text);

                        // prepare wall sound insulation model
                        FrameWallInsulation wallInsulation = (FrameWallInsulation)getWall().getSoundInsulation();

                        wallInsulation
                            .setPanel1Thickness(frame1PanelThickness)
                            .setPanel1LayerNum(frame1PanelNum)
                            .setPanel2Thickness(framel2PanelThickness)
                            .setPanel2LayerNum(frame2PanelNum);

                        AbstractPanels panels = wallInsulation.getPanels()
                                .setPanel1Density(frame1PanelDensity)
                                .setPanel1FbCoefitient(double.Parse(txtFrame1Fb.Text))
                                .setPanel1FcCoefitient(double.Parse(txtFrame1Fc.Text))
                                .setPanel1Rb(double.Parse(txtFrame1Rb.Text))
                                .setPanel1Rc(double.Parse(txtFrame1Rc.Text))

                                .setPanel2Density(frame2PanelDensity)
                                .setPanel2FbCoefitient(double.Parse(txtFrame2Fb.Text))
                                .setPanel2FcCoefitient(double.Parse(txtFrame2Fc.Text))
                                .setPanel2Rb(double.Parse(txtFrame2Rb.Text))
                                .setPanel2Rc(double.Parse(txtFrame2Rc.Text))

                                .setAirSpaceThickness(double.Parse(txtFrameAirSpaceThickness.Text) / 1000);

                        AbstractPanels.Skeleton airSpaceFillerSkeleton = (AbstractPanels.Skeleton)Convert.ToInt32(comboFrameAirSpaceSkeleton.SelectedValue);
                        if (airSpaceFillerSkeleton != AbstractPanels.Skeleton.None)
                        {
                            panels
                                .setAirSpaceFillerSkeleton(airSpaceFillerSkeleton)
                                .setAirSpaceFillerThickness(double.Parse(txtFrameAirSpaceFillerThickness.Text) / 1000)
                                .setAirSpaceFillerDensity(double.Parse(txtFrameAirSpaceFillerDensity.Text));

                            if (airSpaceFillerSkeleton == AbstractPanels.Skeleton.Hard)
                            {
                                panels.setAirSpaceHardSkeletonFillerElasiticityModulus(double.Parse(txtFrameAirSpaceFillerElasticityModulus.Text));
                            }
                        }



                        break;
                    }
            }

            _drawChart();
        }

        private void checkMonolithFillerExist_Checked(object sender, RoutedEventArgs e)
        {
            if (MonolithWallInsulation.CoveringCalcType.Extended == (MonolithWallInsulation.CoveringCalcType) Convert.ToInt32(comboMonolithPanelsCalcTypes.SelectedValue))
                panelMonolithFillerParams.Visibility = System.Windows.Visibility.Visible;
            else
                panelMonolithFillerParams.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void checkMonolithFillerExist_UnChecked(object sender, RoutedEventArgs e)
        {
            panelMonolithFillerParams.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void checkMonolithPanelBindType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Sound.Model.Soundinsulation.MonolithWallInsulation.PanelBindType bindType = (Sound.Model.Soundinsulation.MonolithWallInsulation.PanelBindType)Convert.ToInt32(checkMonolithPanelBindType.SelectedValue);

            switch (bindType)
            {
                case Sound.Model.Soundinsulation.MonolithWallInsulation.PanelBindType.springLinear:
                    panelSpringBindParams.Visibility = System.Windows.Visibility.Visible;
                    stackLathDirection.Visibility = System.Windows.Visibility.Visible;
                    break;

                case Sound.Model.Soundinsulation.MonolithWallInsulation.PanelBindType.springPoint:
                    panelSpringBindParams.Visibility = System.Windows.Visibility.Visible;
                    stackLathDirection.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case Sound.Model.Soundinsulation.MonolithWallInsulation.PanelBindType.rudeLinear:
                    panelSpringBindParams.Visibility = System.Windows.Visibility.Collapsed;
                    stackLathDirection.Visibility = System.Windows.Visibility.Visible;
                    break;

                case Sound.Model.Soundinsulation.MonolithWallInsulation.PanelBindType.rudePoint:
                    panelSpringBindParams.Visibility = System.Windows.Visibility.Collapsed;
                    stackLathDirection.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }

        }

        private void checkSetMonolithMaterialsManually_Checked(object sender, RoutedEventArgs e)
        {
            comboMonolithMaterials.IsEnabled = false;

            txtMonolithDensity.IsEnabled = true;
            txtMonolithCoefficientK.IsEnabled = true;
        }

        private void checkSetMonolithMaterialsManually_UnChecked(object sender, RoutedEventArgs e)
        {
            comboMonolithMaterials.IsEnabled = true;

            txtMonolithDensity.IsEnabled = false;
            txtMonolithCoefficientK.IsEnabled = false;
        }

        private void checkSetMonolithPanelsManually_Checked(object sender, RoutedEventArgs e)
        {
            comboMonolithPanels.IsEnabled = false;

            txtMonolithPanelDensity.IsEnabled = true;
            txtMonolithPanelVelocity.IsEnabled = true;
        }

        private void checkSetMonolithPanelsManually_UnChecked(object sender, RoutedEventArgs e)
        {
            comboMonolithPanels.IsEnabled = true;

            txtMonolithPanelDensity.IsEnabled = false;
            txtMonolithPanelVelocity.IsEnabled = false;
        }

        private void checkSetPanelMaterialManually_Checked(object sender, RoutedEventArgs e)
        {
            comboOnelayerMaterials.IsEnabled = false;

            txtOnelayerDensity.IsEnabled = true;
            txtOnelayerFc.IsEnabled = true;
            txtOnelayerFb.IsEnabled = true;
            txtOnelayerRb.IsEnabled = true;
            txtOnelayerRc.IsEnabled = true;
        }

        private void checkSetPanelMaterialManually_UnChecked(object sender, RoutedEventArgs e)
        {
            comboOnelayerMaterials.IsEnabled = true;

            txtOnelayerDensity.IsEnabled = false;
            txtOnelayerFc.IsEnabled = false;
            txtOnelayerFb.IsEnabled = false;
            txtOnelayerRb.IsEnabled = false;
            txtOnelayerRc.IsEnabled = false;
        }

        private void checkSetFramework1MaterialManually_Checked(object sender, RoutedEventArgs e)
        {
            comboFramework1Materials.IsEnabled = false;

            txtFrame1Density.IsEnabled = true;
            txtFrame1Fc.IsEnabled = true;
            txtFrame1Fb.IsEnabled = true;
            txtFrame1Rb.IsEnabled = true;
            txtFrame1Rc.IsEnabled = true;
        }

        private void checkSetFramework1MaterialManually_UnChecked(object sender, RoutedEventArgs e)
        {
            comboFramework1Materials.IsEnabled = true;

            txtFrame1Density.IsEnabled = false;
            txtFrame1Fc.IsEnabled = false;
            txtFrame1Fb.IsEnabled = false;
            txtFrame1Rb.IsEnabled = false;
            txtFrame1Rc.IsEnabled = false;
        }

        private void checkSetFramework2MaterialManually_Checked(object sender, RoutedEventArgs e)
        {
            comboFramework2Materials.IsEnabled = false;

            txtFrame2Density.IsEnabled = true;
            txtFrame2Fc.IsEnabled = true;
            txtFrame2Fb.IsEnabled = true;
            txtFrame2Rb.IsEnabled = true;
            txtFrame2Rc.IsEnabled = true;
        }

        private void checkSetFramework2MaterialManually_UnChecked(object sender, RoutedEventArgs e)
        {
            comboFramework2Materials.IsEnabled = true;

            txtFrame2Density.IsEnabled = false;
            txtFrame2Fc.IsEnabled = false;
            txtFrame2Fb.IsEnabled = false;
            txtFrame2Rb.IsEnabled = false;
            txtFrame2Rc.IsEnabled = false;
        }

        private void txtMonolithThickness_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.OemPeriod && (e.Key < Key.D0 || e.Key > Key.D9))
                e.Handled = true;
        }

        private void txtMonolithDensity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.OemPeriod && (e.Key < Key.D0 || e.Key > Key.D9))
                e.Handled = true;
        }

        private void txtMonolithCoefficientK_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.OemPeriod && (e.Key < Key.D0 || e.Key > Key.D9))
                e.Handled = true;
        }

        private void comboMonolithPanelsCalcTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MonolithWallInsulation.CoveringCalcType monolithPanelCalcType = (MonolithWallInsulation.CoveringCalcType)Convert.ToInt32(comboMonolithPanelsCalcTypes.SelectedValue);

            switch (monolithPanelCalcType)
            {
                case MonolithWallInsulation.CoveringCalcType.None:
                    stackMonolithPanelsConfiguration.Visibility = System.Windows.Visibility.Collapsed;
                    gridMonolithPanelsSimple.Visibility = System.Windows.Visibility.Collapsed;
                    gridMonolithPanelsExtended.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case MonolithWallInsulation.CoveringCalcType.Simple:
                    stackMonolithPanelsConfiguration.Visibility = System.Windows.Visibility.Visible;
                    gridMonolithPanelsSimple.Visibility = System.Windows.Visibility.Visible;
                    gridMonolithPanelsExtended.Visibility = System.Windows.Visibility.Collapsed;

                    stackMonolithPanelBindTypes.Visibility = System.Windows.Visibility.Collapsed;
                    break;

                case MonolithWallInsulation.CoveringCalcType.Extended:
                    stackMonolithPanelsConfiguration.Visibility = System.Windows.Visibility.Visible;
                    gridMonolithPanelsSimple.Visibility = System.Windows.Visibility.Collapsed;
                    gridMonolithPanelsExtended.Visibility = System.Windows.Visibility.Visible;
                    stackMonolithPanelBindTypes.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        private void frmWallSoundInsulation_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void comboFrameAirSpaceSkeleton_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AbstractPanels.Skeleton skeleton = (AbstractPanels.Skeleton)Convert.ToInt32(comboFrameAirSpaceSkeleton.SelectedValue);
            if (skeleton == AbstractPanels.Skeleton.Hard)
            {
                stackFrameAitSpaceFillerSkeletonParams.Visibility = Visibility.Visible;
                stackFrameAitSpaceFillerHardSkeletonParams.Visibility = Visibility.Visible;
            }
            else if (skeleton == AbstractPanels.Skeleton.Soft)
            {
                stackFrameAitSpaceFillerSkeletonParams.Visibility = Visibility.Visible;
                stackFrameAitSpaceFillerHardSkeletonParams.Visibility = Visibility.Collapsed;
            }
            else
            {
                stackFrameAitSpaceFillerSkeletonParams.Visibility = Visibility.Collapsed;
                stackFrameAitSpaceFillerHardSkeletonParams.Visibility = Visibility.Collapsed;
            }
        }

        private void btbNextWindow_Click(object sender, RoutedEventArgs e)
        {
            // calc
            calc();

            /**
             * hide this window
             */
            Hide();

            /**
             * Show next window 
             */
            Wall.Name frameName = (Wall.Name)Convert.ToInt32(comboWallEnum.SelectedValue);

            frmWallSoundInsulation frmWallInsulation = Core.ServiceLocator.getFramePropertiesWindow(frameName);
            frmWallInsulation.ShowDialog();

            // set configured wall
            Core.ServiceLocator.getWallConnection().setWall(
                frmWallInsulation.getWall()
            );
        }

        private void comboWallEnum_GotFocus(object sender, RoutedEventArgs e)
        {
            // highlight changed frame propery windows
            for (int i = 0; i < comboWallEnum.Items.Count; i++)
            {
                ComboBoxItem item = (ComboBoxItem)comboWallEnum.ItemContainerGenerator.ContainerFromIndex(i);

                Wall.Name wallName = (Wall.Name)Convert.ToInt32( ((System.Xml.XmlElement) item.Content).GetAttribute("Value") );
                frmWallSoundInsulation form = Core.ServiceLocator.getFramePropertiesWindow(wallName);
                
                if(form.isCalculated())
                    item.Background = new SolidColorBrush(Color.FromArgb(255, 255, 100, 20));
            }
        }


    }
}
