using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SignalProcessing.Signal;
using SignalProcessing.Filter;

using Sound.Model;

namespace Sound.Core.Soundprocessing
{
    public class SoundTrasmittion
    {
        private Noise _room1Noise;
        private Noise _room2Noise;

        private Wav _signal;

        public enum SNRCalculationMethod
        {
            Formant     = 1,
            RaSTI       = 2
        }

        private SNRCalculationMethod _snrCalculationMethod = SNRCalculationMethod.Formant;

        double[] _frequencyRange = new double[] { 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150 };

        private SortedList<double, double> _soundInsulationCharacteristic;

        private bool _withFlankingTransmittion = true;

        double _room2SignalWithNoiseVarianceMedian = 0;

        public SoundTrasmittion withFlankingTransmittion(bool trigger = true)
        {
            _withFlankingTransmittion = trigger;

            return this;
        }

        public SoundTrasmittion setRoom1Noise(Noise signal)
        {
            _room1Noise = signal;

            if (_signal != null)
                _room1Noise.setLength(_signal.getCountsNumber());

            return this;
        }

        public SoundTrasmittion setRoom2Noise(Noise signal)
        {
            _room2Noise = signal;

            if (_signal != null)
                _room2Noise.setLength(_signal.getCountsNumber());

            return this;
        }

        public SoundTrasmittion setSignal(Wav signal)
        {
            _signal = signal;

            if(_room1Noise != null)
                _room1Noise.setLength(_signal.getCountsNumber());

            if(_room2Noise != null)
                _room2Noise.setLength(_signal.getCountsNumber());

            return this;
        }

        public SoundTrasmittion setSeparateWallSoundInsulationCharacteristic(SortedList<double, double> soundInsulationCharacteristic)
        {
            _soundInsulationCharacteristic = soundInsulationCharacteristic;

            return this;
        }

        public SortedList<double, double> getSeparateWallSoundInsulationCharacteristic()
        {
            if (!_withFlankingTransmittion)
                return _soundInsulationCharacteristic;

            return Core.ServiceLocator
                .getWallConnection()
                .applyFlankingTransmittion(_soundInsulationCharacteristic);
        }

        public Wav[] getRoom2NoiseSignalsChain()
        {
            _room2Noise.setLength(_signal.getCountsNumber());

            // apply filter to sound
            KaiserBandpassChain filter = new KaiserBandpassChain();
            filter
                .setFilterOrder(1000)
                .setSampleFrequency(_room2Noise.getSampleFrequency())
                .setChainFrequencies(_frequencyRange)
                .setSampleFrequency(_room2Noise.getSampleFrequency());

            return filter.filter(_room2Noise).getFilteredSignalsChain();
        }

        public SoundTrasmittion setSNRCalculationMethod(SNRCalculationMethod calcMethod)
        {
            _snrCalculationMethod = calcMethod;

            return this;
        }

        public SortedList<double, double> getSNR()
        {
            switch (_snrCalculationMethod)
            {
                default:
                case SNRCalculationMethod.Formant:
                    return _getFormantSNR();

                case SNRCalculationMethod.RaSTI:
                    return _getRaSTISNR();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f">4 frequencies</param>
        /// <returns></returns>
        private double getRaSTI_SNR(Wav wav, double[] freqencies, double coef)
        {
            double[] signal = wav.getSignal();
            int signalLength = signal.Length;

            alglib.complex[] complexBuff = new alglib.complex[signalLength];
            for (int tickNum = 0; tickNum < signalLength; tickNum++)
            {
                complexBuff[tickNum] = (alglib.complex)Math.Pow(signal[tickNum], 2);
            }

            // fft
            alglib.fftc1d(ref complexBuff);

            double SNR = 0;
            double aik0 = alglib.math.abscomplex(complexBuff[0]);
            double aik, m;

            for (int i = 0; i < freqencies.Length; i++)
            {
                double f = freqencies[i];
                int tick = (int)Math.Round(f * complexBuff.Length / wav.getSampleFrequency());

                aik = alglib.math.abscomplex(complexBuff[tick]) / aik0;
                m = aik / coef;

                SNR += 10 * Math.Log10(m / (1 - m));
            }

            SNR /= freqencies.Length;

            return SNR;
        }

        private SortedList<double, double> _getRaSTISNR()
        {
            Wav[] room2SignalWithNoise = getRoom2Signal(_signal.mix(_room1Noise));

            _room2SignalWithNoiseVarianceMedian = 0;

            double signal500SNR = getRaSTI_SNR(
                room2SignalWithNoise[Array.IndexOf(_frequencyRange, 500)].trimSeconds(10),
                new double[] {1,2,4,8},
                0.2
            );

            double signal2000SNR = getRaSTI_SNR(
                room2SignalWithNoise[Array.IndexOf(_frequencyRange, 2000)],
                new double[] { 0.7, 1.4, 2.8, 5.6, 11.2 },
                0.16
            );

            // pow

            return new SortedList<double, double> { {500, signal500SNR}, {2000, signal2000SNR} };
        }

        private SortedList<double, double> _getFormantSNR()
        {
            // ------------ DEBUG
            #if DEBUG
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@ Singal in room 2 from noise in room 1: ");
            #endif
            // ------------ DEBUG

            Wav[] room2OnlyNoise = getRoom2Signal(_room1Noise);

            // ------------ DEBUG
            #if DEBUG
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@ Singal in room 2 of noise and signal mix from room 1: ");
            #endif
            // ------------ DEBUG

            Wav[] room2SignalWithNoise = getRoom2Signal(_signal.mix(_room1Noise));

            SortedList<double, double> SNR = new SortedList<double, double>();

            _room2SignalWithNoiseVarianceMedian = 0;

            for(int f = 0; f < _frequencyRange.Length; f++)
            {
                double F = _frequencyRange[f];

                double room2OnlyNoiseVariance = room2OnlyNoise[f].getVariance();
                double room2SignalWithNoiseVariance = room2SignalWithNoise[f].getVariance();

                _room2SignalWithNoiseVarianceMedian += room2SignalWithNoiseVariance;

                SNR[F] = 10 * Math.Log10((room2SignalWithNoiseVariance - room2OnlyNoiseVariance) / room2OnlyNoiseVariance);
            }

            _room2SignalWithNoiseVarianceMedian /= _frequencyRange.Length;

            _room2SignalWithNoiseVarianceMedian = 10 * Math.Log10(_room2SignalWithNoiseVarianceMedian / Math.Pow(10, -11));

            return SNR;
        }

        public double getRoom2SignalLevel()
        {
            if(0 == _room2SignalWithNoiseVarianceMedian)
                throw new Exception("SNR must be calculated");

            return _room2SignalWithNoiseVarianceMedian;
        }

        /// <summary>
        /// DEBUG
        /// </summary>
        /// <param name="filteredSignals"></param>
        private void _debugCalcSignal(Wav[] filteredSignals, string message)
        {
            Wav signal = Wav.fromChain(filteredSignals);
            double variance = 20 * Math.Log10(Math.Sqrt(signal.getVariance()) / Math.Pow(10, -6));
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@ " + message + ": " + Convert.ToString(Convert.ToString(variance)));
        }

        private void _debugCalcSignal<T>(T signal, string message) where T: AbstractSignal<T>
        {
            double variance = 20 * Math.Log10(Math.Sqrt(signal.getVariance()) / Math.Pow(10, -6));
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@ " + message + ": " + Convert.ToString(Convert.ToString(variance)));
        }

        public Wav[] getRoom2Signal<T>(T signal) where T: AbstractSignal<T>
        {
            // ------------ DEBUG
            #if DEBUG
            _debugCalcSignal(signal, "Variance of original signal");
            #endif
            // ------------ DEBUG

            // apply filter to sound
            KaiserBandpassChain filter = new KaiserBandpassChain();
            filter
                .setFilterOrder(1000)
                .setChainFrequencies(_frequencyRange)
                .setSampleFrequency(signal.getSampleFrequency());

            Wav[] filteredSignals = filter.filter(signal).getFilteredSignalsChain();

            // prepare chain of signals convoluted with first room's transmit function
            alglib.complex[][] reverberatedChain = new alglib.complex[filteredSignals.Length][];

            // first room's transmit function 
            WallSprandTransmitFunction sprandTransmitFunction = Core.ServiceLocator
                .getWallSprandTransmitFunction()
                .setSampleFrequency(_signal.getSampleFrequency());

            alglib.complex[][] room1SprantTransmitFunction = null;
            alglib.complex[][] room2SprantTransmitFunction = null;

            try
            {
                room1SprantTransmitFunction = sprandTransmitFunction.getRoom1TransmitFunction();
                room2SprantTransmitFunction = sprandTransmitFunction.getRoom2TransmitFunction();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }

            // ------------ DEBUG
            #if DEBUG
            _debugCalcSignal(filteredSignals, "Variance of filtered signal before adding reverb in room 1");
            #endif
            // ------------ DEBUG

            // adding reverb
            for (int f = 0; f < filteredSignals.Length; f++)
            {
                filteredSignals[f].addReverberation(room1SprantTransmitFunction[f]);
            }

            // ------------ DEBUG
            #if DEBUG
            _debugCalcSignal(filteredSignals, "Variance of filtered signal after adding reverb in room 1");
            #endif
            // ------------ DEBUG

            // calculate ci from sound insulation characteristic (Rw)
            SortedList<double, double> ci = new SortedList<double, double>();
            foreach (KeyValuePair<double, double> r in getSeparateWallSoundInsulationCharacteristic())
            {
                ci[r.Key] = 1 / Math.Pow(10, r.Value / 20);
            }
            // get 16 chains of noise in room 2
            Wav[] room2NoiseSignalsChain = getRoom2NoiseSignalsChain();

            // multiply ci with related signal
            for (int f = 0; f < _frequencyRange.Length; f++)
            {
                filteredSignals[f] = (Wav)(filteredSignals[f] * ci[_frequencyRange[f]]);
            }

            // ------------ DEBUG
            #if DEBUG
            _debugCalcSignal(filteredSignals, "Variance of filtered signal after Ci");
            #endif
            // ------------ DEBUG

            for (int f = 0; f < _frequencyRange.Length; f++)
            {
                // add noise
                filteredSignals[f].mix(room2NoiseSignalsChain[f]);
            }

            // ------------ DEBUG
            #if DEBUG
            _debugCalcSignal(filteredSignals, "Variance of filtered signal after room 2 noise mix");
            #endif
            // ------------ DEBUG

            for (int f = 0; f < _frequencyRange.Length; f++)
            {
                // reverberate on room 2
                filteredSignals[f].addReverberation(room2SprantTransmitFunction[f]);
            }

            // ------------ DEBUG
            #if DEBUG
            _debugCalcSignal(filteredSignals, "Variance of filtered signal after reverb in room 2");
            #endif
            // ------------ DEBUG

            return filteredSignals;
        }

        /// <summary>
        /// 1. signal + noise => room2
        /// 2. noise => room2
        /// 3. v1 = getVariance(1), v2 = getVariance(2)
        /// 4. sign/noise = 10 * log10 (v1 - v2 / v2)
        /// 5. razb1(Q)
        /// </summary>
        /// <returns></returns>
        public double getArticulation()
        {
            SortedList<double, double> SNR = getSNR();
                
            double E;
            double P;
            double pv;

            double A = 0;

            foreach (KeyValuePair<double, double> snr in SNR)
            {
                double F = snr.Key;
                double snrVal = snr.Value;

                double Fright = F * 1.12;
                double Fleft = F / 1.12;

                // Sensitivity levels
                if (F < 1000)
                {
                    E = snrVal - 200 / Math.Pow(F, 0.43) + 0.37;
                }
                else
                {
                    E = snrVal - (1.37 + 1000 / Math.Pow(F, 0.69));
                }

                // Perception coefitient
                P = (0.78 + 5.46 * Math.Exp(-4.3 * 0.001 * Math.Pow(27.3 - Math.Abs(E), 2)))/ (1 + Math.Pow(10, 0.1 * Math.Abs(E)));
                if (E > 0)
                {
                    P = 1 - P;
                }

                // Formant probability
                if (F < 400)
                {
                    pv = 2.57 * Math.Pow(10, -8) * (Math.Pow(Fright, 2.4) - Math.Pow(Fleft, 2.4));
                }
                else
                {
                    pv = 1.074 * (Math.Exp(-0.0001 * Math.Pow(Fleft, 1.18)) - Math.Exp(-0.0001 * Math.Pow(Fright, 1.18)));
                }

                // articulation inteligibility
                A += pv * P;
            }

            // inteligibility of words
            if (A < 0.15)
            {
                return 1.54 * Math.Pow(A, 0.25) * (1 - Math.Exp(-11 * A));
            }
            else
            {
                return 1 - Math.Exp( -11 * A / (1 + 0.7 * A) );
            }
        }
    }
}
