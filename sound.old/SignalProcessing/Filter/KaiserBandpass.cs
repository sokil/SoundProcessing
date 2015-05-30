using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing.Filter
{
    public class KaiserBandpass : AbstractFilter<KaiserBandpass>
    {
        private double _leftBoundFrequency;

        private double _rightBoundFrequency;

        public KaiserBandpass setLeftBoundFrequency(double frequency)
        {
            _leftBoundFrequency = frequency;

            return this;
        }

        public KaiserBandpass setRightBoundFrequency(double frequency)
        {
            _rightBoundFrequency = frequency;

            return this;
        }

        public override alglib.complex getA0()
        {
            return 2 * (_rightBoundFrequency - _leftBoundFrequency) / getSampleFrequency();
        }

        public override alglib.complex getAk(double k)
        {
            double g1 = Math.Sin(2 * Math.PI * k * _leftBoundFrequency / getSampleFrequency());

            double g2 = Math.Sin(2 * Math.PI * k * _rightBoundFrequency / getSampleFrequency());

            double ak = ( 1 / (Math.PI * k ) ) * (g2 - g1);

            return ak * getWindowFunctionValue(k);
        }

        public override alglib.complex getFreqCharValue(double f)
        {
            alglib.complex sum = 0;

            int filterOrder = getFilterOrder();
            double sampleStep = getSampleStep();
            double w = 2 * Math.PI * f * sampleStep;

            for (int k = 1; k < filterOrder; k++)
            {
                sum += getAk(k) * Math.Cos(w * k);
            }

            return getA0() + 2 * sum;
        }
    }
}
