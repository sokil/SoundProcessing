using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SignalProcessing.Signal;

namespace SignalProcessing.Filter
{
    public class KaiserBandpassChain
    {
        private double[] _chainFrequencies;

        private double[] _chainLevels;

        private double _sampleFrequency;

        private Wav[] _filteredSignalsChain;

        private Wav _filteredSignal;

        private int _N;

        public KaiserBandpassChain setChainFrequencies(double[] frequencies)
        {
            _chainFrequencies = frequencies;

            return this;
        }

        public KaiserBandpassChain setChainLevels(double[] levels)
        {
            _chainLevels = levels;

            return this;
        }

        public KaiserBandpassChain setSampleFrequency(double frequency)
        {
            _sampleFrequency = frequency;

            return this;
        }

        public double getSampleFrequency()
        {
            return _sampleFrequency;
        }

        public KaiserBandpassChain setFilterOrder(int N)
        {
            if (N % 2 == 1)
                N++;

            _N = N;

            return this;
        }

        public int getFilterOrder()
        {
            return _N;
        }

        public KaiserBandpassChain filter<T>(T signal) where T: AbstractSignal<T>
        {
            int frequencyNum = _chainFrequencies.Length;

            // if chain lvels not specified - create default levets equals to 1
            if (_chainLevels == null)
            {
                _chainLevels = new double[frequencyNum];
                for (int i = 0; i < _chainFrequencies.Length; i++)
                {
                    _chainLevels[i] = 1;
                }
            }

            // apply filters
            double leftBoundFrequency, rightBoundFrequency;

            KaiserBandpass filter = (new KaiserBandpass())
                .setSampleFrequency(signal.getSampleFrequency())
                .setFilterOrder(2000);

            _filteredSignalsChain = new Wav[frequencyNum];

            for (int f = 0; f < frequencyNum; f++)
            {
                leftBoundFrequency = _chainFrequencies[f] / 1.12;
                rightBoundFrequency = _chainFrequencies[f] * 1.12;

                filter
                    .setLeftBoundFrequency(leftBoundFrequency)
                    .setRightBoundFrequency(rightBoundFrequency);

                // multiply
                _filteredSignalsChain[f] = (Wav) (filter.filter(signal) * _chainLevels[f]);
            }

            return this;
        }

        
        public Wav[] getFilteredSignalsChain()
        {
            return _filteredSignalsChain;
        }

        public Wav getFilteredSignal()
        {
            if (_filteredSignal != null)
                return _filteredSignal;

            int signalLength = _filteredSignalsChain[0].getSignal().Length;
            double[] filteredSignal = new double[signalLength];

            // aggregate filtered signals
            for (int f = 0; f < _chainFrequencies.Length; f++)
            {
                double[] chainedSignal = _filteredSignalsChain[f].getSignal();

                for (int i = 0; i < signalLength; i++)
                {
                    filteredSignal[i] += chainedSignal[i];
                }
            }

            _filteredSignal = (new Wav())
                .setSignal(filteredSignal);

            return _filteredSignal;
        }
    }
}
