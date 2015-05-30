using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sound.Model
{
    public class WallConnection
    {
        private double _room1Height;
        private double _room1Width;
        private double _room1Length;

        private double _room2Height;
        private double _room2Width;
        private double _room2Length;

        private double _room1Volume;
        private double _room1FloorSquare;

        private double _room2Volume;
        private double _room2FloorSquare;

        public enum Connection
        {
            Cross       = 0,
            Consecutive = 1
        }

        public enum SeparateWallConnectPosition
        {
            Top     = 1,
            Front   = 2,
            Bottom  = 4,
            Back    = 8
        }

        private Dictionary<Wall.Name, Wall> _wallType = new Dictionary<Wall.Name, Wall>();

        public delegate double TransmitCoeficientFunction(double f);

        private Dictionary<SeparateWallConnectPosition, bool> _flexibleConnections = new Dictionary<SeparateWallConnectPosition, bool>();

        public WallConnection()
        {
            // set default connection to non-flexible
            foreach (SeparateWallConnectPosition connection in Enum.GetValues(typeof(SeparateWallConnectPosition)))
            {
                _flexibleConnections[connection] = false;
            }
        }

        /* public WallConnection setRoom1Dimensions(double height, double width, double length)
        {
            _room1Height = height;

            _room1Width = width;

            _room1Length = length;

            _room1FloorSquare = _room1Length * _room1Width;

            _room1Volume = _room1FloorSquare * _room1Height;

            return this;
        } */

        public WallConnection setRoom1Height(double height)
        {
            _room1Height = height;

            if (null != _room1FloorSquare)
            {
                _room1Volume = _room1FloorSquare * _room1Height;
            }

            return this;
        }

        public WallConnection setRoom1Width(double width)
        {
            _room1Width = width;

            if (null != _room1Length)
            {
                _room1FloorSquare = _room1Length * _room1Width;

                if (null != _room1Height)
                {
                    _room1Volume = _room1FloorSquare * _room1Height;
                }
            }

            return this;
        }

        public WallConnection setRoom1Length(double length)
        {
            _room1Length = length;

            if (null != _room1Width)
            {
                _room1FloorSquare = _room1Length * _room1Width;

                if(null != _room1Height)
                {
                    _room1Volume = _room1FloorSquare * _room1Height;
                }
            }

            return this;
        }

        public WallConnection setRoom2Height(double height)
        {
            _room2Height = height;

            if (null != _room2FloorSquare)
            {
                _room2Volume = _room2FloorSquare * _room2Height;
            }

            return this;
        }

        public WallConnection setRoom2Width(double width)
        {
            _room2Width = width;

            if (null != _room2Length)
            {
                _room2FloorSquare = _room2Length * _room2Width;

                if (null != _room2Height)
                {
                    _room2Volume = _room2FloorSquare * _room2Height;
                }
            }

            return this;
        }

        public WallConnection setRoom2Length(double length)
        {
            _room2Length = length;

            if (null != _room2Width)
            {
                _room2FloorSquare = _room2Length * _room2Width;

                if (null != _room2Height)
                {
                    _room2Volume = _room2FloorSquare * _room2Height;
                }
            }

            return this;
        }

        public double getRoom1Height() { return _room1Height;  }
        public double getRoom1Width() { return _room1Width; }
        public double getRoom1Length() { return _room1Length; }
        public double getRoom2Height() { return _room2Height; }
        public double getRoom2Width() { return _room2Width; }
        public double getRoom2Length() { return _room2Length; }

        public double getWallSquare(string wallName)
        {
            double s = 0;

            switch (wallName)
            {
                case "Room1Floor": s = _room1FloorSquare; break;
                case "Room2Floor": s = _room2FloorSquare; break;
                case "Room1Seiling": s = _room1FloorSquare; break;
                case "Room2Seiling": s = _room2FloorSquare; break;
                case "Room1BackWall": s = _room1Width * _room1Height; break;
                case "Room2BackWall": s = _room2Width * _room2Height; break;
                case "Room1FrontWall": s = _room1Width * _room1Height; break;
                case "Room2FrontWall": s = _room2Width * _room2Height; break;
                case "Room1LeftWall": s = _room1Height * _room1Length; break;
                case "Room1SeparateWall": s = _room1Height * _room1Length; break;
                case "Room2SeparateWall": s = _room2Height * _room2Length; break;
                case "SeparateWall": 
                    double room1Square = getWallSquare("Room1SeparateWall");
                    double room2Square = getWallSquare("Room2SeparateWall");

                    s = room1Square > room2Square ? room2Square : room1Square;
                    break;
                case "Room2RightWall": s = _room2Height * _room2Length; break;
            }

            return s;
        }

        public double[] getRoomVolumes()
        {
            return new double[] {
                _room1Volume,
                _room2Volume
            };
        }

        public double[] getRoomFloorSquares()
        {
            return new double[] {
                _room1FloorSquare,
                _room2FloorSquare
            };
        }

        public WallConnection setWall(Wall wall)
        {
            _wallType[wall.getName()] = wall;

            return this;
        }

        public Connection getConnection(Wall room1Wall, Wall room2Wall)
        {
            if (room1Wall.hasName(Wall.Name.SeparateWall))
                return Connection.Cross;

            if (room2Wall.hasName(Wall.Name.SeparateWall))
                return Connection.Cross;

            return Connection.Consecutive;
        }

        public bool hasCrossConnection(Wall room1Wall, Wall room2Wall)
        {
            return Connection.Cross == getConnection(room1Wall, room2Wall);
        }

        public bool hasConsecutiveConnection(Wall room1Wall, Wall room2Wall)
        {
            return Connection.Consecutive == getConnection(room1Wall, room2Wall);
        }

        public TransmitCoeficientFunction getTransmitCoeficient(Wall room1Wall, Wall room2Wall)
        {
            // vibro insulation coeficient
            double M = Math.Log10( room1Wall.getSurfaceMass() / room2Wall.getSurfaceMass() );

            // if connection not flexible
            if(!isWallConnectionFlexible(room1Wall, room2Wall))
            {
                int relation;
                if ((int)room1Wall.getType() == (int)room2Wall.getType())
                {
                    relation = (int)room1Wall.getType();
                }
                else
                {
                    relation = (int)room1Wall.getType() + (int)room2Wall.getType();
                }

                // both monolite
                if(relation == (int)Wall.Type.Monolith)
                {
                    if (hasConsecutiveConnection(room1Wall, room2Wall))
                    {
                        return delegate(double f) { return 5.7 + 14.1 * M + 5.7 * M * M; };
                    }
                    else
                    {
                        return delegate(double f) { return 5.7 + 5.7 * M * M; };
                    }
                }

                // one of panels is ONELAYER
                if ((relation & (int) Wall.Type.Onelayer) == (int) Wall.Type.Onelayer)
                {
                    if (hasConsecutiveConnection(room1Wall, room2Wall))
                    {
                        return delegate(double f) { return 5 + 10 * M; };
                    }
                    else
                    {
                        return delegate(double f) { return 10 + 10 * Math.Abs(M); };
                    }
                }

                // both are FRAME
                if (relation == (int)Wall.Type.Frame)
                {
                    if (hasConsecutiveConnection(room1Wall, room2Wall))
                    {
                        return delegate(double f) { return 10 + 20 * M - 3.3 * Math.Log10(f / 500); };
                    }
                    else
                    {
                        return delegate(double f) { return 10 + 10 * Math.Abs(M) + 3.3 * Math.Log10( f / 500 ); };
                    }
                }

                // MONOLITH + FRAME
                if (relation == (int)Wall.Type.Monolith + (int)Wall.Type.Frame)
                {
                    if (hasConsecutiveConnection(room1Wall, room2Wall))
                    {
                        return delegate(double f) { return 3 - 14.1 * M + 5.7 * M * M; };
                    }
                    else
                    {
                        return delegate(double f) { return 10 + 10 * Math.Abs(M) + 3.3 * Math.Log10( f / 500 ); };
                    }
                }

                throw new NotImplementedException();
            }

            // if connection flexible
            else
            {
                if (hasConsecutiveConnection(room1Wall, room2Wall))
                {
                    return delegate(double f) { return 3.7 + 14.1 * M + 5.7  * M * M; };
                }
                else
                {
                    return delegate(double f) { return 5.7 + 5.7 * M * M + 10 * Math.Log10( f / 125); };
                }
            }

            
        }

        public WallConnection setWallConnectionFlexibility(SeparateWallConnectPosition connection, bool isFlexible)
        {
            _flexibleConnections[connection] = isFlexible;

            return this;
        }

        public bool isWallConnectionFlexible(SeparateWallConnectPosition connection)
        {
            return _flexibleConnections[connection];
        }

        public SeparateWallConnectPosition getConnectPosition(Wall room1Wall, Wall room2Wall)
        {
            int room1WallName = (int)room1Wall.getName();
            int room2WallName = (int)room2Wall.getName();

            switch (room1WallName + room2WallName)
            {
                case (int) Wall.Name.Room1BackWall + (int) Wall.Name.Room2BackWall:
                case (int)Wall.Name.Room1BackWall + (int)Wall.Name.SeparateWall:
                case (int)Wall.Name.SeparateWall + (int)Wall.Name.Room2BackWall:
                    return SeparateWallConnectPosition.Back;

                case (int)Wall.Name.Room1Floor + (int)Wall.Name.Room2Floor:
                case (int)Wall.Name.Room1Floor + (int)Wall.Name.SeparateWall:
                case (int)Wall.Name.SeparateWall + (int)Wall.Name.Room2Floor:
                    return SeparateWallConnectPosition.Bottom;

                case (int)Wall.Name.Room1Seiling + (int)Wall.Name.Room2Seiling:
                case (int)Wall.Name.Room1Seiling + (int)Wall.Name.SeparateWall:
                case (int)Wall.Name.SeparateWall + (int)Wall.Name.Room2Seiling:
                    return SeparateWallConnectPosition.Top;

                case (int)Wall.Name.Room1FrontWall + (int)Wall.Name.Room2FrontWall:
                case (int)Wall.Name.Room1FrontWall + (int)Wall.Name.SeparateWall:
                case (int)Wall.Name.SeparateWall + (int)Wall.Name.Room2FrontWall:
                    return SeparateWallConnectPosition.Front;

                default:
                    throw new Exception("Walls not connected through separate wall");
            }
        }

        public bool isWallConnectionFlexible(Wall room1Wall, Wall room2Wall)
        {
            SeparateWallConnectPosition position = getConnectPosition(room1Wall, room2Wall);

            return isWallConnectionFlexible(position);
        }

        private double _getDimensionCoefitient(Wall room1Wall, Wall room2Wall)
        {
            int relation = (int)room1Wall.getName() + (int)room2Wall.getName();

            bool topOrBottom = (
                ((relation & (int) Wall.Name.Room1Seiling) == (int) Wall.Name.Room1Seiling) || 
                ((relation & (int) Wall.Name.Room2Seiling) == (int) Wall.Name.Room2Seiling) ||
                ((relation & (int) Wall.Name.Room1Floor) == (int) Wall.Name.Room1Floor) ||
                ((relation & (int) Wall.Name.Room2Floor) == (int) Wall.Name.Room2Floor)
            );

            double length;

            // seiling or floor
            if ( topOrBottom )
            {
                length = _room1Length < _room2Length
                    ? _room1Length 
                    : _room2Length;
            }
            // back or front
            else
            {
                length = _room1Height < _room2Height
                    ? _room1Height
                    : _room2Height;
            }

            return 10 * Math.Log10(length);
        }
        
        private TransmitCoeficientFunction _getFlankingTransmittion(Wall.Name room1WallName, Wall.Name room2WallName)
        {
            Wall room1Wall = _wallType[room1WallName];
            Wall room2Wall = _wallType[room2WallName];

            TransmitCoeficientFunction transmitCoefitientFunction = getTransmitCoeficient(room1Wall, room2Wall);
            double dimensionCoefitient = _getDimensionCoefitient(room1Wall, room2Wall);

            SortedList<double,double> wall1FrequencyCharacteristic = room1Wall
                .getSoundInsulation()
                .getFrequencyCharacteristic();

            SortedList<double, double> wall2FrequencyCharacteristic = room1Wall
                .getSoundInsulation()
                .getFrequencyCharacteristic();

            return delegate(double w)
            {
                return transmitCoefitientFunction(w) 
                    + dimensionCoefitient
                    + wall1FrequencyCharacteristic[w] / 2
                    + wall2FrequencyCharacteristic[w] / 2;
            };
        }

        public SortedList<double, double> applyFlankingTransmittion(SortedList<double, double> separateWallInsulationFrequencyCharacteristic)
        {
            Wall.Name[][] map = new Wall.Name[][]
            {
                new Wall.Name[] { Wall.Name.Room1Seiling, Wall.Name.Room2Seiling},
                new Wall.Name[] { Wall.Name.Room1Floor, Wall.Name.Room2Floor},
                new Wall.Name[] { Wall.Name.Room1FrontWall, Wall.Name.Room2FrontWall},
                new Wall.Name[] { Wall.Name.Room1BackWall, Wall.Name.Room2BackWall},
                
                new Wall.Name[] { Wall.Name.Room1Seiling, Wall.Name.SeparateWall},
                new Wall.Name[] { Wall.Name.Room1Floor, Wall.Name.SeparateWall},
                new Wall.Name[] { Wall.Name.Room1FrontWall, Wall.Name.SeparateWall},
                new Wall.Name[] { Wall.Name.Room1BackWall, Wall.Name.SeparateWall},

                new Wall.Name[] { Wall.Name.Room2Seiling, Wall.Name.SeparateWall},
                new Wall.Name[] { Wall.Name.Room2Floor, Wall.Name.SeparateWall},
                new Wall.Name[] { Wall.Name.Room2FrontWall, Wall.Name.SeparateWall},
                new Wall.Name[] { Wall.Name.Room2BackWall, Wall.Name.SeparateWall},
            };

            // init characteristion by insulation of separate wall
            SortedList<double, double> characterisctic = new SortedList<double, double>();
            foreach (KeyValuePair<double, double> freqInsulation in separateWallInsulationFrequencyCharacteristic)
            {
                characterisctic[ freqInsulation.Key ] = Math.Pow(10, -0.1 * freqInsulation.Value);
            }

            // add flanking transmit insulation
            for (int i = 0; i < map.Length; i++)
            {
                TransmitCoeficientFunction flankingTransmitFunction = _getFlankingTransmittion(map[i][0], map[i][1]);
                foreach (KeyValuePair<double, double> freqInsulation in separateWallInsulationFrequencyCharacteristic)
                {              
                    characterisctic[ freqInsulation.Key ] += Math.Pow(10, -0.1 * flankingTransmitFunction( freqInsulation.Key ));
                }
            }

            // logarifm
            foreach (KeyValuePair<double, double> freqInsulation in separateWallInsulationFrequencyCharacteristic)
            {
                characterisctic[ freqInsulation.Key ] = -10 * Math.Log10( characterisctic[ freqInsulation.Key ] );
            }

            return characterisctic;
        }
    }
}
