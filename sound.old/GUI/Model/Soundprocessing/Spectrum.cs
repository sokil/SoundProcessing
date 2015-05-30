using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SignalProcessing.Filter;

namespace Sound.Core.Soundprocessing
{
    class Spectrum
    {
        int _sampleFrequency;

        int _periodogramSegmentsPerSampleFrequency = 8;

        alglib.complex[] _signal;

        protected double getWindowFunctionValue(double k, int ticks, int alpha = 5)
        {
            double besselAlpha = alglib.besseli0(alpha);

            double betta_k = alpha * Math.Sqrt(1 - Math.Pow((k / ticks), 2));

            return alglib.besseli0(betta_k) / besselAlpha;
        }

        public Spectrum setSampleFrequency(int sampleFrequency)
        {
            _sampleFrequency = sampleFrequency;

            return this;
        }

        public int getSampleFrequency()
        {
            return _sampleFrequency;
        }

        public int getPeriodogramSegmentsPerSample()
        {
            return _periodogramSegmentsPerSampleFrequency;
        }

        public Spectrum setPeriodogramSegmentsPerSample(int num)
        {
            _periodogramSegmentsPerSampleFrequency = num;

            return this;
        }

        public Spectrum setSignal(alglib.complex[] signal)
        {
            _signal = signal;

            return this;
        }

        public double[] getPeriodogram()
        {
            // get ticks count
            int ticksTotal = _signal.Length;
            int segmentLength = ticksTotal / _periodogramSegmentsPerSampleFrequency; // _sampleFrequency / _periodogramSegmentsPerSampleFrequency;

            double[] windowFunction = new double[segmentLength];

            // calc U
            double U = 0;
            for (int i = 0; i < segmentLength; i++)
            {
                windowFunction[i] = getWindowFunctionValue(i, segmentLength);
                U += Math.Pow(windowFunction[i], 2);
            }

            U = U / segmentLength;

            // prepare segment
            alglib.complex[] segment = new alglib.complex[segmentLength];

            // prepare average array
            double[] averageSegment = new double[segmentLength];
            bool averageSegmentInitialized = false;

            // calc P
            int tick = 0;
            double val;

            while (tick + segmentLength < ticksTotal)
            {
                segment = _signal.Skip(tick).Take(segmentLength).ToArray();

                for (int i = 0; i < segmentLength; i++)
                {
                    segment[i] = segment[i] * windowFunction[i];
                }

                // calculate bla-bla-gramm
                alglib.fftc1d(ref segment);

                for (int i = 0; i < segmentLength; i++)
                {
                    val = Math.Pow(alglib.math.abscomplex(segment[i]), 2) / (_sampleFrequency * segmentLength * U);

                    // average with prev
                    if (averageSegmentInitialized)
                    {
                        averageSegment[i] = (averageSegment[i] + val) / 2;
                    }
                    else
                    {
                        averageSegment[i] = val;
                    }
                }

                averageSegmentInitialized = true;

                // move forvard on half of ticks window
                tick += (segmentLength / 2);
            }

            return averageSegment;
        }
    }
}
