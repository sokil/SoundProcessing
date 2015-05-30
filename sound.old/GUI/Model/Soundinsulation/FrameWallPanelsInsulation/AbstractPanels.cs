using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model.Soundinsulation.FrameWallPanelsInsulation
{
    abstract class AbstractPanels
    {
        protected OnelayerWallInsulation _onelayerWallInsulation = new OnelayerWallInsulation();

        protected SortedList<double,double> _frequencyCharacteristic;

        protected int _panel1LayerNum;
        protected int _panel2LayerNum;

        protected double _panel1Thickness;
        protected double _panel2Thickness;

        protected double _panel1Density;
        protected double _panel2Density;

        // used to calculate fc as (_panel1FcCoef / thickness)
        protected double _panel1FcCoef;
        protected double _panel2FcCoef;

        // used to calculate fb as (_panel1FbCoef / thickness)
        protected double _panel1FbCoef;
        protected double _panel2FbCoef;

        protected double _panel1Rb;
        protected double _panel2Rb;

        protected double _panel1Rc;
        protected double _panel2Rc;
        
        // in meters
        protected double _airSpaceThickness;

        // in meters
        protected double _airSpaceFillerThickness = 0;

        protected double _airSpaceFillerDensity = 0;

        protected Skeleton _airSpaceFillerSceleton = Skeleton.None;

        /// <summary>
        /// Ung modulus, Pa
        /// </summary>
        protected double _airSpaceHardSkeletonFillerElasiticityModulus = 0;

        protected bool _airSpaceHardSkeletonFillerGlued = false;

        public enum Skeleton
        {
            None = 0, 
            Hard = 1,
            Soft = 2
        }

        protected SortedList<double,double> _deltaR1List = new SortedList<double,double>()
        {
            {1.4, 2.0}, {1.5, 2.5}, {1.6, 3.0}, {1.7, 3.5}, {1.8, 4.0}, {2, 4.5}, {2.2, 5.0}, {2.3, 5.5}, 
            {2.5, 6.0}, {2.7, 6.5}, {2.9, 7.0}, {3.1, 7.5}, {3.4, 8.0}, {3.7, 8.5}, {4.0, 9.0}, {4.3, 9.5}, 
            {4.6, 10.0}, {5.0, 10.5}
        };

        /// <summary>
        /// relation of air thickness in MILIMETERS to dH
        /// </summary>
        protected SortedList<double, double> _deltaH = new SortedList<double, double>()
        {
            {0.01, 20}, {0.025, 22}, {0.05, 24}, {0.075, 25}, {0.1, 26}, {0.15, 27}, {0.2, 28}
        };

        protected SortedList<double, double> _deltaR4List = new SortedList<double, double>()
        {
            {0, 0}, {0.2, 2}, {0.3, 3}, {0.4, 4}, {0.5, 5}
        };

        public AbstractPanels setPanel1Thickness(double thickness)
        {
            _panel1Thickness = thickness;

            return this;
        }

        public AbstractPanels setPanel2Thickness(double thickness)
        {
            _panel2Thickness = thickness;

            return this;
        }

        public AbstractPanels setPanel1LayerNum(int layerNum)
        {
            _panel1LayerNum = layerNum;

            return this;
        }

        public AbstractPanels setPanel2LayerNum(int layerNum)
        {
            _panel2LayerNum = layerNum;

            return this;
        }

        public double getPanel1OneLayerSurfaceDensity()
        {
            return _panel1Density * _panel1Thickness;
        }

        public double getPanel1SurfaceDensity()
        {
            return getPanel1OneLayerSurfaceDensity() * _panel1LayerNum;
        }

        public double getPanel2OneLayerSurfaceDensity()
        {
            return _panel2Density * _panel2Thickness;
        }

        public double getPanel2SurfaceDensity()
        {
            return getPanel2OneLayerSurfaceDensity() * _panel2LayerNum;
        }

        public double getAirSpaceFillerDensity()
        {
            return _airSpaceFillerDensity * _airSpaceFillerThickness;
        }

        public double getTotalSurfaceDensity()
        {
            return getPanel1SurfaceDensity() + getPanel2SurfaceDensity() + getAirSpaceFillerDensity();
        }

        protected double getDeltaR1()
        {
            // thicker panel density
            double panelDensity = _panel1Thickness > _panel2Thickness
                ? getPanel1OneLayerSurfaceDensity()
                : getPanel2OneLayerSurfaceDensity();

            // get (m1 + m2 / m1)
            double m = (getPanel1SurfaceDensity() + getPanel2SurfaceDensity()) / panelDensity;

            // get deltaR1
            return AbstractWallInsulation.getClosestGreaterOrEqualValueFromTable(m, _deltaR1List).Value;
        }

        protected double getDeltaR4()
        {
            switch(_airSpaceFillerSceleton)
            {
                case Skeleton.Hard:
                    return 2;

                case Skeleton.Soft:
                    double fillerPercent = _airSpaceFillerThickness / _airSpaceThickness;
                    return AbstractWallInsulation.getClosestLessOrEqualValueFromTable(fillerPercent, _deltaR4List).Value;
                
                default:
                    throw new Exception("Delta R4 used only when filler presents");

            }


        }

        protected double getDeltaH(double airSpaceThickness)
        {
            return AbstractWallInsulation.getClosestGreaterOrEqualValueFromTable(airSpaceThickness, _deltaH).Value;
        }

        public AbstractPanels setPanel1Density(double density)
        {
            this._panel1Density = density;

            return this;
        }

        public AbstractPanels setPanel1FbCoefitient(double fb)
        {
            _panel1FbCoef = fb;

            return this;
        }

        public AbstractPanels setPanel1FcCoefitient(double fc)
        {
            _panel1FcCoef = fc;

            return this;
        }

        public AbstractPanels setPanel1Rb(double rb)
        {
            _panel1Rb = rb;

            return this;
        }

        public AbstractPanels setPanel1Rc(double rc)
        {
            _panel1Rc = rc;

            return this;
        }

        public AbstractPanels setPanel2Density(double density)
        {
            this._panel2Density = density;

            return this;
        }

        public AbstractPanels setPanel2FbCoefitient(double fb)
        {
            _panel2FbCoef = fb;

            return this;
        }

        public AbstractPanels setPanel2FcCoefitient(double fc)
        {
            _panel2FcCoef = fc;

            return this;
        }

        public AbstractPanels setPanel2Rb(double rb)
        {
            _panel2Rb = rb;

            return this;
        }

        public AbstractPanels setPanel2Rc(double rc)
        {
            _panel2Rc = rc;

            return this;
        }

        /// <summary>
        /// Air space thickness in mm
        /// </summary>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public AbstractPanels setAirSpaceThickness(double thickness)
        {
            _airSpaceThickness = thickness;

            return this;
        }

        public AbstractPanels setAirSpaceFillerSkeleton(Skeleton skeleton)
        {
            _airSpaceFillerSceleton = skeleton;

            return this;
        }

        /// <summary>
        /// in meters
        /// </summary>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public AbstractPanels setAirSpaceFillerThickness(double thickness)
        {
            _airSpaceFillerThickness = thickness;

            return this;
        }

        public AbstractPanels setAirSpaceFillerDensity(double density)
        {
            _airSpaceFillerDensity = density;

            return this;
        }

        public AbstractPanels setAirSpaceHardSkeletonFillerElasiticityModulus(double modulus)
        {
            _airSpaceHardSkeletonFillerElasiticityModulus = modulus;

            return this;
        }

        public AbstractPanels setAirSpaceHardSkeletonFillerGlued(bool gluence = true)
        {
            _airSpaceHardSkeletonFillerGlued = gluence;

            return this;
        }

        public double getResonanceFrequency()
        {
            double f = 0;

            double m1 = getPanel1SurfaceDensity();
            double m2 = getPanel2SurfaceDensity();

            if (_airSpaceFillerSceleton == Skeleton.Hard)
            {
                double modulus = _airSpaceHardSkeletonFillerElasiticityModulus;

                if (_airSpaceHardSkeletonFillerGlued == false)
                {
                    modulus *= 0.75;
                }

                f = 0.16 * Math.Sqrt(modulus * (m1 + m2) / (_airSpaceThickness * m1 * m2));
            }
            else
            {
                f = 60 * Math.Sqrt((m1 + m2) / (_airSpaceThickness * m1 * m2));
            }

            return AbstractWallInsulation.getStandartFrequency(f);

            
        }

        /// <summary>
        /// get frequency in B point of first layer
        /// </summary>
        /// <returns></returns>
        public double getFb()
        {
            return _onelayerWallInsulation.getFb();
        }

        /// <summary>
        /// get frequency in C point of first layer
        /// </summary>
        /// <returns></returns>
        public double getFc()
        {
            return _onelayerWallInsulation.getFc();
        }

        protected abstract SortedList<double, double> _getNoFillerFrequencyCharacteristic();

        public SortedList<double, double> getFrequencyCharacteristic()
        {
            SortedList<double, double>  frequencyCharacteristic = _getNoFillerFrequencyCharacteristic();

            if (_airSpaceFillerSceleton == Skeleton.None)
                return frequencyCharacteristic;

            // fQ - ...
            double fR = getResonanceFrequency();
            double fQ = AbstractWallInsulation.getStandartFrequency(1.6 * fR);
            double deltaR4 = getDeltaR4();

            List<KeyValuePair<double, double>> coordinates = AbstractWallInsulation.getSlice(fQ, null, frequencyCharacteristic);
            foreach (KeyValuePair<double, double> pair in coordinates)
            {
                frequencyCharacteristic[pair.Key] += deltaR4;
            }

            // fR - fQ
            double fR1 = AbstractWallInsulation.getClosestGreaterValueFromTable(fR, frequencyCharacteristic).Key;
            frequencyCharacteristic[fR1] = AbstractWallInsulation.getInterpolated(
                1,
                0, 2,
                frequencyCharacteristic[fR], frequencyCharacteristic[fQ]
            );

            return frequencyCharacteristic;
        }

        public abstract SortedList<double, double> getBaseFrequencyCharacteristic();

    }
}
