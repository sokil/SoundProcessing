using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model.Soundinsulation
{
    class OnelayerWallInsulation : AbstractWallInsulation
    {
        // slope per 1/3 octave
        const double INCLINATION_AB = 1.5; // 4.5 per octave;

        // slope per 1/3 octave
        const double INCLINATION_CD = 2.5; // 7.5 per octave;

        private double _thickness;

        private double _density;

        // used to calculate fc as (_fcCoef / thickness)
        private double _fcCoef;

        // used to calculate fb as (_fbCoef / thickness)
        private double _fbCoef;

        private double _rb;

        private double _rc;

        /// <summary>
        /// thickness of layer setter
        /// </summary>
        /// <param name="thickness">thickness in meters</param>
        /// <returns>OnelayerWallInsulation</returns>
        public OnelayerWallInsulation setThickness(double thickness)
        {
            this._thickness = thickness;

            return this;
        }

        public OnelayerWallInsulation setDensity(double density)
        {
            this._density = density;

            return this;
        }

        public override double getSurfaceDensity()
        {
            return _density * _thickness;
        }

        public override SortedList<double,double> getFrequencyCharacteristic()
        {

            _frequencyCharacteristic = new SortedList<double,double>();

            double fb = getFb();
            double fc = getFc();

            double rb = getRb();
            double rc = getRc();

            // A - B
            
            int iFbPos = Array.IndexOf( standartFreqList, fb);
            _frequencyCharacteristic[fb] = rb;
            for (int i = iFbPos - 1; i >= 0; i--)
            {
                _frequencyCharacteristic[standartFreqList[i]] = _frequencyCharacteristic[standartFreqList[i + 1]] - INCLINATION_AB;
            }

            // C - D
            int iFcPos = Array.IndexOf( standartFreqList, fc);
            _frequencyCharacteristic[fc] = rc;

            for (int i = iFcPos + 1; i < standartFreqList.Length; i++)
            {
                _frequencyCharacteristic[standartFreqList[i]] = _frequencyCharacteristic[standartFreqList[i - 1]] + INCLINATION_CD;
            }

            // B - C
            for (int i = iFbPos + 1; i < iFcPos; i++)
            {
                _frequencyCharacteristic[standartFreqList[i]] = getInterpolated(standartFreqList[i], fb, fc, rb, rc);
            }

            return _frequencyCharacteristic;
        }

        public override double getFb()
        {
            return getStandartFrequency( _fbCoef / (_thickness * 1000));
        }

        public override double getFc()
        {
            return getStandartFrequency( _fcCoef / (_thickness * 1000 ));
        }

        public OnelayerWallInsulation setFbCoefitient(double fb)
        {
            _fbCoef = fb;

            return this;
        }

        public OnelayerWallInsulation setFcCoefitient(double fc)
        {
            _fcCoef = fc;

            return this;
        }

        public override double getRb()
        {
            return _rb;
        }

        public override double getRc()
        {
            return _rc;
        }

        public OnelayerWallInsulation setRb(double rb)
        {
            _rb = rb;

            return this;
        }

        public OnelayerWallInsulation setRc(double rc)
        {
            _rc = rc;

            return this;
        }
    }
}
