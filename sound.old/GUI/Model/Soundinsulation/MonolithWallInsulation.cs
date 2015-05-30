using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model.Soundinsulation
{
    public class MonolithWallInsulation : AbstractWallInsulation
    {
		private static int[] fbDensityList = new int[] { 600, 800, 1000, 1200, 1400, 1600, 1800 };
		private static int[] fbFrequencyCoefficientList = new int[] {40000, 39000, 37000, 35000, 33000, 31000, 29000 };

        // private static double[] normalFrequencyCharacteristic = new double[] { 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 52, 53, 54, 55, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56 };

        private Dictionary<string, int[]> _panels = new Dictionary<string, int[]>();

        public enum PanelBindType
        {
            rudeLinear = 1,
            rudePoint = 2,
            springLinear = 3,
            springPoint = 4
        }

        public struct PanelBind
        {
            public PanelBindType type;
            public int count;
            public double modulus; // for spring only
            public double thickness;  // for spring only
            public double square;  // for spring only
            public bool isLatitudeLathDirection; // for linear only

            public bool isSpring()
            {
                return (type == PanelBindType.springLinear) || (type == PanelBindType.springPoint);
            }

            public bool isLinear()
            {
                return (type == PanelBindType.rudeLinear) || (type == PanelBindType.springLinear);
            }

            public bool isPoint()
            {
                return (type == PanelBindType.rudePoint) || (type == PanelBindType.springPoint);
            }
        }

        public enum CoveringCalcType
        {
            None = 0,
            Simple = 1,
            Extended = 2
        }

        private CoveringCalcType _coveringCalcType = CoveringCalcType.None;

        private int _simpleCoveringDelta = 0;

        private SortedList<double, double> _extendedCoveringDelta = new SortedList<double, double>();

        private double _density;

        private double _thickness;

        private double _k;

        private SortedList<double, double> _materialFrequencyCharacteristic = null;

        const double RC = 65;

        const double NORM_FREQ_RANGE_RBOUND = 3150;

        

        private double _rb = 0;

        private double _fc = 0;

        private double _fb = 0;

        public MonolithWallInsulation(Sound.Model.Wall.Name wallName) : base(wallName)
        {
            // material, [one_panel_with_filler, two_panels_with_filler, one_panel_no_filler, two_panels_no_filler]
            _panels.Add("type1", new int[] { 4, 6, 2, 4 });
            _panels.Add("type2", new int[] { 2, 5, 0, 1 });
        }

        public MonolithWallInsulation reset()
        {
            _frequencyCharacteristic = null;

            return this;
        }

        public MonolithWallInsulation setDensity(double density)
        {
            reset();

            this._density = density;

            return this;
        }

        /**
         * @param thickness in meters
         */
        public MonolithWallInsulation setThickness(double thickness)
        {
            reset();

            this._thickness = thickness;

            return this;
        }

        public MonolithWallInsulation setCoefficientK(double k)
        {
            reset();

            this._k = k;

            return this;
        }

        /// <summary>
        /// may be specified instead of _dencity, _thickness and _k
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public MonolithWallInsulation setMaterialFrequencyCharacteristic(SortedList<double, double> ch)
        {
            reset();

            _materialFrequencyCharacteristic = ch;

            return this;
        }

		private double getEquivalentSourfaceDensity()
		{
			return _k * getSurfaceDensity();
		}
		
		public override double getSurfaceDensity()
		{
			return _density * _thickness;	
		}
		
		public override double getRb()
		{

			_rb = Math.Round((double) (20 * Math.Log10(getEquivalentSourfaceDensity()) - 12), 1);

            return _rb;
		}

        public override double getRc()
        {
            return RC;
        }
		
		public override double getFb()
		{

			double minFbDensity = 0;
			double maxFbDensity = 0;

            double minFbCoefficient = 0;
            double maxFbCoefficient = 0;

            // if density greater than max density
            double frequencyCoefficient;

            if (_density >= fbDensityList[fbDensityList.Length - 1])
            {
                frequencyCoefficient = fbFrequencyCoefficientList[fbDensityList.Length - 1];
            }
            else
            {
                // find density range
                for (int i = 0; i < fbDensityList.Length; i++)
                {
                    if (_density >= fbDensityList[i])
                        continue;

                    maxFbDensity = fbDensityList[i];
                    maxFbCoefficient = fbFrequencyCoefficientList[i];

                    if (i > 0)
                    {
                        minFbDensity = fbDensityList[i - 1];
                        minFbCoefficient = fbFrequencyCoefficientList[i - 1];
                    }

                    break;
                }

                // get interpolated frequency
                frequencyCoefficient = getInterpolated(
                    _density,
                    minFbDensity, maxFbDensity,
                    minFbCoefficient, maxFbCoefficient
                );

            }

            // return fb
            _fb = getStandartFrequency(frequencyCoefficient / _thickness / 1000);
            return _fb;
		}

        public override double getFc()
        {
            double rb = getRb();
            double fb = getFb();

            double rc = rb;

            if (rc > RC)
                throw new Exception("Вирахуваний Rb більше межі  " + RC + " дБ");

            _fc = standartFreqList[standartFreqList.Length - 1];
            for (var i = Array.IndexOf(standartFreqList, fb); i < standartFreqList.Length; i++)
            {
                if (rc < RC)
                {
                    rc += 2;
                    continue;
                }

                _fc = Math.Round(getInterpolated(RC, rc, rc - 2, standartFreqList[i], standartFreqList[i - 1]));
                break;
            }

            return _fc;
        }

        public override double getRw()
        {
            _rw = getEstimateFrequencyCharacteristic()[500];
            return _rw;
        }

        public override SortedList<double,double> getFrequencyCharacteristic()
        {
            if (_frequencyCharacteristic != null)
                return _frequencyCharacteristic;

            _frequencyCharacteristic = new SortedList<double,double>();

            double[] frequencyList = AbstractWallInsulation
                .getStandartEstimateFrequencyCharacteristic()
                .Keys.ToArray();

            if (_materialFrequencyCharacteristic == null)
            {
                double fb = getFb();
                double rb = getRb();

                double fc = getFc();

                double R = rb;

                for (var i = 0; i < frequencyList.Length; i++)
                {
                    if (frequencyList[i] > fb)
                    {
                        if (frequencyList[i] < fc || fc > NORM_FREQ_RANGE_RBOUND)
                        {
                            R += 2;
                        }
                        else
                        {
                            R = RC;
                        }
                    }

                    _frequencyCharacteristic[frequencyList[i]] = R;
                }
            }
            else
            {
                _frequencyCharacteristic = _materialFrequencyCharacteristic;
            }


            for (var i = 0; i < frequencyList.Length; i++)
            {

                switch (_coveringCalcType)
                {
                    case CoveringCalcType.Simple:
                        _frequencyCharacteristic[frequencyList[i]] += _simpleCoveringDelta;
                        break;

                    case CoveringCalcType.Extended:
                        _frequencyCharacteristic[frequencyList[i]] += _extendedCoveringDelta[frequencyList[i]];
                        break;

                }
                
            }

            return _frequencyCharacteristic;
            
        }

        public void setNoCovering()
        {
            reset();

            _coveringCalcType = CoveringCalcType.None;
        }

        public void setSimpleCovering(string meterialType, bool isTwoPannels, bool isFillerExists)
        {
            reset();

            _coveringCalcType = CoveringCalcType.Simple;

            int[] panelData = _panels[meterialType];

            int index = 0;

            if (!isFillerExists)
                index += 2;

            if (isTwoPannels)
                index += 1;

            _simpleCoveringDelta = panelData[index];

        }

        private double _getWallSizePerpendicularToLath(bool isLatitude)
        {
            double size = 0;

            switch (_wallName)
            {
                case Wall.Name.Room1BackWall:
                case Wall.Name.Room1FrontWall:
                    size = isLatitude
                        ? Core.ServiceLocator.getWallConnection().getRoom1Height()
                        : Core.ServiceLocator.getWallConnection().getRoom1Width();
                    break;

                case Wall.Name.Room1Floor:
                case Wall.Name.Room1Seiling:
                    size = isLatitude
                        ? Core.ServiceLocator.getWallConnection().getRoom1Length()
                        : Core.ServiceLocator.getWallConnection().getRoom1Width();
                    break;

                case Wall.Name.Room2BackWall:
                case Wall.Name.Room2FrontWall:
                    size = isLatitude
                        ? Core.ServiceLocator.getWallConnection().getRoom2Height()
                        : Core.ServiceLocator.getWallConnection().getRoom2Width();
                    break;

                case Wall.Name.Room2Floor:
                case Wall.Name.Room2Seiling:
                    size = isLatitude
                        ? Core.ServiceLocator.getWallConnection().getRoom2Length()
                        : Core.ServiceLocator.getWallConnection().getRoom2Width();

                    break;

                case Wall.Name.SeparateWall:
                    if (isLatitude)
                    {
                        double room1Height = Core.ServiceLocator.getWallConnection().getRoom1Height();
                        double room2Height = Core.ServiceLocator.getWallConnection().getRoom2Height();

                        size = room1Height > room2Height ? room2Height : room1Height;
                    }
                    else
                    {
                        double room1Length = Core.ServiceLocator.getWallConnection().getRoom1Length();
                        double room2Length = Core.ServiceLocator.getWallConnection().getRoom2Length();

                        size = room1Length > room2Length ? room2Length : room1Length;
                    }

                    break;

                default:
                    throw new Exception("Unknown wall");
            }

            return size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="panelDensity"></param>
        /// <param name="panelVelocity"></param>
        /// <param name="panelThickness">in meters</param>
        /// <param name="isTwoPannels"></param>
        /// <param name="airThickness">in meters</param>
        /// <param name="airFillerThickness">in meters</param>
        /// <param name="bind"></param>
        public void setExtendedCovering(double panelDensity, double panelVelocity, double panelThickness, bool isTwoPannels, double airThickness, double airFillerThickness, PanelBind bind)
        {
            reset();

            _coveringCalcType = CoveringCalcType.Extended;

            // f0
            double f0 = 0.14 * 1000000 / (airThickness * panelDensity * panelThickness);
            if(isTwoPannels)
                f0 /= 4;

            f0 = 0.16 * Math.Sqrt(f0); 

            // fBoundary
            double fBoundary = 340 * 340 / (1.8 * panelVelocity * panelThickness);
            if (isTwoPannels)
                fBoundary *= 2;

            // k
            double k = bind.modulus * bind.square / bind.thickness;
            double K;
            double Sl;

            // wall square
            string wallName = System.Enum.GetName(typeof(Wall.Name), _wallName);
            double square = Core.ServiceLocator.getWallConnection().getWallSquare(wallName);

            //
            double[] frequencyList = AbstractWallInsulation
                .getStandartEstimateFrequencyCharacteristic()
                .Keys.ToArray();

            if (bind.isPoint())
            {
                Sl = Math.Sqrt(0.2584 * 340 * 340 * bind.count / (fBoundary * fBoundary * square));
            }
            else
            {
                double size = _getWallSizePerpendicularToLath(bind.isLatitudeLathDirection);

                Sl = Math.Sqrt(2 * 340 * 340 * size * bind.count / (square * Math.PI * fBoundary * fBoundary));
            }

            for (int i = 0; i < frequencyList.Length; i++)
            {
                double f = frequencyList[i];

                if (f < f0)
                {
                    _extendedCoveringDelta[f] = 0;
                    continue;
                }

                if (bind.isSpring())
                {
                    K = (k * k) / Math.Pow(k - (panelDensity * panelThickness * square * 6.28 * f * f / bind.count), 2);
                }
                else
                {
                    K = 1;
                }

                if (f < 3 * f0)
                {
                    _extendedCoveringDelta[f] = -10 * Math.Log10(Math.Pow(f0 / f, 2) + K * Sl);
                }
                else
                {
                    _extendedCoveringDelta[f] = -10 * Math.Log10(Sl);
                }
            }
        }
    }
}
