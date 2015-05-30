using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model.Soundinsulation
{
    public class EstimateCharacteristic
    {
        private SortedList<double, double> _estimateCharacteristic = null;
        private SortedList<double, double> _characteristic = null;

        private static SortedList<double, double> _standartEstimateFrequencyCharacteristic16 = new SortedList<double, double>() 
        { 
            {100, 33}, {125, 36}, {160, 39}, {200, 42}, {250, 45}, {315, 48}, {400, 51}, {500, 52},
            {630, 53}, {800, 54}, {1000, 55}, {1250, 56}, {1600, 56}, {2000, 56}, {2500, 56}, {3150, 56}
        };

        private static SortedList<double, double> _standartEstimateFrequencyCharacteristic6 = new SortedList<double, double>() 
        { 
            {125, 36}, {250, 45}, {500, 52}, {1000, 55}, {2000, 56}, {4000, 56}, {8000, 56}, {16000, 56}
        };

        public EstimateCharacteristic(SortedList<double, double> characteristic = null)
        {
            if (null != characteristic)
            {
                setCharacteristic(characteristic);
            }
        }

        public SortedList<double, double> getStarndartEstimateFrequencyCharacteristic()
        {
            SortedList<double, double> freqlist;

            switch (Sound.Core.ServiceLocator.getSoundTransmittion().getElementsInFilterChain())
            {
                default:
                case Sound.Model.Soundprocessing.SoundTrasmittion.FrequencyRange.Octave:
                    freqlist = _standartEstimateFrequencyCharacteristic6;
                    break;

                case Sound.Model.Soundprocessing.SoundTrasmittion.FrequencyRange.OneThirdOctave:
                    freqlist = _standartEstimateFrequencyCharacteristic16;
                    break;
            }

            return freqlist;
        }

        public EstimateCharacteristic setCharacteristic(SortedList<double,double> characteristic)
        {
            int count = getStarndartEstimateFrequencyCharacteristic().Count;
            IEnumerable<KeyValuePair<double, double>> slice = characteristic.Take<KeyValuePair<double, double>>(count);
            Dictionary<double, double> dict = slice.ToDictionary(x => x.Key, x => x.Value);

            _characteristic = new SortedList<double, double>(dict);

            return this;
        }

        private int _getShift()
        {
            int? foundShift = null;

            int shift = 0;

            double sum;

            while (true)
            {
                sum = 0;

                foreach (KeyValuePair<double, double> pair in getStarndartEstimateFrequencyCharacteristic())
                {
                    double deviation = pair.Value - _characteristic[pair.Key] + shift;

                    if (deviation <= 0)
                        continue;

                    sum += deviation;
                }

                if (sum > 32)
                {
                    if (foundShift != null)
                        return (int)foundShift-1;

                    shift--;
                }
                else
                {
                    shift++;
                    foundShift = shift;
                }
            }
        }

        public SortedList<double, double> getEstimateCharacteristic()
        {
            if(null != _estimateCharacteristic)
                return _estimateCharacteristic;

            _estimateCharacteristic = getStarndartEstimateFrequencyCharacteristic();

            int shift = _getShift();

            foreach (double frequency in _estimateCharacteristic.Keys.ToList<double>())
            {
                _estimateCharacteristic[frequency] = _estimateCharacteristic[frequency] + shift;
            }

            return _estimateCharacteristic;
        }

        public double getRw()
        {
            return getEstimateCharacteristic()[500];
        }
    }
}
