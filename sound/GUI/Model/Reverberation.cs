using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model
{
    class Reverberation
    {
        private SortedList<double, double>[] _reverbTime = new SortedList<double,double>[2];

        private string[][] _roomWalls = new string[][]
        {
            new string[] { "Room1Floor", "Room1Seiling", "Room1BackWall", "Room1FrontWall", "Room1LeftWall", "Room1SeparateWall" },
            new string[] { "Room2Floor", "Room2Seiling", "Room2BackWall", "Room2FrontWall", "Room2SeparateWall", "Room2RightWall" }
        };

        /**
         * Filter
         */
        private double[] _frequencyList;

        private Dictionary<string, SortedList<double, double>> _absorbCoefs = new Dictionary<string, SortedList<double, double>>();

        public Reverberation setWallAbsorbCoefitients(Dictionary<string, SortedList<double, double>> absorbCoefs)
        {
            _absorbCoefs = absorbCoefs;

            return this;
        }

        public Reverberation setFrequencyList(double[] list)
        {
            _frequencyList = list;

            return this;
        }

        public SortedList<double, double> getRoomReverbTime(int room)
        {
            if (null != _reverbTime[room])
                return _reverbTime[room];

            _reverbTime[room] = new SortedList<double, double>();

            Sound.Model.WallConnection wallConnection = Core.ServiceLocator.getWallConnection();
            double[] roomVolumes = wallConnection.getRoomVolumes();

            double[] roomFloorSquare = wallConnection.getRoomFloorSquares();

            double tc = 0.164 * roomVolumes[room];
            double nnc = 340 / Math.Sqrt(roomFloorSquare[room]);

            double absorbCoef = 0;

            for (int f = 0; f < _frequencyList.Length; f++)
            {
                double frequency = _frequencyList[f];
                double reverbTime = 0;

                for (int w = 0; w < 6; w++)
                {
                    string wallName = _roomWalls[room][w];

                    if (!_absorbCoefs.ContainsKey(wallName))
                        throw new Exception("Absorb coeficient for wall " + wallName + " not set");

                    if (_absorbCoefs[wallName].ContainsKey(frequency))
                    {
                        absorbCoef = _absorbCoefs[wallName][frequency];
                    }

                    reverbTime += wallConnection.getWallSquare(wallName) * absorbCoef;
                }

                // calc reverberation time
                _reverbTime[room][_frequencyList[f]] = tc / reverbTime;
            }

            return _reverbTime[room];
        }

        public SortedList<double, double> getRoom1ReverbTime()
        {
            return getRoomReverbTime(0);
        }

        public SortedList<double, double> getRoom2ReverbTime()
        {
            return getRoomReverbTime(1);
        }
    }
}
