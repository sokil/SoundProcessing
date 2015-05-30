using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sound.Model.Soundinsulation;

namespace Sound.Model
{
    public class Wall
    {
        public enum Name
        {
            Room1Floor      = 1,
            Room2Floor      = 2,
            Room1Seiling    = 4,
            Room2Seiling    = 8,
            Room1BackWall   = 16,
            Room2BackWall   = 32,
            Room1FrontWall  = 64,
            Room2FrontWall  = 128,
            SeparateWall    = 256
        }

        public enum Type
        {
            Monolith        = 1,
            Onelayer        = 2,
            Frame           = 4
        }

        private AbstractWallInsulation _soundInsulation;

        private Name _name;

        private Type _type;

        public Wall setName(Name name)
        {
            _name = name;

            return this;
        }

        public Name getName()
        {
            return _name;
        }

        public bool hasName(Name name)
        {
            return name == _name;
        }

        public Wall setType(Type type)
        {
            _type = type;

            _soundInsulation = null;

            return this;
        }

        public Type getType()
        {
            return _type;
        }

        public AbstractWallInsulation getSoundInsulation()
        {
            if(null != _soundInsulation)
                return _soundInsulation;

            switch(getType())
            {
                case Type.Monolith:
                    _soundInsulation = new MonolithWallInsulation(_name);
                    break;

                case Type.Onelayer:
                    _soundInsulation = new OnelayerWallInsulation();
                    break;

                case Type.Frame:
                    _soundInsulation = new FrameWallInsulation();
                    break;
            }

            return _soundInsulation;
        }

        public bool hasType(Type type)
        {
            return type == _type;
        }

        public double getSurfaceMass()
        {
            return getSoundInsulation().getSurfaceDensity();
        }
    }
}
