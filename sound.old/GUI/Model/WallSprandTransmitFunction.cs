using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SignalProcessing.Signal;

namespace Sound.Model
{
    class WallSprandTransmitFunction
    {
        private string[][] _roomWalls = new string[][]
        {
            new string[] { "Room1Floor", "Room1Seiling", "Room1BackWall", "Room1FrontWall", "Room1LeftWall", "Room1SeparateWall" },
            new string[] { "Room2Floor", "Room2Seiling", "Room2BackWall", "Room2FrontWall", "Room2SeparateWall", "Room2RightWall" }
        };

        double[] _frequencyRange = new double[] { 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150 };

        private int _sampleFrequency;

        // private SortedList<double, double>[] _T = new SortedList<double, double>[2];

        private Dictionary<string, SortedList<double, double>> _absorbCoefs = new Dictionary<string, SortedList<double, double>>();

        // [room 0..1][frequencyPointer 0..15][tick 0..N]
        private alglib.complex[][][] _sprandTransmitFunction;

        public WallSprandTransmitFunction setWallAbsorbCoefitients(string wallName, SortedList<double, double> absorbCoefs)
        {
            _absorbCoefs[wallName] = absorbCoefs;

            return this;
        }

        public WallSprandTransmitFunction setSampleFrequency(int sampleFrequency)
        {
            _sampleFrequency = sampleFrequency;

            return this;
        }

        private WallSprandTransmitFunction _calculateTransmitFunction()
        {
            alglib.complex[][][] sprandTransmitFunction = new alglib.complex[2][][];
            SortedList<double, Sprand>[] sprand = new SortedList<double, Sprand>[2];

            Sound.Model.WallConnection wallConnection = Core.ServiceLocator.getWallConnection();
            double[] roomVolumes = wallConnection.getRoomVolumes();

            for (int room = 0; room < 2; room++)
            {
                double[] roomFloorSquare = wallConnection.getRoomFloorSquares();

                //_T[room] = new SortedList<double, double>();
                sprand[room] = new SortedList<double, Sprand>();
                sprandTransmitFunction[room] = new alglib.complex[_frequencyRange.Length][];

                double tc = 0.164 * roomVolumes[room];
                double nnc = 340 / Math.Sqrt(roomFloorSquare[room]);

                for (int f = 0; f < _frequencyRange.Length; f++)
                {
                    double frequency = _frequencyRange[f];
                    double reverbTime = 0;

                    for (int w = 0; w < 6; w++)
                    {
                        string wallName = _roomWalls[room][w];

                        if (!_absorbCoefs.ContainsKey(wallName))
                            throw new Exception("Absorb coeficient for wall " + wallName + " not set");

                        reverbTime += wallConnection.getWallSquare(wallName) * _absorbCoefs[wallName][frequency];
                    }

                    // calc reverberation time
                    reverbTime = tc / reverbTime;

                    // generate sprand
                    int length = Convert.ToInt32(_sampleFrequency * reverbTime);
                    int notNullCount = Convert.ToInt32(reverbTime * nnc);

                    sprand[room][frequency] = (new Sprand(length, notNullCount))
                        .setSampleFrequency(_sampleFrequency);

                    double[] sprandSignal = sprand[room][frequency].getSignal();

                    sprandTransmitFunction[room][f] = new alglib.complex[sprandSignal.Length];

                    double sic = -6.91 / (_sampleFrequency * reverbTime);
                    for (int si = 0; si < sprandSignal.Length; si++)
                    {
                        sprandTransmitFunction[room][f][si] = (alglib.complex)sprandSignal[si] * Math.Pow(Math.E, si * sic);
                    }

                    //_T[room][frequency] = reverbTime;
                }
            }

            _sprandTransmitFunction = sprandTransmitFunction;

            return this;
        }

        public alglib.complex[][] getRoom1TransmitFunction()
        {
            if (_sprandTransmitFunction == null)
            {
                _calculateTransmitFunction();
            }
            
            return _sprandTransmitFunction[0];
        }

        public alglib.complex[][] getRoom2TransmitFunction()
        {
            if (_sprandTransmitFunction == null)
            {
                _calculateTransmitFunction();
            }

            return _sprandTransmitFunction[1];
        }
    }
}
