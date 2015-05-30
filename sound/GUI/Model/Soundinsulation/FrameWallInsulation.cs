using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sound.Model.Soundinsulation.FrameWallPanelsInsulation;

namespace Sound.Model.Soundinsulation
{
    class FrameWallInsulation : AbstractWallInsulation
    {

        private double _panel1Thickness;

        private int _panel1LayerNum;

        private double _panel2Thickness;

        private int _panel2LayerNum;

        private AbstractPanels _panels;

        public FrameWallInsulation setPanel1Thickness(double thickness)
        {
            _panels = null;

            this._panel1Thickness = thickness;

            return this;
        }

        public FrameWallInsulation setPanel1LayerNum(int num)
        {
            _panels = null;

            this._panel1LayerNum = num;

            return this;
        }

        public FrameWallInsulation setPanel2Thickness(double thickness)
        {
            _panels = null;

            this._panel2Thickness = thickness;

            return this;
        }

        public FrameWallInsulation setPanel2LayerNum(int num)
        {
            _panels = null;

            this._panel2LayerNum = num;

            return this;
        }

        public override double getSurfaceDensity()
        {
            return getPanels().getTotalSurfaceDensity();
        }

        public override SortedList<double,double> getFrequencyCharacteristic()
        {
            return getPanels().getFrequencyCharacteristic();
        }

        public override double getFb()
        {
            return 0;
        }

        public override double getFc()
        {
            return 0;
        }

        public override double getRb()
        {
            return 0;
        }

        public override double getRc()
        {
            return 0;
        }

        public AbstractPanels getPanels()
        {
            if (null != _panels)
                return _panels;

            // choose strategy of calculation
            if (_panel1Thickness == _panel2Thickness)
            {
                _panels = new SameThickness()
                    .setThickness(_panel1Thickness)
                    .setPanel1LayerNum(_panel1LayerNum)
                    .setPanel2LayerNum(_panel2LayerNum);
            }
            else
            {
                _panels = new DiffThickness()
                    .setPanel1Thickness(_panel1Thickness)
                    .setPanel2Thickness(_panel2Thickness)
                    .setPanel1LayerNum(_panel1LayerNum)
                    .setPanel2LayerNum(_panel2LayerNum);
            }

            return _panels;
        }

        public override string getSummary()
        {
            return base.getSummary() + ", Fr: " + getPanels().getResonanceFrequency();
        }
    }
}
