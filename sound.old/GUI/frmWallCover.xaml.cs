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

namespace Sound
{
    /// <summary>
    /// Логика взаимодействия для frmWallCover.xaml
    /// </summary>
    public partial class frmWallCover : Window
    {
        private string _wallName;

        private static Covering _materialsModel;

        private static DataRowCollection _materials;

        private bool _isSet = false;

        public frmWallCover(string wallName)
        {
            InitializeComponent();

            _wallName = wallName;

            // init cover materials
            if (null == _materialsModel)
            {
                _materialsModel = Sound.Core.ServiceLocator.getCoveringMaterials();
                _materials = _materialsModel.getAll();
            }

            comboWallCovers.DataContext = _materialsModel;

            // init data grid headers
            DataGridColumn column;

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
            }

            // define wall name in label
            lblWallName.Content = wallName;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            _isSet = true;

            for (int i = 0; i < comboWallCoverEnum.Items.Count; i++)
            {
                System.Xml.XmlElement item = (System.Xml.XmlElement)comboWallCoverEnum.Items[i];

                string comboItemWallName = Convert.ToString(item.GetAttribute("Value"));

                if (comboItemWallName != _wallName)
                    continue;

                int nextComboItemId = ++i;

                if (nextComboItemId == comboWallCoverEnum.Items.Count)
                {
                    nextComboItemId = 0;
                }

                comboWallCoverEnum.SelectedIndex = nextComboItemId;
                break;
            }
        }

        public bool isSet()
        {
            return _isSet;
        }

        private void frmWallCover_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        private void comboWallCovers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = (DataRowView) comboWallCovers.SelectedItem;

            dataGridAbsorbedCoefs.ItemsSource = new object[][] { row.Row.ItemArray } ;

            // put to absorb model
            int rowid = Convert.ToInt32(row["ROWID"]);

            SortedList<double, double> absorbCoefs = Sound.Core.ServiceLocator
                .getCoveringMaterials()
                .getRow(rowid);

            Core.ServiceLocator.getWallSprandTransmitFunction().setWallAbsorbCoefitients(_wallName, absorbCoefs);
            
        }

        private void cmdOk_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void cmdOk_Next(object sender, RoutedEventArgs e)
        {
            Hide();

            Core.ServiceLocator
                .getWallCoverWindow( Convert.ToString(comboWallCoverEnum.SelectedValue) )
                .ShowDialog();
        }

        private void comboWallCoverEnum_GotFocus(object sender, RoutedEventArgs e)
        {
            // highlight changed frame propery windows
            for (int i = 0; i < comboWallCoverEnum.Items.Count; i++)
            {
                ComboBoxItem item = (ComboBoxItem)comboWallCoverEnum.ItemContainerGenerator.ContainerFromIndex(i);

                string wallName = Convert.ToString(((System.Xml.XmlElement)item.Content).GetAttribute("Value"));
                frmWallCover form = Core.ServiceLocator.getWallCoverWindow(wallName);

                if (form.isSet())
                    item.Background = new SolidColorBrush(Color.FromArgb(255, 255, 100, 20));
            }
        }
    }
}
