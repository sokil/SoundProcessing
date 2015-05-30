using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model.Soundprocessing
{
    public enum SignalType
    {
        Voise = 0,
        White = 1,
        Pink = 2,
        Brown = 3
    }

    public class SoundTrasmittion
    {
        /**
         * Room1
         */
        private SignalType _room1SignalType;
        private double _room1SignalLevel;

        /**
         * Room2
         */
        private SignalType _room2NoiseType;
        private double _room2NoiseLevel;

        // calculated in getSNR
        private double _room2EquivalentSignalLevel;
        private double _room2EquivalentNoiseLevel;
        private double _room2MedianSignalLevel;
        private double _room2EquivalentSignalNoiseRatio;

        /**
         * Filter
         */
        private FrequencyRange _elementsInFilterChain = FrequencyRange.OneThirdOctave;

        public enum FrequencyRange
        {
            Octave = 6,
            OneThirdOctave = 16
        }

        /**
         * Sound insulation
         */
        private SortedList<double, double> _soundInsulationCharacteristic;
        private bool _withFlankingTransmittion = true;

        private Dictionary<string, SortedList<double, double>> _absorbCoefs = new Dictionary<string, SortedList<double, double>>();

        SortedList<double, double> _room1ReverbTime;
        SortedList<double, double> _room2ReverbTime;

        public SoundTrasmittion setWallAbsorbCoefitients(string wallName, SortedList<double, double> absorbCoefs)
        {
            _absorbCoefs[wallName] = absorbCoefs;

            return this;
        }

        public SoundTrasmittion setRoomReverbTime(int room, SortedList<double, double> reverbTime)
        {
            if(0 == room)
                _room1ReverbTime = reverbTime;

            else
                _room2ReverbTime = reverbTime;

            return this;
        }

        public SoundTrasmittion withFlankingTransmittion(bool trigger = true)
        {
            _withFlankingTransmittion = trigger;
            return this;
        }

        public SoundTrasmittion setRoom1SignalLevel(double level)
        {
            _room1SignalLevel = level;
            return this;
        }

        public double getRoom1SignalLevel()
        {
            return _room1SignalLevel;
        }

        public double getRoom2EquivalentSignalLevel()
        {
            return _room2EquivalentSignalLevel;
        }

        public double getRoom2EquivalentNoiseLevel()
        {
            return _room2EquivalentNoiseLevel;
        }

        public double getRoom2MedianSignalLevel()
        {
            return _room2MedianSignalLevel;
        }

        public double getRoom2EquivalentSignalNoiseRatio()
        {
            return _room2EquivalentSignalNoiseRatio;
        }

        public SoundTrasmittion setRoom1SignalType(SignalType type)
        {
            _room1SignalType = type;
            return this;
        }

        public SoundTrasmittion setRoom2NoiseLevel(double level)
        {
            _room2NoiseLevel = level;
            return this;
        }

        public double getRoom2NoiseLevel()
        {
            return _room2NoiseLevel;
        }

        public SoundTrasmittion setRoom2NoiseType(SignalType type)
        {
            _room2NoiseType = type;
            return this;
        }

        public SoundTrasmittion setElementsInFilterChain(FrequencyRange range)
        {
            _elementsInFilterChain = range;

            return this;
        }

        public FrequencyRange getElementsInFilterChain()
        {
            return _elementsInFilterChain;
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

        /// <summary>
        /// Get frequency related signal levels for White signal in room1
        /// </summary>
        /// <returns></returns>
        protected SortedList<double, double> _getFreqRelatedRoomWhiteSignalLevels(double signalLevel)
        {
            SortedList<double, double> freqRelatedRoomSignalLevel = new SortedList<double, double>();

            double[] frequencyRange = Sound.Core.ServiceLocator.getFrequencyRangeByFilterChainNum(_elementsInFilterChain);

            for (int i = 0; i < frequencyRange.Length; i++)
            {
                freqRelatedRoomSignalLevel[frequencyRange[i]] = signalLevel;
            }

            return freqRelatedRoomSignalLevel;
        }

        /// <summary>
        /// Get frequency related signal levels for Voise signal in room1
        /// </summary>
        /// <returns></returns>
        protected SortedList<double, double> _getFreqRelatedRoomVoiseSignalLevels(double signalLevel)
        {
            SortedList<double, double> freqRelatedRoomSignalLevel = new SortedList<double, double>();

            double[] frequencyRange = Sound.Core.ServiceLocator.getFrequencyRangeByFilterChainNum(_elementsInFilterChain);

            double[] level = null;
            switch (_elementsInFilterChain)
            {
                case FrequencyRange.Octave:
                    level = new double[] { 60, 70, 68, 59.5, 52, 46, 40.5, 36 };

                    break;

                case FrequencyRange.OneThirdOctave:
                    level = new double[] { 57, 60, 63.3, 66.7, 70, 69.3, 68.67, 68, 65.17, 62.33, 59.5, 57, 54.5, 52, 50, 48, 46, 44.17, 42.33, 40.5, 39, 37.5 };
                    break;
            }

            // get K
            double sum = 0;
            for (int i = 0; i < level.Length; i++)
            {
                sum += Math.Pow(10, 0.1 * level[i]);
            }

            double k = 10 * Math.Log10(Math.Pow(10, 0.1 * signalLevel) / sum);

            for (int i = 0; i < frequencyRange.Length; i++)
            {
                freqRelatedRoomSignalLevel[frequencyRange[i]] = level[i] + k;
            }

            return freqRelatedRoomSignalLevel;
        }

        protected SortedList<double, double> _getFreqRelatedRoomColorSignalLevels(double signalLevel, double bandwidth6, double bandwidth16)
        {
            SortedList<double, double> freqRelatedRoomSignalLevel = new SortedList<double, double>();

            double[] frequencyRange = Sound.Core.ServiceLocator.getFrequencyRangeByFilterChainNum(_elementsInFilterChain);

            // Frequency: N-1
            double lastL = 0;
            double shift = (_elementsInFilterChain == FrequencyRange.Octave) ? bandwidth6 : bandwidth16;
            int elementsInFilterChain = (_elementsInFilterChain == FrequencyRange.Octave) ? 6 : 16;

            for (double filterChainNum = 0; filterChainNum < elementsInFilterChain; filterChainNum++)
            {
                lastL += Math.Pow(10, filterChainNum * shift / 10);
            }

            lastL = Math.Pow(10, signalLevel / 10) / lastL;
            lastL = 10 * Math.Log10(lastL);

            freqRelatedRoomSignalLevel[frequencyRange[frequencyRange.Length - 1]] = lastL;

            // Frequency: N-2 ... 0
            for (int i = frequencyRange.Length - 2; i >= 0; i--)
            {
                lastL += (elementsInFilterChain == 6) ? bandwidth6 : bandwidth16;

                freqRelatedRoomSignalLevel[frequencyRange[i]] = lastL;
            }

            return freqRelatedRoomSignalLevel;
        }

        /// <summary>
        /// Get frequency related signal levels for Pink signal in room1
        /// </summary>
        /// <returns></returns>
        protected SortedList<double, double> _getFreqRelatedRoomPinkSignalLevels(double signalLevel)
        {
            return _getFreqRelatedRoomColorSignalLevels(signalLevel, 3, 1);
        }

        /// <summary>
        /// Get frequency related signal levels for Brown signal in room1
        /// </summary>
        /// <returns></returns>
        protected SortedList<double, double> _getFreqRelatedRoomBrownSignalLevels(double signalLevel)
        {
            return _getFreqRelatedRoomColorSignalLevels(signalLevel, 6, 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SortedList<double, double> getSNR()
        {
            SortedList<double, double> snr = new SortedList<double,double>();

            SortedList<double, double> freqRelatedRoom1SignalLevel;
            SortedList<double, double> freqRelatedRoom2NoiseLevel;

            // room 1
            switch (_room1SignalType)
            {
                default:
                case SignalType.White: freqRelatedRoom1SignalLevel  = _getFreqRelatedRoomWhiteSignalLevels(_room1SignalLevel); break;
                case SignalType.Voise: freqRelatedRoom1SignalLevel  = _getFreqRelatedRoomVoiseSignalLevels(_room1SignalLevel); break;
                case SignalType.Pink: freqRelatedRoom1SignalLevel   = _getFreqRelatedRoomPinkSignalLevels(_room1SignalLevel); break;
                case SignalType.Brown: freqRelatedRoom1SignalLevel  = _getFreqRelatedRoomBrownSignalLevels(_room1SignalLevel); break;
            }

            // room 2
            switch (_room2NoiseType)
            {
                default:
                case SignalType.White: freqRelatedRoom2NoiseLevel = _getFreqRelatedRoomWhiteSignalLevels(_room2NoiseLevel); break;
                case SignalType.Voise: freqRelatedRoom2NoiseLevel = _getFreqRelatedRoomVoiseSignalLevels(_room2NoiseLevel); break;
                case SignalType.Pink: freqRelatedRoom2NoiseLevel = _getFreqRelatedRoomPinkSignalLevels(_room2NoiseLevel); break;
                case SignalType.Brown: freqRelatedRoom2NoiseLevel = _getFreqRelatedRoomBrownSignalLevels(_room2NoiseLevel); break;
            }

            // get signal to noise ratio
            SortedList<double, double> sn = new SortedList<double,double>();

            SortedList<double, double> freqRelatedRoom2SignalLevel = new SortedList<double, double>();
            

            SortedList<double, double> soundInsulation = getSeparateWallSoundInsulationCharacteristic();
            foreach (KeyValuePair<double, double> pair in freqRelatedRoom1SignalLevel)
            {
                // consider insulation
                freqRelatedRoom2SignalLevel[pair.Key] = freqRelatedRoom1SignalLevel[pair.Key] - soundInsulation[pair.Key];

                sn[pair.Key] = freqRelatedRoom2SignalLevel[pair.Key] - freqRelatedRoom2NoiseLevel[pair.Key];

                #if DEBUG
                Console.WriteLine("# F:" + pair.Key + " # NL_R2: " + freqRelatedRoom2NoiseLevel[pair.Key] + " ####");
                #endif
            }

            // get equivalent signal level in room 2
            // get median signal level in room 2
            _room2EquivalentSignalLevel = 0;
            _room2EquivalentNoiseLevel = 0;
            _room2EquivalentSignalNoiseRatio = 0;
            _room2MedianSignalLevel = 0;

            // compare equivalent signal level in room1 with equivalent
            double room1EquivalentSignalLevel = 0;

            foreach (KeyValuePair<double, double> pair in freqRelatedRoom2SignalLevel)
            {
                room1EquivalentSignalLevel          += Math.Pow(10, 0.1 * freqRelatedRoom1SignalLevel[pair.Key]);
                _room2EquivalentSignalLevel         += Math.Pow(10, 0.1 * freqRelatedRoom2SignalLevel[pair.Key]);
                _room2EquivalentNoiseLevel          += Math.Pow(10, 0.1 * freqRelatedRoom2NoiseLevel[pair.Key]);
                _room2EquivalentSignalNoiseRatio    += Math.Pow(10, 0.1 * sn[pair.Key]);
                _room2MedianSignalLevel             += freqRelatedRoom2SignalLevel[pair.Key];
            }

            room1EquivalentSignalLevel              = 10 * Math.Log10(room1EquivalentSignalLevel);
            _room2EquivalentSignalLevel             = 10 * Math.Log10( _room2EquivalentSignalLevel );
            _room2EquivalentNoiseLevel              = 10 * Math.Log10(_room2EquivalentNoiseLevel);
            _room2EquivalentSignalNoiseRatio        = 10 * Math.Log10(_room2EquivalentSignalNoiseRatio);
            _room2MedianSignalLevel                 /= freqRelatedRoom2SignalLevel.Count;

            #if DEBUG
            Console.WriteLine("### EQ_SL_R1: " + room1EquivalentSignalLevel + ", SL_R2:" + _room1SignalLevel);
            Console.WriteLine("### EQ_NL_R2: " + _room2EquivalentNoiseLevel + ", NL_R2:" + _room2NoiseLevel);
            #endif

            // configure reverberation
            if (null == _room1ReverbTime || null == _room2ReverbTime)
            {
                Sound.Model.Reverberation reverb = new Reverberation();
                reverb
                    .setFrequencyList(Sound.Core.ServiceLocator.getFrequencyRangeByFilterChainNum(_elementsInFilterChain))
                    .setWallAbsorbCoefitients(_absorbCoefs);

                if(null ==_room1ReverbTime)
                    _room1ReverbTime = reverb.getRoom1ReverbTime();

                if(null == _room2ReverbTime)
                    _room2ReverbTime = reverb.getRoom2ReverbTime();
            }

            // apply modulation - modulated signal-foise ratio 
            double[] frequencyRange = Sound.Core.ServiceLocator.getFrequencyRangeByFilterChainNum(_elementsInFilterChain);
            SortedList<double, double> modulationCoef = new SortedList<double, double>();
            double[] modulationFrequencyList = new double[] { 0.7, 1.4, 2.8, 5.6, 11.2 };

            double room1ReverbTime = 0;
            double room2ReverbTime = 0;

            for (int i = 0; i < frequencyRange.Length; i++)
            {
                double filterChainFreq = frequencyRange[i];

                if(_room1ReverbTime.ContainsKey(filterChainFreq))
                {
                    room1ReverbTime = _room1ReverbTime[filterChainFreq];
                }

                if(_room2ReverbTime.ContainsKey(filterChainFreq))
                {
                    room2ReverbTime = _room2ReverbTime[filterChainFreq];
                }

                // get max reverb time on frequency
                double maxReverbTime = room1ReverbTime > room2ReverbTime
                    ? room1ReverbTime
                    : room2ReverbTime;

                // get modulation coefitients on filter chain frequebcy
                modulationCoef[filterChainFreq] = 0;
                for (int j = 0; j < modulationFrequencyList.Length; j++)
                {
                    double modulationFrequency = modulationFrequencyList[j];

                    double modulationCoefOnFreq = 1 / Math.Sqrt(1 + Math.Pow(2 * Math.PI * modulationFrequency * maxReverbTime / 13.8, 2));
                    modulationCoefOnFreq *= 1 / (1 + Math.Pow(10, -0.1 * sn[filterChainFreq]));

                    modulationCoefOnFreq = 10 * Math.Log10(modulationCoefOnFreq / (1 - modulationCoefOnFreq));

                    modulationCoef[filterChainFreq] += modulationCoefOnFreq;
                }

                modulationCoef[filterChainFreq] /= 5;
            }

            return modulationCoef;
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

            // Formant probability
            double[] frequencyRange = Sound.Core.ServiceLocator.getFrequencyRangeByFilterChainNum(_elementsInFilterChain);
            double[] pv;

            switch (_elementsInFilterChain)
            {
                case FrequencyRange.Octave:
                    pv = new double[] { 0.0053872, 0.027276, 0.11762, 0.21084, 0.30742, 0.25776 };
                    break;

                default:
                case FrequencyRange.OneThirdOctave:
                    pv = new double[] { 0.00089301, 0.0015256, 0.0027589, 0.0047133, 0.0080521, 0.014022, 0.024877, 0.037784, 0.047287, 0.058685, 0.070432, 0.082503, 0.094696, 0.10264, 0.10533, 0.10035 ,0, 0, 0, 0, 0, 0 };
                    break;
            }

            double A = 0;

            for (int i = 0; i < frequencyRange.Length; i++ )
            {
                
                double F = frequencyRange[i];
                double snrVal = SNR[F];

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
                P = (0.78 + 5.46 * Math.Exp(-4.3 * 0.001 * Math.Pow(27.3 - Math.Abs(E), 2))) / (1 + Math.Pow(10, 0.1 * Math.Abs(E)));
                if (E > 0)
                {
                    P = 1 - P;
                }

                // articulation inteligibility
                A += pv[i] * P;
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
