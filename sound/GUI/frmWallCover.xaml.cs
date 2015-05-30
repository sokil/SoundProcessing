using System;
using System.Collections;
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

namespace Sound
{
    /// <summary>
    /// Логика взаимодействия для frmWallCover.xaml
    /// </summary>
    public partial class frmWallCover : Window
    {
        private int _room;

        private static Covering _materialsModel;

        private static DataRowCollection _materials;

        Dictionary<string, string> _walls = new Dictionary<string, string>();

        List<object[]> _dataGridDataSource = new List<object[]>();

        public frmWallCover(int room)
        {
            _room = room;

            InitializeComponent();

            if (0 == room)
            {
                _walls["Room1Floor"]="Підлога першої камнати";
                lblWallName1.Content = _walls["Room1Floor"];

                _walls["Room1Seiling"]="Стеля першої камнати";
                lblWallName2.Content = _walls["Room1Seiling"];

                _walls["Room1BackWall"]="Задня стіна першої камнати";
                lblWallName3.Content = _walls["Room1BackWall"];
                
                _walls["Room1FrontWall"]="Передня стіна першої камнати";
                lblWallName4.Content = _walls["Room1FrontWall"];
                
                _walls["Room1LeftWall"]="Ліва стіна першої кімнати";
                lblWallName5.Content = _walls["Room1LeftWall"];
                
                _walls["Room1SeparateWall"]="Суміжня стіна першої кімнати";
                lblWallName6.Content = _walls["Room1SeparateWall"];
            }
            else
            {
                _walls["Room2Floor"]="Підлога другої кімнати";
                lblWallName1.Content = _walls["Room2Floor"];

                _walls["Room2Seiling"]="Стеля другої кімнати";
                lblWallName2.Content = _walls["Room2Seiling"];

                _walls["Room2BackWall"]="Задня стіна другої кімнати";
                lblWallName3.Content = _walls["Room2BackWall"];

                _walls["Room2FrontWall"]="Передня стіна другої камнати";
                lblWallName4.Content = _walls["Room2FrontWall"];

                _walls["Room2SeparateWall"]="Суміжня стіна другої кімнати";
                lblWallName5.Content = _walls["Room2SeparateWall"];

                _walls["Room2RightWall"]="Права стіна другої кімнати";
                lblWallName6.Content = _walls["Room2RightWall"];
            }

            // define room name in label
            this.Title += " Кімната " + Convert.ToString(1 + room);

            // init cover materials
            if (null == _materialsModel)
            {
                _materialsModel = Sound.Core.ServiceLocator.getCoveringMaterials();
                _materials = _materialsModel.getAll();
            }

            comboWallCovers1.DataContext = _materialsModel;
            comboWallCovers2.DataContext = _materialsModel;
            comboWallCovers3.DataContext = _materialsModel;
            comboWallCovers4.DataContext = _materialsModel;
            comboWallCovers5.DataContext = _materialsModel;
            comboWallCovers6.DataContext = _materialsModel;

            // init data grid headers
            DataGridColumn column;
            int colNum = 0;
            foreach(DataColumn c in _materials[0].Table.Columns)
            {
                if (!c.ColumnName.Contains("freq_"))
                    continue;
                
                column = new DataGridTextColumn()
                {
                    Header = c.ColumnName.Substring(5),
                    Binding = new Binding(".[" + c.Ordinal + "]")
                };

                dataGridAbsorbedCoefs.Columns.Add(column);
                
                column = new DataGridTextColumn()
                {
                    Header = c.ColumnName.Substring(5),
                    Binding = new Binding(".[" + colNum++ + "]")
                };

                dataGridReverbTime.Columns.Add(column);
            }

            // bind data
            for (int i = 0; i < 6; i++)
            {
                _dataGridDataSource.Add(_materials[0].ItemArray);
            }

            _renderAbsorbDataGrid();

            dataGridReverbTime.ItemsSource = new List<object[]> { Enumerable.Repeat<object>(0.5, dataGridReverbTime.Columns.Count).ToArray<object>() };
        }

        private void frmWallCover_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void _renderAbsorbDataGrid()
        {
            if (_dataGridDataSource.Count != 6)
                return;

            dataGridAbsorbedCoefs.ItemsSource = _dataGridDataSource;
            dataGridAbsorbedCoefs.Items.Refresh();
        }

        private void comboWallCovers1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // show data in table
            DataRowView row = (DataRowView) comboWallCovers1.SelectedItem;
            _dataGridDataSource[0] = row.Row.ItemArray;
            _renderAbsorbDataGrid();
        }

        private void comboWallCovers2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // show data in table
            DataRowView row = (DataRowView)comboWallCovers2.SelectedItem;
            _dataGridDataSource[1] = row.Row.ItemArray;
            _renderAbsorbDataGrid();
        }

        private void comboWallCovers3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // show data in table
            DataRowView row = (DataRowView)comboWallCovers3.SelectedItem;
            _dataGridDataSource[2] = row.Row.ItemArray;
            _renderAbsorbDataGrid();
        }

        private void comboWallCovers4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // show data in table
            DataRowView row = (DataRowView)comboWallCovers4.SelectedItem;
            _dataGridDataSource[3] = row.Row.ItemArray;
            _renderAbsorbDataGrid();
        }

        private void comboWallCovers5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // show data in table
            DataRowView row = (DataRowView)comboWallCovers5.SelectedItem;
            _dataGridDataSource[4] = row.Row.ItemArray;
            _renderAbsorbDataGrid();
        }

        private void comboWallCovers6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // show data in table
            DataRowView row = (DataRowView)comboWallCovers6.SelectedItem;
            _dataGridDataSource[5] = row.Row.ItemArray;
            _renderAbsorbDataGrid();
        }

        private void _storeReverbTime()
        {
            SortedList<double, double> reverbTime = new SortedList<double, double>();

            DataGridRow row = (DataGridRow)dataGridAbsorbedCoefs.ItemContainerGenerator.ContainerFromIndex(0);
            if (row == null)
            {
                dataGridAbsorbedCoefs.UpdateLayout();
                dataGridAbsorbedCoefs.ScrollIntoView(dataGridAbsorbedCoefs.Items[0]);
                row = (DataGridRow)dataGridAbsorbedCoefs.ItemContainerGenerator.ContainerFromIndex(0);
            }

            foreach (DataGridTextColumn column in dataGridAbsorbedCoefs.Columns)
            {
                TextBlock textBlock = (TextBlock)column.GetCellContent(row);
                reverbTime[Convert.ToDouble(column.Header)] = Convert.ToDouble(textBlock.Text.Replace('.', ','));
            }

            Core.ServiceLocator.getSoundTransmittion().setRoomReverbTime (_room, reverbTime);
        }

        // put to absorb model
        private void _storeAbsorbCoefs()
        {
            SortedList<double, double> absorbCoefs;

            Dictionary<string, string>.Enumerator enumerator = _walls.GetEnumerator();

            for (int i = 0; i < 6; i++)
            {
                enumerator.MoveNext();
                KeyValuePair<string, string> pair = enumerator.Current;

                string wallName = pair.Key;
                string wallCaption = pair.Value;

                // coefs specified manually - get from dataGridAbsorbedCoefs
                if ((bool) checkManualWallCover.IsChecked)
                {
                    absorbCoefs = new SortedList<double, double>();

                    DataGridRow row = (DataGridRow)dataGridAbsorbedCoefs.ItemContainerGenerator.ContainerFromIndex(0);
                    if (row == null)
                    {
                        dataGridAbsorbedCoefs.UpdateLayout();
                        dataGridAbsorbedCoefs.ScrollIntoView(dataGridAbsorbedCoefs.Items[0]);
                        row = (DataGridRow)dataGridAbsorbedCoefs.ItemContainerGenerator.ContainerFromIndex(0);
                    }

                    foreach (DataGridTextColumn column in dataGridAbsorbedCoefs.Columns)
                    {
                        TextBlock textBlock = (TextBlock)column.GetCellContent(row);
                        absorbCoefs[Convert.ToDouble(column.Header)] = Convert.ToDouble(textBlock.Text.Replace('.', ','));
                    }
                }

                // get from DB
                else
                {
                    DataRowView row = (DataRowView)comboWallCovers1.SelectedItem;

                    int rowid = Convert.ToInt32(row["ROWID"]);
                    absorbCoefs = Sound.Core.ServiceLocator.getCoveringMaterials().getRow(rowid);
                }

                Core.ServiceLocator.getSoundTransmittion().setWallAbsorbCoefitients(wallName, absorbCoefs);
            }
        }

        private void cmdOk_Click(object sender, RoutedEventArgs e)
        {
            if (checkManualReverbTime.IsChecked == true)
            {
                _storeReverbTime();
            }
            else
            {
                _storeAbsorbCoefs();
            }
            

            Hide();
        }

        private void checkManualWallCover_Checked(object sender, RoutedEventArgs e)
        {
            comboWallCovers1.IsEnabled = false;
            comboWallCovers2.IsEnabled = false;
            comboWallCovers3.IsEnabled = false;
            comboWallCovers4.IsEnabled = false;
            comboWallCovers5.IsEnabled = false;
            comboWallCovers6.IsEnabled = false;

            dataGridAbsorbedCoefs.IsReadOnly = false;
        }

        private void checkManualWallCover_Unchecked(object sender, RoutedEventArgs e)
        {
            comboWallCovers1.IsEnabled = true;
            comboWallCovers2.IsEnabled = true;
            comboWallCovers3.IsEnabled = true;
            comboWallCovers4.IsEnabled = true;
            comboWallCovers5.IsEnabled = true;
            comboWallCovers6.IsEnabled = true;

            dataGridAbsorbedCoefs.IsReadOnly = true;
        }

        private void checkManualReverbTime_Checked(object sender, RoutedEventArgs e)
        {
            panelAbsorb.IsEnabled = false;

            dataGridReverbTime.IsEnabled = true;
        }

        private void checkManualReverbTime_Unchecked(object sender, RoutedEventArgs e)
        {
            panelAbsorb.IsEnabled = true;

            dataGridReverbTime.IsEnabled = false;
        }
    }
}
