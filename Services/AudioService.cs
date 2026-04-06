using NAudio.Wave;
using System;
using System.IO;

namespace LegaCity.Services
{
    public class AudioService : IDisposable
    {
        private IWavePlayer? _backgroundPlayer;
        private IWavePlayer? _sfxPlayer;
        private LoopingWaveProvider? _loopingProvider;

        public AudioService()
        {
            _backgroundPlayer = new WaveOutEvent();
            _sfxPlayer = new WaveOutEvent();
        }

        public void PlayBackgroundMusic(string assetPath, float volume = 0.2f)
        {
            try
            {
                StopBackgroundMusic();

                var resourcePath = GetResourcePath(assetPath);
                if (!File.Exists(resourcePath))
                {
                    System.Diagnostics.Debug.WriteLine($"audio not found {resourcePath}");
                    return;
                }

                var reader = new AudioFileReader(resourcePath);
                reader.Volume = volume;

                _loopingProvider = new LoopingWaveProvider(reader);
                _backgroundPlayer?.Init(_loopingProvider);
                _backgroundPlayer?.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"error {ex.Message}");
            }
        }

        public void PlaySoundEffect(string assetPath, float volume = 1.0f)
        {
            try
            {
                var resourcePath = GetResourcePath(assetPath);
                if (!File.Exists(resourcePath))
                {
                    System.Diagnostics.Debug.WriteLine($"audio not found {resourcePath}");
                    return;
                }

                _sfxPlayer?.Stop();

                var reader = new AudioFileReader(resourcePath);
                reader.Volume = volume;

                _sfxPlayer?.Init(reader);
                _sfxPlayer?.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"error {ex.Message}");
            }
        }

        public void StopBackgroundMusic()
        {
            try
            {
                _backgroundPlayer?.Stop();
                _loopingProvider?.Dispose();
                _loopingProvider = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"i cant stop {ex.Message}");
            }
        }

        private string GetResourcePath(string assetPath)
        {
            var appDir = AppContext.BaseDirectory;
            return Path.Combine(appDir, assetPath);
        }

        public void Dispose()
        {
            _backgroundPlayer?.Dispose();
            _sfxPlayer?.Dispose();
            _loopingProvider?.Dispose();
        }
    }

    public class LoopingWaveProvider : IWaveProvider
    {
        private readonly AudioFileReader _reader;
        private readonly long _length;

        public WaveFormat WaveFormat => _reader.WaveFormat;

        public LoopingWaveProvider(AudioFileReader reader)
        {
            _reader = reader;
            _length = reader.Length;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int read = _reader.Read(buffer, offset, count);

            if (read < count)
            {
                _reader.CurrentTime = TimeSpan.Zero;
                int remaining = count - read;
                int additionalRead = _reader.Read(buffer, offset + read, remaining);
                read += additionalRead;
            }

            return read;
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
