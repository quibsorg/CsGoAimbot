﻿using System;
using System.IO;
using SharpDX.XAudio2;
using SharpDX.Multimedia;

namespace CsGoApplicationAimbot
{
    public class SoundManager : IDisposable
    {
        #region Variables
        private XAudio2 _audio;
        private MasteringVoice _masteringVoice;
        private SoundStream[] _soundStreams;
        private AudioBuffer[] _audioBuffers;
        private SourceVoice[] _sourceVoices;
        #endregion

        #region Constructor
        public SoundManager(int sounds)
        {
            _audio = new XAudio2();
            _masteringVoice = new MasteringVoice(_audio);
            _masteringVoice.SetVolume(0.5f);
            _soundStreams = new SoundStream[sounds];
            _audioBuffers = new AudioBuffer[sounds];
            _sourceVoices = new SourceVoice[sounds];   
        }
        #endregion

        #region Methods
        //Adds the sound we want to play
        public void Add(int index, UnmanagedMemoryStream stream)
        {
            _soundStreams[index] = new SoundStream(stream);
            _audioBuffers[index] = new AudioBuffer();
            _audioBuffers[index].Stream = _soundStreams[index].ToDataStream();
            _audioBuffers[index].AudioBytes = (int) _soundStreams[index].Length;
            _audioBuffers[index].Flags = BufferFlags.EndOfStream;
            _sourceVoices[index] = new SourceVoice(_audio, _soundStreams[index].Format);
        }

        //Plays the sound
        public void Play(int index)
        {
            _sourceVoices[index].Stop();
            _sourceVoices[index].FlushSourceBuffers();
            _sourceVoices[index].SubmitSourceBuffer(_audioBuffers[index], _soundStreams[index].DecodedPacketsInfo);
            _sourceVoices[index].Start();
        }

        //Sets the voulme of the sound we'll play.
        public void SetVolume(float volume)
        {
            _masteringVoice.SetVolume(volume);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (_audio != null)
            {
                _audio.StopEngine();
                _audio.Dispose();
                _audio = null;

                foreach (var sourceVoice in _sourceVoices)
                {
                    sourceVoice.Dispose();
                }
                foreach (var soundStream in _soundStreams)
                {
                    soundStream.Dispose();
                }
            }
        }
        #endregion
    }
}
