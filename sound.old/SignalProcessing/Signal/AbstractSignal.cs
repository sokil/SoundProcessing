using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing.Signal
{
    public abstract class AbstractSignal<T> where T: AbstractSignal<T>
    {
        protected double[] _signal;

        protected alglib.complex[] _complexSignal;

        protected int _sampleFrequency;

        protected double _signalLevel;

        private double _median = 0;

        private double _variance = 0;

        public int getSampleFrequency()
        {
            return _sampleFrequency;
        }

        public T setSampleFrequency(int sampleFrequency)
        {
            _sampleFrequency = sampleFrequency;

            return (T)this;
        }

        public T setSignalLevel(double level)
        {
            _signalLevel = level;

            // if signal already specified - change it level
            if(_signal != null)
                _applySignalLevel();

            return (T) this;
        }

        public double getMedian()
        {
            if (_median != 0)
                return _median;

            for (int i = 0; i < _signal.Length; i++)
            {
                _median += _signal[i];
            }

            _median /= _signal.Length;

            return _median;
        }


        public double getVariance()
        {
            // double mean, variance, skewness, kurtosis;
            //alglib.samplemoments(getSignal(), out mean, out variance, out skewness, out kurtosis);

            if (_variance != 0)
                return _variance;

            double median = getMedian();

            for (int i = 0; i < _signal.Length; i++)
            {
                _variance += Math.Pow(_signal[i] - median, 2);
            }

            _variance /= _signal.Length - 1;

            return _variance;
        }

        public virtual double[] getSignal()
        {
            return _signal;
        }

        public int getCountsNumber()
        {
            return _signal.Length;
        }

        public T setSignal(double[] signal)
        {
            _reset();

            _signal = signal;

            _complexSignal = new alglib.complex[signal.Length];

            for (int i = 0; i < signal.Length; i++)
            {
                _complexSignal[i] = (alglib.complex)signal[i];
            }

            return (T)this;
        }

        public alglib.complex[] getComplexSignal()
        {
            if (_complexSignal != null)
                return _complexSignal;

            // generate signal
            getSignal();

            return _complexSignal;
        }

        public T setComplexSignal(alglib.complex[] signal)
        {
            _reset();

            _complexSignal = signal;

            _signal = new double[signal.Length];

            for (int i = 0; i < signal.Length; i++)
            {
                _signal[i] = signal[i].x;
            }

            return (T)this;
        }

        protected T _applySignalLevel()
        {
            double K;

            // change level
            if (_signalLevel <= 0)
                return (T)this;

            // calc dispersion

            K = Math.Sqrt(Math.Pow(10, -6) * Math.Pow(10, _signalLevel / 10)) / Math.Sqrt( getVariance() );

            for (int tickNum = 0; tickNum < _signal.Length; tickNum++)
            {
                _signal[tickNum] *= K;
                _complexSignal[tickNum] = _complexSignal[tickNum] * K;
            }

            return (T) this;
        }



        public virtual T save(string filename)
        {
            Wav file = new Wav();

            file
                .setSampleFrequency(_sampleFrequency)
                .setSignal(getSignal())
                .setComplexSignal(getComplexSignal())
                .save(filename);

            return (T)this;
        }

        public T trim(int ticks)
        {
            setSignal(getSignal().Take<double>(ticks).ToArray<double>());
            setComplexSignal(getComplexSignal().Take(ticks).ToArray<alglib.complex>());
            return (T)this;
        }

        public T expand(int ticks)
        {
            if (ticks < _signal.Length)
                return (T)this;

            double[] newSignal = new double[ticks];
            alglib.complex[] newComplexSignal = new alglib.complex[ticks];

            int destIndex = 0;
            int sourceLength = _signal.Length;

            do
            {
                if (ticks - destIndex < _signal.Length)
                    sourceLength = ticks - destIndex;

                Array.Copy(_signal, 0, newSignal, destIndex, sourceLength);
                Array.Copy(_complexSignal, 0, newComplexSignal, destIndex, sourceLength);

                destIndex += sourceLength;

            }
            while (destIndex < ticks);
            

            setSignal(newSignal);
            setComplexSignal(newComplexSignal);

            return (T)this;
        }

        public T trimSeconds(int seconds)
        {
            return trim(seconds * getSampleFrequency());
        }

        public T addReverberation(alglib.complex[] transmitFunction)
        {
            alglib.complex[] reverberatedComplexSignal;

            alglib.convc1d(getComplexSignal(), getComplexSignal().Length, transmitFunction, transmitFunction.Length, out reverberatedComplexSignal);

            // trim to original signal size
            reverberatedComplexSignal = reverberatedComplexSignal.Take(getCountsNumber()).ToArray<alglib.complex>();

            setComplexSignal(reverberatedComplexSignal);

            return (T) this;
        }

        public static AbstractSignal<T> operator *(int level, AbstractSignal<T> signal)
        {
            return signal.multiply(level);
        }

        public static AbstractSignal<T> operator *(AbstractSignal<T> signal, int level)
        {
            return signal.multiply(level);
        }

        public static AbstractSignal<T> operator *(double level, AbstractSignal<T> signal)
        {
            return signal.multiply(level);
        }

        public static AbstractSignal<T> operator *(AbstractSignal<T> signal, double level)
        {
            return signal.multiply(level);
        }

        public AbstractSignal<T> multiply(double level)
        {
            _reset();

            for (int i = 0; i < _signal.Length; i++)
            {
                _signal[i] *= level;
                _complexSignal[i] *= level;
            }

            return this;
        }

        public AbstractSignal<T> multiply(int level)
        {
            return multiply(Convert.ToDouble(level));
        }

        private void _reset()
        {

            _median = 0;
            _variance = 0;
        }
    }

    
}
