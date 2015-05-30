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
using Sound.Model.Soundprocessing;

namespace Sound
{

    /// <summary>
    /// Логика взаимодействия для frmRoom.xaml
    /// </summary>
    public partial class frmRoom : Window
    {
        SoundTrasmittion _transmit = Core.ServiceLocator.getSoundTransmittion();

        public frmRoom()
        {
            InitializeComponent();
        }

        private void frmRoom_Loaded(object sender, RoutedEventArgs e)
        {
            // dimensions
            Sound.Core.ServiceLocator.getWallConnection()
                .setRoom1Height(Convert.ToDouble(txtRoom1Height.Text))
                .setRoom1Width(Convert.ToDouble(txtRoom1Width.Text))
                .setRoom1Length(Convert.ToDouble(txtRoom1Length.Text))
                .setRoom2Height(Convert.ToDouble(txtRoom2Height.Text))
                .setRoom2Width(Convert.ToDouble(txtRoom2Width.Text))
                .setRoom2Length(Convert.ToDouble(txtRoom2Length.Text));

            _transmit.setElementsInFilterChain( (SoundTrasmittion.FrequencyRange) Convert.ToInt32(comboElementsInFilterChanin.Text));

        }

        private void frmRoom_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnOpenFramePropertiesWindow_Click(object sender, RoutedEventArgs e)
        {
            // show first form from list
            Core.ServiceLocator.getFramePropertiesWindow(Wall.Name.Room1Floor).ShowDialog();
        }

        private void btnOpenRoom1WallCoverWindow_Click(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallCoverWindow(0).ShowDialog();
        }

        private void btnOpenRoom2WallCoverWindow_Click(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallCoverWindow(1).ShowDialog();
        }

        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void cmdCalculate_Click(object sender, RoutedEventArgs e)
        {
            // Room1
            _transmit.setRoom1SignalLevel(Convert.ToDouble(txtUsableRoom1SignalLevel.Text));
            _transmit.setRoom1SignalType((SignalType) Convert.ToInt32(chkRoom1SignalType.SelectedValue));

            // Room2
            _transmit.setRoom2NoiseLevel(Convert.ToDouble(txtRoom2NoiseLevel.Text));
            _transmit.setRoom2NoiseType((SignalType)Convert.ToInt32(chkRoom2NoizeType.SelectedValue));

            // set sound insultaion characteristic (rw)
            try
            {
                Sound.Model.Soundinsulation.AbstractWallInsulation wallInsulation = Core.ServiceLocator
                    .getFramePropertiesWindow(Wall.Name.SeparateWall)
                    .getWall()
                    .getSoundInsulation();

                // r
                SortedList<double, double>ferquencyCharacteristic = wallInsulation
                    .getFrequencyCharacteristic();

                _transmit.setSeparateWallSoundInsulationCharacteristic(ferquencyCharacteristic);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не задані параметри звукоізоляції перегородок. " + ex.Message);
                return;
            }

            new frmResults()
                .setRoom2NoiseLevel(Convert.ToDouble(txtRoom2NoiseLevel.Text))
                .setSeparateWall(Core.ServiceLocator.getFramePropertiesWindow(Wall.Name.SeparateWall).getWall())
                .setTransmit(_transmit)
                .ShowDialog();
        }
            

        private void txtRoom1Width_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom1Width.Text == "")
                return;

            try
            {
                double size = Convert.ToDouble(txtRoom1Width.Text.Replace('.',','));

                lblRoom1Width.Text = Convert.ToString(size) + "м";

                // dimensions
                Sound.Core.ServiceLocator
                    .getWallConnection()
                    .setRoom1Width(size);
            }
            catch (FormatException ex) { }
            
        }

        private void txtRoom1Length_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom1Length.Text == "")
                return;

            try
            {
                double size = Convert.ToDouble(txtRoom1Length.Text.Replace('.', ','));

                lblRoom1Length.Text = Convert.ToString(size) + "м";

                // dimensions
                Sound.Core.ServiceLocator
                    .getWallConnection()
                    .setRoom1Width(size);
            }
            catch (FormatException ex) { }
        }

        private void txtRoom1Height_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom1Height.Text == "")
                return;

            try
            {
                double size = Convert.ToDouble(txtRoom1Height.Text.Replace('.', ','));

                lblRoom1Height.Text = Convert.ToString(size) + "м";

                // dimensions
                Sound.Core.ServiceLocator
                    .getWallConnection()
                    .setRoom1Width(size);
            }
            catch (FormatException ex) { }
        }

        private void txtRoom2Width_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom2Width.Text == "")
                return;

            try
            {
                double size = Convert.ToDouble(txtRoom2Width.Text.Replace('.', ','));

                lblRoom2Width.Text = Convert.ToString(size) + "м";

                // dimensions
                Sound.Core.ServiceLocator
                    .getWallConnection()
                    .setRoom1Width(size);
            }
            catch (FormatException ex) { }
        }

        private void txtRoom2Length_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom2Length.Text == "")
                return;

            try
            {
                double size = Convert.ToDouble(txtRoom2Length.Text.Replace('.', ','));

                lblRoom2Length.Text = Convert.ToString(size) + "м";

                // dimensions
                Sound.Core.ServiceLocator
                    .getWallConnection()
                    .setRoom1Width(size);
            }
            catch (FormatException ex) { }
        }

        private void txtRoom2Height_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom2Height.Text == "")
                return;

            try
            {
                double size = Convert.ToDouble(txtRoom2Height.Text.Replace('.', ','));

                lblRoom2Height.Text = Convert.ToString(size) + "м";

                // dimensions
                Sound.Core.ServiceLocator
                    .getWallConnection()
                    .setRoom1Width(size);
            }
            catch (FormatException ex) { }
        }

        private void chkTopConnection_Checked(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallConnection().setWallConnectionFlexibility(WallConnection.SeparateWallConnectPosition.Top, true);
        }

        private void chkBackConnection_Checked(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallConnection().setWallConnectionFlexibility(WallConnection.SeparateWallConnectPosition.Back, true);
        }

        private void chkBottomConnection_Checked(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallConnection().setWallConnectionFlexibility(WallConnection.SeparateWallConnectPosition.Bottom, true);
        }

        private void chkFrontConnection_Checked(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallConnection().setWallConnectionFlexibility(WallConnection.SeparateWallConnectPosition.Front, true);
        }

        private void chkFrontConnection_Unchecked(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallConnection().setWallConnectionFlexibility(WallConnection.SeparateWallConnectPosition.Top, false);
        }

        private void chkBottomConnection_Unchecked(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallConnection().setWallConnectionFlexibility(WallConnection.SeparateWallConnectPosition.Back, false);
        }

        private void chkBackConnection_Unchecked(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallConnection().setWallConnectionFlexibility(WallConnection.SeparateWallConnectPosition.Bottom, false);
        }

        private void chkTopConnection_Unchecked(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallConnection().setWallConnectionFlexibility(WallConnection.SeparateWallConnectPosition.Front, false);
        }

        private void comboElementsInFilterChanin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            _transmit.setElementsInFilterChain( (SoundTrasmittion.FrequencyRange) Convert.ToInt32(comboElementsInFilterChanin.SelectedValue));
        }

    }
}
