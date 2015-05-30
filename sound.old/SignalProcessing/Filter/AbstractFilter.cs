using System;
using System.Collections.Generic;
using System.Linq;

using SignalProcessing.Signal;

namespace SignalProcessing.Filter
{
    public abstract class AbstractFilter<T> where T: AbstractFilter<T>
    {
        const double alpha = 9;

        private int _sampleFrequency;

        private double _sampleStep;

        private int _N;

        abstract public alglib.complex getA0();

        abstract public alglib.complex getAk(double k);

        abstract public alglib.complex getFreqCharValue(double f);

        public T setSampleFrequency(int frequency)
        {
            _sampleFrequency = frequency;

            _sampleStep = 1 / _sampleFrequency;

            return (T) this;
        }

        public int getSampleFrequency()
        {
            return _sampleFrequency;
        }

        public double getSampleStep()
        {
            return _sampleStep;
        }

        public T setFilterOrder(int N)
        {
            if (N % 2 == 1)
                N++;

            _N = N;

            return (T) this;
        }

        public int getFilterOrder()
        {
            return _N;
        }

        protected double getWindowFunctionValue(double k)
        {
            double besselAlpha = alglib.besseli0(alpha);

            double betta_k = alpha * Math.Sqrt(1 - Math.Pow((k / _N), 2));

            return alglib.besseli0(betta_k) / besselAlpha;
        }

        public Wav filter<S>(S signal) where S : AbstractSignal<S>
        {
            alglib.complex[] filteredSignal;

            alglib.complex[] ak = new alglib.complex[_N];
            ak[0] = getA0();

            for (int k = 1; k < _N; k++)
            {
                ak[k] = getAk(k);
            }

            alglib.complex[] complexSignalData = signal.getComplexSignal();

            alglib.convc1d(complexSignalData, complexSignalData.Length, ak, _N, out filteredSignal);

            // trim to original signal size
            filteredSignal = filteredSignal.Take(signal.getCountsNumber()).ToArray<alglib.complex>();

            return (new Wav())
                .setSampleFrequency(getSampleFrequency())
                .setComplexSignal(filteredSignal);
        }

        // hyperbolic arc sine
        protected double _arsh(double x)
        {
            return Math.Log((x + Math.Sqrt(x * x + 1)), Math.E);
        }

        // hyperbolic arc cosine
        protected double _arch(double x)
        {
            return Math.Log((x + Math.Sqrt(x * x - 1)), Math.E);
        }

        // hyperbolic sine
        protected double _sh(double x)
        {
            return (Math.Pow(Math.E, x) - Math.Pow(Math.E, -x)) / 2;
        }

        // hyperbolic cosine
        protected double _ch(double x)
        {
            return (Math.Pow(Math.E, x) + Math.Pow(Math.E, -x)) / 2;
        }
    }
}
