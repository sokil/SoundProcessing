using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model.Soundinsulation
{
    abstract public class AbstractWallInsulation
    {
        protected static double[] standartFreqList;
        protected static double[] standartFreqRangeList = new double[] { 112, 140, 180, 224, 280, 355, 450, 560, 710, 900, 1120, 1400, 1800, 2240, 2800, 3550, 4500, 5600, 7100, 9000, 11200 };

        protected SortedList<double,double> _frequencyCharacteristic;

        protected Sound.Model.Wall.Name _wallName;

        public AbstractWallInsulation()
        {
            standartFreqList = Core.ServiceLocator.getFrequencyRangeByFilterChainNum(Core.ServiceLocator.getSoundTransmittion().getElementsInFilterChain());
        }

        public AbstractWallInsulation(Sound.Model.Wall.Name wallName)
        {
            _wallName = wallName;

            standartFreqList = Core.ServiceLocator.getFrequencyRangeByFilterChainNum(Core.ServiceLocator.getSoundTransmittion().getElementsInFilterChain());
        }

        public double[] getStandartFrequencyList()
        {
            return standartFreqList;
        }

        abstract public double getSurfaceDensity();

        abstract public SortedList<double,double> getFrequencyCharacteristic();

        abstract public double getFb();

        abstract public double getFc();

        abstract public double getRb();

        abstract public double getRc();

        public static double getStandartFrequency(double frequency)
        {
            for (int i = 0; i < standartFreqRangeList.Length; i++)
            {
                if (frequency < standartFreqRangeList[i])
                    return standartFreqList[i];
            }

            return standartFreqList[standartFreqList.Length - 1];
        }

        public static List<KeyValuePair<double, double>> getClosestPoints(double x, SortedList<double, double> list)
        {
            List<KeyValuePair<double, double>> points = new List<KeyValuePair<double, double>>();

            points.Add(getClosestLessValueFromTable(x, list));
            points.Add(getClosestGreaterValueFromTable(x, list));

            return points;
        }

       

        public static double getInterpolated(double x, double x1, double x2, double y1, double y2)
        {
            if (x1 == x)
                return y1;

            if (x2 == x)
                return y2;

            return ((y2 - y1) / (x2 - x1)) * (x - x1) + y1;
        }

        public static KeyValuePair<double, double> getClosestGreaterValueFromTable(double x, IDictionary<double, double> table)
        {
            foreach (KeyValuePair<double, double> pair in table)
            {
                if (x < pair.Key)
                {
                    return pair;
                }
            }

            return table.Last();
        }

        public static KeyValuePair<double, double> getClosestGreaterOrEqualValueFromTable(double x, IDictionary<double, double> table)
        {
            foreach (KeyValuePair<double, double> pair in table)
            {
                if (x <= pair.Key)
                {
                    return pair;
                }
            }

            return table.Last();
        }

        public static KeyValuePair<double, double> getClosestLessValueFromTable(double x, IDictionary<double, double> table)
        {
            KeyValuePair<double, double> less = table.First();

            foreach (KeyValuePair<double, double> pair in table)
            {
                if (x > pair.Key)
                {
                    less = pair; 
                }
                else
                {
                    return less;
                }
            }

            return less;
        }

        public static KeyValuePair<double, double> getClosestLessOrEqualValueFromTable(double x, IDictionary<double, double> table)
        {
            KeyValuePair<double, double> less = table.First();

            foreach (KeyValuePair<double, double> pair in table)
            {
                if (x >= pair.Key)
                {
                    less = pair;
                }
                else
                {
                    return less;
                }
            }

            return less;
        }

        public static List<KeyValuePair<double, double>> getSlice(double minX, double? maxX, IDictionary<double, double> table)
        {
            List<KeyValuePair<double, double>> points = new List<KeyValuePair<double, double>>();

            foreach (KeyValuePair<double, double> pair in table)
            {
                if (pair.Key < minX)
                    continue;

                if (maxX != null && pair.Key > maxX)
                    break;

                points.Add(pair);
            }

            return points;
        }

        public EstimateCharacteristic getEstimateFrequencyCharacteristic()
        {
            return new EstimateCharacteristic(getFrequencyCharacteristic());
        }

        public double getRw()
        {
            return getEstimateFrequencyCharacteristic().getRw();
        }

        public virtual string getSummary()
        {
            return "Fb: " + getFb() + " Гц, Rb: " + getRb() + "дБ, Fc: " + getFc() + " Гц, Rc: " + getRc() + ", Rw: " + getRw();
        }

    }


}
