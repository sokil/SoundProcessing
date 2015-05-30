using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing.Filter
{
    public class KaiserLowpass : AbstractFilter<KaiserLowpass>
    {
        private double _frequency;

        public KaiserLowpass setFrequency(double frequency)
        {
            _frequency = frequency;

            return this;
        }

        public override alglib.complex getA0()
        {
            return 2 * _frequency / getSampleFrequency();
        }

        public override alglib.complex getAk(double k)
        {
            double g = Math.Sin(2 * Math.PI * k * (_frequency / getSampleFrequency()));

            double ak = g / (Math.PI * k);

            return ak * getWindowFunctionValue(k);
        }

        public override alglib.complex getFreqCharValue(double f)
        {
            alglib.complex sum = 0;

            int filterOrder = getFilterOrder();
            double sampleStep = getSampleStep();
            double w = 2 * Math.PI * f * sampleStep;

            for (int k = 1; k <= filterOrder; k++)
            {
                sum += getAk(k) * Math.Cos( k * w );
            }

            return getA0() + 2 * sum;
        }
    }
}
