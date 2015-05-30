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

using System.Threading;

using Sound.Core.Soundprocessing;

using SignalProcessing.Signal;
using SignalProcessing.Filter;

using Sound.Model;

namespace Sound
{

    /// <summary>
    /// Логика взаимодействия для frmRoom.xaml
    /// </summary>
    public partial class frmRoom : Window
    {
        SoundTrasmittion _transmit = new SoundTrasmittion();

        public frmRoom()
        {
            InitializeComponent();
        }

        private void frmRoom_Loaded(object sender, RoutedEventArgs e)
        {
            // source
            txtSourceSound.Text = Properties.Settings.Default.SampleFilesDir + "\\Sample\\white_noise.wav";

            // dimensions
            Sound.Core.ServiceLocator.getWallConnection()
                .setRoom1Height(Convert.ToDouble(txtRoom1Height.Text))
                .setRoom1Width(Convert.ToDouble(txtRoom1Width.Text))
                .setRoom1Length(Convert.ToDouble(txtRoom1Length.Text))
                .setRoom2Height(Convert.ToDouble(txtRoom2Height.Text))
                .setRoom2Width(Convert.ToDouble(txtRoom2Width.Text))
                .setRoom2Length(Convert.ToDouble(txtRoom2Length.Text));

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

        private void btnOpenWallCoverWindow_Click(object sender, RoutedEventArgs e)
        {
            Core.ServiceLocator.getWallCoverWindow("Room1Floor").ShowDialog();
        }


        private void btnOpenSourceSoundWindow_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".wav";
            dlg.Filter = "Звукові файли WAV (.wav)|*.wav";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == false)
                return;

            txtSourceSound.Text = dlg.FileName;
        }

        private void cmdBuildRoom1NoiseSpectrum_Click(object sender, RoutedEventArgs e)
        {
            // prepare chart form
            frmPeriodogram wnd = new frmPeriodogram();

            // set noise type
            int noiseType = Convert.ToInt32(chkRoom1NoizeType.SelectedValue);

            if (noiseType == -1)
            {
                Wav noiseSignal = new Wav();

                wnd.setSignal(noiseSignal);
            }
            else
            {
                Noise noiseSignal = (new Noise(10000))
                    .setSampleFrequency(22050)
                    .setType( (Noise.NoiseType) noiseType )
                    .setSignalLevel(Convert.ToDouble(txtRoom1NoiseLevel.Text));

                wnd.setSignal(noiseSignal);
            }
            
            // build signal
            wnd.drawChart();
            wnd.Show();
        }

        private void cmdBuildRoom2NoiseSpectrum_Click(object sender, RoutedEventArgs e)
        {
            // prepare chart form
            frmPeriodogram wnd = new frmPeriodogram();

            // set noise type
            int noiseType = Convert.ToInt32(chkRoom2NoizeType.SelectedValue);

            if (noiseType == -1)
            {
                Wav noiseSignal = new Wav();

                wnd.setSignal(noiseSignal);
            }
            else
            {
                Noise noiseSignal = (new Noise(10000))
                    .setSampleFrequency(22050)
                    .setType((Noise.NoiseType)noiseType)
                    .setSignalLevel(Convert.ToDouble(txtRoom2NoiseLevel.Text));

                wnd.setSignal(noiseSignal);
            }

            // build signal
            wnd.drawChart();
            wnd.Show();
        }

        private void btnShowFilterTransmitFunctionWindow_Click(object sender, RoutedEventArgs e)
        {
            Window w = new frmFilterTransmitFunction();

            w.Show();
        }

        private void btnBuildSpektr_Click(object sender, RoutedEventArgs e)
        {
            Wav soundFile = (new Wav())
                .setDecimate(2)
                .normalize()
                .setSignalLevel(Convert.ToDouble(txtUsableSignalLevel.Text))
                .setFilename(txtSourceSound.Text)
                .open();

            frmPeriodogram wnd = new frmPeriodogram();
            wnd.Show();

            // draw
            wnd.setSignal(soundFile);
            wnd.drawChart();
            
        }

        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void cmdSaveRoom1Noise_Click(object sender, RoutedEventArgs e)
        {
            // set noise type
            int noiseType = Convert.ToInt32(chkRoom1NoizeType.SelectedValue);

            Noise noiseSignal = (new Noise(10000))
                .setSampleFrequency(22050)
                .setType((Noise.NoiseType)noiseType)
                .setSignalLevel(Convert.ToDouble(txtRoom2NoiseLevel.Text));

            // choose filename
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".wav";
            dlg.Filter = "Звукові файли WAV (.wav)|*.wav";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == false)
                return;

            // save
            noiseSignal.save(dlg.FileName);
        }

        private void cmdCalculate_Click(object sender, RoutedEventArgs e)
        {
            int sampleFrequency = 22050;

            // noise in room 1
            Noise room1NoiseSignal = (new Noise())
                .setSampleFrequency(sampleFrequency)
                .setType((Noise.NoiseType) Convert.ToInt32(chkRoom1NoizeType.SelectedValue) )
                .setSignalLevel(Convert.ToDouble(txtRoom1NoiseLevel.Text));

            _transmit.setRoom1Noise(room1NoiseSignal);

            // noise in room 2
            Noise room2NoiseSignal = (new Noise())
                .setSampleFrequency(sampleFrequency)
                .setType((Noise.NoiseType)Convert.ToInt32(chkRoom2NoizeType.SelectedValue))
                .setSignalLevel(Convert.ToDouble(txtRoom2NoiseLevel.Text));

            _transmit.setRoom2Noise(room2NoiseSignal);

            // sound
            SoundTrasmittion.SNRCalculationMethod calcMethod = (SoundTrasmittion.SNRCalculationMethod)Convert.ToInt32(comboCalcMethod.SelectedValue);
            _transmit.setSNRCalculationMethod(calcMethod);

            Wav soundFile = (new Wav())
                .setDecimate(2)
                .normalize()
                .setSignalLevel(Convert.ToDouble(txtUsableSignalLevel.Text));

            switch (calcMethod)
            {
                default:
                case SoundTrasmittion.SNRCalculationMethod.Formant:
                    soundFile.setFilename(txtSourceSound.Text);
                    break;

                case SoundTrasmittion.SNRCalculationMethod.RaSTI:
                    soundFile.setFilename(Properties.Settings.Default.SampleFilesDir + "/RaSTI/rasti22k143.wav");
                    break;
            }

            _transmit.setSignal(soundFile.open());

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
                .setRoom1SignalLevel(Convert.ToDouble(txtUsableSignalLevel.Text))
                .setRoom1NoiseLevel(Convert.ToDouble(txtRoom1NoiseLevel.Text))
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

            lblRoom1Width.Text = txtRoom1Width.Text;

            // dimensions
            Sound.Core.ServiceLocator
                .getWallConnection()
                .setRoom1Width(Convert.ToDouble(txtRoom1Width.Text));
        }

        private void txtRoom1Length_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom1Length.Text == "")
                return;

            lblRoom1Length.Text = txtRoom1Length.Text;

            // dimensions
            Sound.Core.ServiceLocator
                .getWallConnection()
                .setRoom1Length(Convert.ToDouble(txtRoom1Length.Text));
        }

        private void txtRoom1Height_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom1Height.Text == "")
                return;

            lblRoom1Height.Text = txtRoom1Height.Text;

            // dimensions
            Sound.Core.ServiceLocator
                .getWallConnection()
                .setRoom1Height(Convert.ToDouble(txtRoom1Height.Text));
        }

        private void txtRoom2Width_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom2Width.Text == "")
                return;

            lblRoom2Width.Text = txtRoom2Width.Text;

            // dimensions
            Sound.Core.ServiceLocator
                .getWallConnection()
                .setRoom2Width(Convert.ToDouble(txtRoom2Width.Text));
        }

        private void txtRoom2Length_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom2Length.Text == "")
                return;

            lblRoom2Length.Text = txtRoom2Length.Text;

            // dimensions
            Sound.Core.ServiceLocator
                .getWallConnection()
                .setRoom2Length(Convert.ToDouble(txtRoom2Length.Text));
        }

        private void txtRoom2Height_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (txtRoom2Height.Text == "")
                return;

            lblRoom2Height.Text = txtRoom2Height.Text;

            // dimensions
            Sound.Core.ServiceLocator
                .getWallConnection()
                .setRoom2Height(Convert.ToDouble(txtRoom2Height.Text));
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

        private void comboCalcMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((SoundTrasmittion.SNRCalculationMethod) Convert.ToInt32(comboCalcMethod.SelectedValue))
            {
                case SoundTrasmittion.SNRCalculationMethod.Formant:
                    stackSourceFile.IsEnabled = true;
                    break;

                case SoundTrasmittion.SNRCalculationMethod.RaSTI:
                    stackSourceFile.IsEnabled = false;
                    break;
            }

            
        }

    }
}
