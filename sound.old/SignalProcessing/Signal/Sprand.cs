using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing.Signal
{
    public class Sprand : AbstractSignal<Sprand>
    {
        private int _length;

        private int _notNullCount;

        Random _normalDistributionRand = new Random();

        public Sprand(int length, int notNullCount)
        {
            _length = length;

            _notNullCount = notNullCount;
        }

        private double _getNormalValue()
        {
            // alglib.invnormaldistribution(_normalDistributionRand.NextDouble());

            double val = 0;

            for (int i = 0; i < 12; i++)
            {
                val += _normalDistributionRand.NextDouble();
            }

            return val / 12;
        }

        public override double[] getSignal()
        {
            // generate empty signal
            _signal = new double[_length];
            _complexSignal = new alglib.complex[_length];

            // add not-null points
            Random posRand = new Random();

            int inserted = 0;
            while (_notNullCount > inserted)
            {
                int pos = posRand.Next(0, _length);

                if (_signal[pos] != 0)
                    continue;

                _signal[pos] = _getNormalValue();
                _complexSignal[pos] = (alglib.complex)_signal[pos];

                inserted++;
            }

            _signal[0] = 1;
            _complexSignal[0] = (alglib.complex) 1;

            return _signal;
        }
    }
}
