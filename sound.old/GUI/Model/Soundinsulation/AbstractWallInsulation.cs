using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model.Soundinsulation
{
    abstract public class AbstractWallInsulation
    {
        protected static double[] standartFreqList = new double[] { 31.5, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 10000 };
        protected static double[] standartFreqRangeList = new double[] { 35.5, 45, 56, 71, 90, 112, 140, 180, 224, 280, 355, 450, 560, 710, 900, 1120, 1400, 1800, 2240, 2800, 3550, 4500, 5600, 7100, 9000, 11200 };

        private static SortedList<double,double> _standartEstimateFrequencyCharacteristic = new SortedList<double,double>() 
        { 
            {100, 33}, {125, 36}, {160, 39}, {200, 42}, {250, 45}, {315, 48}, {400, 51}, {500, 52},
            {630, 53}, {800, 54}, {1000, 55}, {1250, 56}, {1600, 56}, {2000, 56}, {2500, 56}, {3150, 56}
        };

        protected SortedList<double,double> _frequencyCharacteristic;

        protected double _rw = 0;

        protected Sound.Model.Wall.Name _wallName;

        public AbstractWallInsulation()
        {
            
        }

        public AbstractWallInsulation(Sound.Model.Wall.Name wallName)
        {
            _wallName = wallName;
        }

        public double[] getStandartFrequencyList()
        {
            return standartFreqList;
        }

        public static SortedList<double,double> getStandartEstimateFrequencyCharacteristic()
        {
            return _standartEstimateFrequencyCharacteristic;
        }

        public int getEstimateFrequencyCharacteristicShift()
        {
            int? foundShift = null;

            SortedList<double,double> charact = getFrequencyCharacteristic();
            
            int shift = 0;
            
            double sum;

            while (true)
            {
                sum = 0;

                foreach(KeyValuePair<double, double> pair in _standartEstimateFrequencyCharacteristic)
                {
                    double deviation = pair.Value - charact[pair.Key] + shift;

                    if (deviation <= 0)
                        continue;

                    sum += deviation;
                }

                if (sum > 32)
                {
                    if(foundShift != null)
                        return (int) foundShift;

                    shift--;
                }
                else
                {
                    shift++;
                    foundShift = shift;
                }
            }
        }

        /// <summary>
        /// Used to calculate Rw
        /// </summary>
        /// <returns></returns>
        public SortedList<double,double> getEstimateFrequencyCharacteristic()
        {
            SortedList<double,double> estimateCharact = _standartEstimateFrequencyCharacteristic;

            int shift = getEstimateFrequencyCharacteristicShift();

            foreach (double frequency in estimateCharact.Keys.ToList<double>())
            {
                estimateCharact[frequency] = estimateCharact[frequency] + shift;
            }

            return estimateCharact;
        }

        abstract public double getSurfaceDensity();

        abstract public SortedList<double,double> getFrequencyCharacteristic();

        abstract public double getFb();

        abstract public double getFc();

        abstract public double getRb();

        abstract public double getRc();

        abstract public double getRw();

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

        public virtual string getSummary()
        {
            return "Fb: " + getFb() + " Гц, Rb: " + getRb() + "дБ, Fc: " + getFc() + " Гц, Rc: " + getRc() + ", Rw: " + getRw();
        }

    }


}
