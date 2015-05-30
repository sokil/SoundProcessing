using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sound.Model;

using Sound.Model.Soundprocessing;

namespace Sound.Core
{
    class ServiceLocator
    {
        private static double[] _6ChainsFilterFrequencyRange = new double[] { 125, 250, 500, 1000, 2000, 4000, 8000, 16000 };

        private static double[] _16ChainsFilterFrequencyRange = new double[] { 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 10000, 12500};

        private static DbAdapter _dbAdapter;

        private static Sound.Model.WallComposition.Covering _coveringMaterials;

        private static Sound.Model.WallConnection _wallConnection;

        private static Sound.Model.Soundprocessing.SoundTrasmittion _transmittion = new Model.Soundprocessing.SoundTrasmittion();



        public static double[] getFrequencyRangeByFilterChainNum(SoundTrasmittion.FrequencyRange range, int chainsNum = 0)
        {
            double[] frequencyRange;

            switch (range)
            {
                case SoundTrasmittion.FrequencyRange.Octave:
                {
                    frequencyRange = _6ChainsFilterFrequencyRange;
                    break;
                }

                default:
                case SoundTrasmittion.FrequencyRange.OneThirdOctave:
                {
                    frequencyRange = _16ChainsFilterFrequencyRange;
                    break;
                }
            }

            if(0 == chainsNum)
            {
                return frequencyRange;
            }
            else
            {
                if (frequencyRange.Length < chainsNum)
                    return frequencyRange;

                return frequencyRange.Take(chainsNum).ToArray<double>();
            }
        }

        /**
         * frmWallSoundInsulation collection
         */
        private static Dictionary<Wall.Name, frmWallSoundInsulation> _framePropertiesWindowCollection = new Dictionary<Wall.Name, frmWallSoundInsulation>();

        /**
         * frmWallCover collection
         */
        private static frmWallCover[] _wallCoverWindowCollection = new frmWallCover[2];

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
        public static frmWallCover getWallCoverWindow(int room)
        {
            // init form
            if (null == _wallCoverWindowCollection[room])
            {
                _wallCoverWindowCollection[room] = new frmWallCover(room);
            }

            return _wallCoverWindowCollection[room];
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

        public static Sound.Model.Soundprocessing.SoundTrasmittion getSoundTransmittion()
        {
            return _transmittion;
        }
    }
}
