using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model.Soundinsulation.FrameWallPanelsInsulation
{
    class SameThickness : AbstractPanels
    {
        public SameThickness setThickness(double thickness)
        {
            setPanel1Thickness(thickness);
            setPanel2Thickness(thickness);

            return this;
        }

        public override SortedList<double, double> getBaseFrequencyCharacteristic()
        {
            // get frequency 
            SortedList<double, double> baseFrequencyCharacteristic = _onelayerWallInsulation
                .setDensity(_panel1Density)
                .setThickness(_panel1Thickness)
                .setFbCoefitient(_panel1FbCoef)
                .setFcCoefitient(_panel1FcCoef)
                .setRb(_panel1Rb)
                .setRc(_panel1Rc)
                .getFrequencyCharacteristic();

            // add delta-R1
            double deltaR1 = getDeltaR1();

            foreach (double frequency in baseFrequencyCharacteristic.Keys.ToList<double>())
            {
                baseFrequencyCharacteristic[frequency] += deltaR1;
            }

            return baseFrequencyCharacteristic;
        }

        protected override SortedList<double, double> _getNoFillerFrequencyCharacteristic()
        {
            if (_frequencyCharacteristic != null)
                return _frequencyCharacteristic;

            SortedList<double, double> baseFrequencyCharacteristic = getBaseFrequencyCharacteristic();
            SortedList<double, double> frequencyCharacteristic = new SortedList<double, double>(baseFrequencyCharacteristic);


            // get resonance frequency
            double resonanceFrequency = getResonanceFrequency();

            // add 0.8 * resonanceFrequency
            double fE = AbstractWallInsulation.getStandartFrequency(0.8 * resonanceFrequency);

            List<KeyValuePair<double, double>> coordinates = AbstractWallInsulation.getClosestPoints(fE, frequencyCharacteristic);
            frequencyCharacteristic[fE] = AbstractWallInsulation.getInterpolated(fE, coordinates[0].Key, coordinates[1].Key, coordinates[0].Value, coordinates[1].Value);

            // add resonanceFrequency and move it 4 dB down
            frequencyCharacteristic[resonanceFrequency] = frequencyCharacteristic[resonanceFrequency] - 4;

            // get fK = 8 * resonanceFrequency
            double fK = AbstractWallInsulation.getStandartFrequency(8 * resonanceFrequency);
            double K = frequencyCharacteristic[resonanceFrequency] + getDeltaH(_airSpaceThickness);

            // K - L
            double fB = getFb(); // L point

            int i;

            if (fB <= fK) // L <= K
            {
                frequencyCharacteristic[fB] = AbstractWallInsulation.getInterpolated(
                    frequencyCharacteristic.IndexOfKey(fB),
                    frequencyCharacteristic.IndexOfKey(resonanceFrequency), frequencyCharacteristic.IndexOfKey(fK),
                    frequencyCharacteristic[resonanceFrequency], K
                );

                // F-K: interpolate all frequencies between resonanceFrequency and 8 * resonanceFrequency
                coordinates = AbstractWallInsulation.getSlice(resonanceFrequency, fB, frequencyCharacteristic);
                foreach (KeyValuePair<double, double> pair in coordinates)
                {
                    frequencyCharacteristic[pair.Key] = AbstractWallInsulation.getInterpolated(
                        frequencyCharacteristic.IndexOfKey(pair.Key),
                        frequencyCharacteristic.IndexOfKey(resonanceFrequency), frequencyCharacteristic.IndexOfKey(fB),
                        frequencyCharacteristic[resonanceFrequency], frequencyCharacteristic[fB]
                    );
                }
            }
            else // L > K
            {
                frequencyCharacteristic[fK] = K;

                // F - K: interpolate all frequencied between resonanceFrequency and fK
                KeyValuePair<double, double> fNextAfterF = AbstractWallInsulation.getClosestGreaterValueFromTable(resonanceFrequency, frequencyCharacteristic);
                KeyValuePair<double, double> fPrevBeforeK = AbstractWallInsulation.getClosestLessValueFromTable(fK, frequencyCharacteristic);
                coordinates = AbstractWallInsulation.getSlice(fNextAfterF.Key, fPrevBeforeK.Key, frequencyCharacteristic);

                i = 0;
                foreach (KeyValuePair<double, double> pair in coordinates)
                {
                    frequencyCharacteristic[pair.Key] = AbstractWallInsulation.getInterpolated(
                        ++i,
                        0, coordinates.Count + 1,
                        frequencyCharacteristic[resonanceFrequency], frequencyCharacteristic[fK]
                    );
                }

                // K - L: (from 8*resonancefrequency to fb) paralel to A1B1
                KeyValuePair<double, double> fNextAfterK = AbstractWallInsulation.getClosestGreaterValueFromTable(fK, frequencyCharacteristic);
                coordinates = AbstractWallInsulation.getSlice(fNextAfterK.Key, fB, frequencyCharacteristic);

                i = 1;
                foreach (KeyValuePair<double, double> pair in coordinates)
                {
                    // add 4.5 per octave
                    frequencyCharacteristic[pair.Key] = K + i * 1.5;
                    i++;
                }
            }

            // get 1.25 fa - draw LM - horisontal
            double fM = AbstractWallInsulation.getStandartFrequency(1.25 * fB);
            frequencyCharacteristic[fM] = frequencyCharacteristic[fB];

            // draw MN
            double deltaR2 = frequencyCharacteristic[fB] - baseFrequencyCharacteristic[fB];
            double fC = getFc();

            double deltaR3 = 0;
            if (_panel1LayerNum == 2 && _panel2LayerNum == 2)
            {
                deltaR3 = 3;
            }
            else if (_panel1LayerNum + _panel2LayerNum == 3)
            {
                deltaR3 = 2;
            }

            frequencyCharacteristic[fC] = baseFrequencyCharacteristic[fC] + deltaR2 + deltaR3;

            KeyValuePair<double, double> fNextAfterM = AbstractWallInsulation.getClosestGreaterValueFromTable(fM, frequencyCharacteristic);
            KeyValuePair<double, double> fPrevBeforeC = AbstractWallInsulation.getClosestLessValueFromTable(fC, frequencyCharacteristic);
            coordinates = AbstractWallInsulation.getSlice(fNextAfterM.Key, fPrevBeforeC.Key, frequencyCharacteristic);
            i = 0;
            foreach (KeyValuePair<double, double> pair in coordinates)
            {
                frequencyCharacteristic[pair.Key] = AbstractWallInsulation.getInterpolated(
                    ++i,
                    0, coordinates.Count + 1,
                    frequencyCharacteristic[fM], frequencyCharacteristic[fC]
                );
            }

            // draw N-P
            KeyValuePair<double, double> fNextAfterC = AbstractWallInsulation.getClosestGreaterValueFromTable(fC, frequencyCharacteristic);
            coordinates = AbstractWallInsulation.getSlice(fNextAfterC.Key, null, frequencyCharacteristic);
            foreach (KeyValuePair<double, double> pair in coordinates)
            {
                // paralel to cd
                // or slope on 7.5 per octave
                frequencyCharacteristic[pair.Key] = frequencyCharacteristic[pair.Key] + deltaR2;
            }

            _frequencyCharacteristic = frequencyCharacteristic;

            return frequencyCharacteristic;
        }
    }
}
