using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SignalProcessing.Filter;

using System.IO;

namespace SignalProcessing.Signal
{
    public class Noise : AbstractSignal<Noise>
    {
        public enum NoiseType { White = 0, Pink = 1, Brown = 2 }

        double[] _noiseFrequencyRange = new double[] { 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150 };

        private int _length;

        // noise type
        private NoiseType _type = NoiseType.White;

        public Noise(int length = 0)
        {
            _length = length;
        }

        public Noise setLength(int length)
        {
            _length = length;

            return this;
        }

        public Noise setType(NoiseType type)
        {
            _type = type;

            return this;
        }

        /// <summary>
        /// Return Noise Characteristic, related to frequencied specified id
        /// </summary>
        /// <param name="noiseType"></param>
        /// <returns></returns>
        private double[] getNoiseCharacteristic(NoiseType noiseType)
        {
            double[] noiseCharacteristic = null;

            switch (noiseType)
            {
                case NoiseType.White:
                    noiseCharacteristic = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                    break;

                case NoiseType.Pink:
                    noiseCharacteristic = new double[] { 1.0, 1.0 / Math.Sqrt(2), 1.0 / 2, 1.0 / (2 * Math.Sqrt(2)), 1.0 / 4, 1.0 / (4 * Math.Sqrt(2)), 1.0 / 8, 1.0 / (8 * Math.Sqrt(2)), 1.0 / 16, 1.0 / (16 * Math.Sqrt(2)), 1.0 / 32, 1.0 / (32 * Math.Sqrt(2)), 1.0 / 64, 1.0 / (64 * Math.Sqrt(2)), 1.0 / 128, 1.0 / (128 * Math.Sqrt(2)) };
                    break;

                case NoiseType.Brown:
                    noiseCharacteristic = new double[] { 1, 1.0 / 2, 1.0 / 4, 1.0 / 8, 1.0 / 16, 1.0 / 32, 1.0 / 64, 1.0 / 128, 1.0 / 256, 1.0 / 512, 1.0 / 1024, 1.0 / 2048, 1.0 / 4096, 1.0 / 8192, 1.0 / 16384, 1.0 / 32768 };
                    break;
            }

            return noiseCharacteristic;
        }

        public override double[] getSignal()
        {
            if (_signal != null)
                return _signal;

            // generate noise
            Wav noise = (new Wav())
                .setStream(SignallProcessing.Properties.Resources.white_noise)
                .setDecimate(2)
                .open();

            // expand or collapse
            if (_length > noise.getCountsNumber())
            {
                noise.expand(_length);
            }
            else if (_length < noise.getCountsNumber())
            {
                noise.trim(_length);
            }

            _signal = noise.getSignal();
            _complexSignal = noise.getComplexSignal();
            _sampleFrequency = noise.getSampleFrequency();

            /*
            _signal = new double[_length];
            _complexSignal = new alglib.complex[_length];
            Random rand = new Random();
            for(int i = 0; i < _length; i++)
            {
                _signal[i] = alglib.invnormaldistribution(rand.NextDouble());
                _complexSignal[i] = (alglib.complex) _signal[i];
            }
             */

            // apply filters
            KaiserBandpassChain filterChain = (new KaiserBandpassChain())
                .setFilterOrder(2000)
                .setSampleFrequency(_sampleFrequency)
                .setChainFrequencies(_noiseFrequencyRange)
                .setChainLevels(getNoiseCharacteristic(_type));

            _complexSignal = filterChain.filter<Noise>(this).getFilteredSignal().getComplexSignal();

            for (int i = 0; i < _complexSignal.Length; i++)
            {
                _signal[i] = alglib.math.abscomplex(_complexSignal[i]);
            }

            _applySignalLevel();

            return _signal;
            
        }
    }
}
