using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace SignalProcessing.Signal
{
    public class Wav : AbstractSignal<Wav>
    {
        private string _filename;

        private Stream _wavStream;

        private int _decimate = 1;

        private double _channelPeakLevel;

        private bool _normalize = true;

        public Wav setStream(Stream wavStream)
        {
            _wavStream = wavStream;

            return this;
        }

        public Wav setFilename(string filename)
        {
            _filename = filename;

            return this;
        }

        public Wav setDecimate(int decimate)
        {
            _decimate = decimate;

            return this;
        }

        public Wav normalize()
        {
            _normalize = true;

            return this;
        }

        public Wav open()
        {
            // if filename specified - open it by stream
            if (_filename != null)
            {
                _wavStream = File.Open(_filename, FileMode.Open);
            }

            // if no filename no stream specified - throw error
            else if (_wavStream == null)
            {
                throw new Exception("Wav file to open not specigfied");
            }

            // get stream reader
            BinaryReader reader = new BinaryReader(_wavStream);

            byte[] buffer = new byte[4];

            /**
             * RIFF chunk
             */

            // ChunkID - "RIFF"
            reader.Read(buffer, 0, 4);

            // ChunkSize - file size
            Int32 chunkSize = reader.ReadInt32();

            // Format - "WAVE"
            reader.Read(buffer, 0, 4);

            /**
             * fmt sub-chunk
             */

            // Subchunk1ID - "fmt"
            reader.Read(buffer, 0, 4);

            // Subchunk1Size - length of format chunk (always 16 for PCM)
            reader.ReadInt32();

            // AudioFormat: 1 - PCM, 2 - other
            int format = reader.ReadInt16();

            // NumCannels: 1 - Mono, 2 - Stereo
            Int16 channelsCount = reader.ReadInt16();

            // SampleRate
            _sampleFrequency = reader.ReadInt32() / _decimate;

            // ByteRate - Bytres per second
            int byteRate = reader.ReadInt32();

            // 2 bytes 
            int blockAlign = reader.ReadInt16();

            // bits per sample
            int bitsPerSample = reader.ReadInt16();

            /**
             *  data sub-chunk
             */

            // Subchunk2ID - "data"
            reader.Read(buffer, 0, 4);

            // Subchunk2Size
            int dataLength = reader.ReadInt32();

            // prepare channelData structure            
            _channelPeakLevel = 0;

            // init temp struct
            List<double> channelData = new List<double>();

            // get data
            try
            {
                int sampleNum = 0;

                double b;

                while (true)
                {
                    switch (bitsPerSample)
                    {
                        case 8:

                            for (int channel = 0; channel < channelsCount; channel++)
                            {
                                b = reader.ReadByte();

                                if (channel > 0)
                                    continue;

                                // check peak level
                                if (b > _channelPeakLevel)
                                    _channelPeakLevel = b;

                                // add
                                if (sampleNum % _decimate != 0)
                                    continue;

                                channelData.Add(b);
                            }
                            
                            break;

                        case 16:

                            for (int channel = 0; channel < channelsCount; channel++)
                            {
                                b = reader.ReadInt16();

                                if (channel > 0)
                                    continue;

                                //check peak level
                                if (b > _channelPeakLevel)
                                    _channelPeakLevel = b;

                                //add
                                if (sampleNum % _decimate != 0)
                                    continue;

                                channelData.Add(b);
                            }
                            
                            break;
                    }

                    sampleNum++;
                }
            }
            catch (EndOfStreamException e) { e = null; }

            // close file
            reader.Close();

            // trim silence from start
            // 
            while (channelData[0] == 0)
            {
                channelData.RemoveAt(0);
            }

            // trim silence from end
            while (channelData[channelData.Count - 1] == 0)
            {
                channelData.RemoveAt(channelData.Count - 1);
            }

            // post-processing
            _signal = channelData.ToArray();
            channelData = null;

            _complexSignal = new alglib.complex[_signal.Length];
            
            // normalize if need
            if (_normalize)
            {
                for (int tickNum = 0; tickNum < _signal.Length; tickNum++)
                {
                    _signal[tickNum] = _signal[tickNum] / _channelPeakLevel;
                    _complexSignal[tickNum] = (alglib.complex) _signal[tickNum];
                }
            }
            else
            {
                for (int tickNum = 0; tickNum < _signal.Length; tickNum++)
                {
                    _complexSignal[tickNum] = (alglib.complex)_signal[tickNum];
                }
            }

            // change level
            if (_signalLevel > 0)
            {
                _applySignalLevel();
            }

            return this;
        }

        public Wav mix<T>(T mixin) where T: AbstractSignal<T>
        {
            double[] mixinSignalCounts = mixin.getSignal();

            double[] mixedSignalCounts = _signal;

            for (int i = 0; i < _signal.Length; i++)
            {
                mixedSignalCounts[i] += mixinSignalCounts[i];
            }

            Wav mixedSignal = (new Wav())
                .setSampleFrequency(getSampleFrequency())
                .setSignal(mixedSignalCounts);

            return mixedSignal;
        }

        /// <summary>
        /// Get sub of ticks of all signals in passed array
        /// </summary>
        /// <param name="?"></param>
        public static Wav fromChain(Wav[] chain)
        {
            Wav signal = chain[0];
            
            for(int i = 1; i < chain.Length; i++)
            {
                signal = signal.mix<Wav>(chain[i]);
            }

            return signal;
        }

        public override Wav save(string filename)
        {
            _filename = filename;

            BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create));

            Int32 subchunk2Size = _signal.Length * 2;

            /**
             * RIFF chunk
             */

            // ChunkID - "RIFF"
            writer.Write(new byte[] {82,73,70,70});

            // ChunkSize - file size
            writer.Write((Int32) (subchunk2Size + 32));

            // Format - "WAVE"
            writer.Write(new byte[] {87,65,86,69});

            /**
             * fmt sub-chunk
             */

            // Subchunk1ID - "fmt"
            writer.Write(new byte[] {102, 109, 116, 32});

            // Subchunk1Size - length of format chunk (always 16 for PCM)
            writer.Write((Int32) 16);

            // AudioFormat: 1 - PCM, 2 - other
            writer.Write((Int16) 1);

            // NumCannels: 1 - Mono, 2 - Stereo
            writer.Write((Int16) 1);

            // SampleRate
            writer.Write((Int32) _sampleFrequency);

            // ByteRate - Bytres per second
            writer.Write((Int32) (_sampleFrequency * 2));

            // blockAlign - 2 bytes 
            writer.Write((Int16) 2);

            // bits per sample
            writer.Write((Int16) 16);

            /**
             *  data sub-chunk
             */

            // Subchunk2ID - "data"
            writer.Write(new byte[] { 100, 97, 116, 97 });

            // Subchunk2Size - data length
            writer.Write(subchunk2Size);

            // save data
            for (int i = 0; i < _signal.Length; i++)
            {
                writer.Write((Int16)(_signal[i] * 32768));
            }

            // close file
            writer.Close();

            return this;
        }
    }
}
