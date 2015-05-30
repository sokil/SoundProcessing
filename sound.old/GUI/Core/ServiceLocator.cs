using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sound.Model;

namespace Sound.Core
{
    class ServiceLocator
    {
        private static DbAdapter _dbAdapter;

        private static Sound.Model.WallComposition.Covering _coveringMaterials;

        private static Sound.Model.WallConnection _wallConnection;

        private static Sound.Model.WallSprandTransmitFunction _wallSprandTransmitFunction;

        /**
         * frmWallSoundInsulation collection
         */
        private static Dictionary<Wall.Name, frmWallSoundInsulation> _framePropertiesWindowCollection = new Dictionary<Wall.Name, frmWallSoundInsulation>();

        /**
         * frmWallCover collection
         */
        private static Dictionary<string, frmWallCover> _wallCoverWindowCollection = new Dictionary<string, frmWallCover>();

        public static DbAdapter getDbAdapter()
        {
            if (_dbAdapter != null)
                return _dbAdapter;

            _dbAdapter = new DbAdapter();
            return _dbAdapter;
        }


        /**
         * Get frmWallCover from
         */
        public static frmWallCover getWallCoverWindow(string wallName)
        {
            // init form
            if (!_wallCoverWindowCollection.ContainsKey(wallName))
            {
                _wallCoverWindowCollection[wallName] = new frmWallCover(wallName);
            }

            return _wallCoverWindowCollection[wallName];
        }

        /**
         * Get frmWallSoundInsulation from
         */
        public static frmWallSoundInsulation getFramePropertiesWindow(Wall.Name wallName)
        {
            /**
            * init frmWallSoundInsulation forms
            */
            if (_framePropertiesWindowCollection.Count == 0)
            {
                // WallConnection wallConnection = Core.ServiceLocator.getWallConnection();
                foreach (Wall.Name _wallName in Enum.GetValues(typeof(Wall.Name)))
                {
                    _framePropertiesWindowCollection[_wallName] = new frmWallSoundInsulation(_wallName);
                }

            }

            return _framePropertiesWindowCollection[wallName];
        }

        public static Sound.Model.WallComposition.Covering getCoveringMaterials()
        {
            if (_coveringMaterials != null)
                return _coveringMaterials;

            _coveringMaterials = new Sound.Model.WallComposition.Covering();

            return _coveringMaterials;
        }

        public static Sound.Model.WallConnection getWallConnection()
        {
            if(null != _wallConnection)
                return _wallConnection;

            _wallConnection = new Sound.Model.WallConnection();

            return _wallConnection;
        }

        public static Sound.Model.WallSprandTransmitFunction getWallSprandTransmitFunction()
        {
            if (null == _wallSprandTransmitFunction)
            {
                _wallSprandTransmitFunction = new Sound.Model.WallSprandTransmitFunction();
            }

            return _wallSprandTransmitFunction;
        }
    }
}
