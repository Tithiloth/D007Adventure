using D007Adventure.Audio.Ogg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace D007Adventure.Audio
{
    // Spielt OGG-Vorbis direkt über den eigenen C#-Decoder und waveOut ab.
    // Es werden keine externen DLLs oder NuGet-Pakete für OGG benötigt.
    public sealed class OggPlayer : IDisposable
    {
        private const uint WaveMapper = 0xFFFFFFFFu;
        private const uint CallbackFunction = 0x00030000u;
        private const ushort WaveFormatPcm = 1;
        private const uint WaveOutDone = 0x3BDu;
        private const int ChunkSize = 64 * 1024;

        private readonly object sync = new object();
        private readonly WaveOutProc callback;
        private readonly AutoResetEvent bufferDone = new AutoResetEvent(false);

        private Thread playbackThread;
        private volatile bool stopRequested;
        private volatile bool isPlaying;
        private bool disposed;
        private IntPtr waveOutHandle;

        public bool IsPlaying
        {
            get { return isPlaying; }
        }

        public OggPlayer()
        {
            // Callback muss als Feld gehalten werden, damit der Delegate
            // während der nativen waveOut-Wiedergabe nicht vom GC entfernt wird.
            callback = WaveOutCallback;
        }

        public void Play(string filePath, bool loop = false)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(OggPlayer));

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Logger.Error("OGG-Dateipfad ist leer.");
                return;
            }

            if (!File.Exists(filePath))
            {
                Logger.Error("OGG-Datei nicht gefunden: " + filePath);
                return;
            }

            Stop();

            stopRequested = false;

            playbackThread = new Thread(() => PlaybackThread(filePath, loop));
            playbackThread.IsBackground = true;
            playbackThread.Start();
        }

        public void Stop()
        {
            stopRequested = true;

            // Falls PlayChunk gerade wartet, soll der Thread sofort
            // erneut prüfen, ob die Wiedergabe beendet werden soll.
            bufferDone.Set();

            Thread thread;

            lock (sync)
            {
                thread = playbackThread;
            }

            if (thread != null && thread != Thread.CurrentThread)
            {
                if (!thread.Join(2000))
                {
                    Logger.Error(
                        "OGG-Wiedergabethread konnte nicht rechtzeitig beendet werden.");
                }
            }

            lock (sync)
            {
                playbackThread = null;
            }

            isPlaying = false;
        }

        private void PlaybackThread(string filePath, bool loop)
        {
            try
            {
                do
                {
                    if (stopRequested)
                        break;

                    byte[] pcm = DecodeFile(
                        filePath,
                        out int sampleRate,
                        out short channels);

                    if (pcm.Length == 0)
                        break;

                    PlayPcm(pcm, sampleRate, channels);
                }
                while (loop && !stopRequested);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    "OGG-Wiedergabe fehlgeschlagen: " + ex.Message);
            }
            finally
            {
                isPlaying = false;
            }
        }

        private byte[] DecodeFile(
            string filePath,
            out int sampleRate,
            out short channels)
        {
            using (FileStream stream = File.OpenRead(filePath))
            using (OggStreamReader ogg = new OggStreamReader(stream))
            {
                VorbisDecoder decoder = VorbisDecoder.Create(ogg);
                List<float[]> blocks = decoder.DecodeAll(ogg);

                sampleRate = decoder.SampleRate;

                if (decoder.Channels <= 0 ||
                    decoder.Channels > ushort.MaxValue)
                {
                    throw new InvalidOperationException(
                        "Ungültige Anzahl an OGG-Kanälen: " +
                        decoder.Channels);
                }

                channels = checked((short)decoder.Channels);

                long sampleCount = 0;

                for (int i = 0; i < blocks.Count; i++)
                    sampleCount += blocks[i].LongLength;

                if (sampleCount > int.MaxValue / 2)
                {
                    throw new InvalidOperationException(
                        "OGG-Datei ist für den aktuellen Player zu groß.");
                }

                byte[] pcm = new byte[(int)sampleCount * 2];
                int offset = 0;

                for (int i = 0; i < blocks.Count; i++)
                {
                    float[] block = blocks[i];

                    for (int j = 0; j < block.Length; j++)
                    {
                        float sample = block[j];

                        if (sample > 1.0f)
                            sample = 1.0f;

                        if (sample < -1.0f)
                            sample = -1.0f;

                        short value =
                            (short)Math.Round(sample * 32767.0f);

                        pcm[offset++] = (byte)(value & 0xFF);
                        pcm[offset++] = (byte)((value >> 8) & 0xFF);
                    }
                }

                return pcm;
            }
        }

        private void PlayPcm(
            byte[] pcm,
            int sampleRate,
            short channels)
        {
            if (sampleRate <= 0)
                throw new InvalidOperationException(
                    "Ungültige Sample-Rate: " + sampleRate);

            if (channels <= 0)
                throw new InvalidOperationException(
                    "Ungültige Kanalanzahl: " + channels);

            WAVEFORMATEX format = new WAVEFORMATEX();

            // WAVE_FORMAT_PCM ist ein ushort.
            format.wFormatTag = WaveFormatPcm;

            // nChannels ist ebenfalls ein ushort.
            format.nChannels = checked((ushort)channels);

            // nSamplesPerSec ist ein uint.
            format.nSamplesPerSec = checked((uint)sampleRate);

            // 16 Bit PCM = 2 Bytes pro Sample und Kanal.
            format.wBitsPerSample = 16;

            // Blockgröße eines einzelnen Sample-Frames.
            format.nBlockAlign =
                checked((ushort)(channels * 2));

            // Durchschnittliche Datenrate in Bytes pro Sekunde.
            format.nAvgBytesPerSec =
                checked((uint)(sampleRate * channels * 2));

            format.cbSize = 0;

            IntPtr handle;

            int result = waveOutOpen(
                out handle,
                WaveMapper,
                ref format,
                callback,
                IntPtr.Zero,
                CallbackFunction);

            if (result != 0)
            {
                throw new InvalidOperationException(
                    "waveOutOpen fehlgeschlagen: " + result);
            }

            lock (sync)
            {
                waveOutHandle = handle;
            }

            isPlaying = true;

            try
            {
                int position = 0;

                while (position < pcm.Length && !stopRequested)
                {
                    int length = Math.Min(
                        ChunkSize,
                        pcm.Length - position);

                    byte[] chunk = new byte[length];

                    Buffer.BlockCopy(
                        pcm,
                        position,
                        chunk,
                        0,
                        length);

                    position += length;

                    PlayChunk(handle, chunk);
                }

                waveOutReset(handle);
            }
            finally
            {
                waveOutReset(handle);

                lock (sync)
                {
                    waveOutHandle = IntPtr.Zero;
                }

                waveOutClose(handle);

                isPlaying = false;
            }
        }

        private void PlayChunk(
            IntPtr handle,
            byte[] data)
        {
            GCHandle pinned =
                GCHandle.Alloc(data, GCHandleType.Pinned);

            IntPtr headerPointer =
                Marshal.AllocHGlobal(
                    Marshal.SizeOf(typeof(WAVEHDR)));

            WAVEHDR header = new WAVEHDR();

            header.lpData = pinned.AddrOfPinnedObject();
            header.dwBufferLength = (uint)data.Length;
            header.dwFlags = 0;

            bool prepared = false;

            try
            {
                Marshal.StructureToPtr(
                    header,
                    headerPointer,
                    false);

                int headerSize =
                    Marshal.SizeOf(typeof(WAVEHDR));

                int result = waveOutPrepareHeader(
                    handle,
                    headerPointer,
                    (uint)headerSize);

                if (result != 0)
                {
                    throw new InvalidOperationException(
                        "waveOutPrepareHeader fehlgeschlagen: " +
                        result);
                }

                prepared = true;

                bufferDone.Reset();

                result = waveOutWrite(
                    handle,
                    headerPointer,
                    (uint)headerSize);

                if (result != 0)
                {
                    waveOutUnprepareHeader(
                        handle,
                        headerPointer,
                        (uint)headerSize);

                    prepared = false;

                    throw new InvalidOperationException(
                        "waveOutWrite fehlgeschlagen: " +
                        result);
                }

                while (!stopRequested)
                {
                    if (bufferDone.WaitOne(100))
                        break;
                }

                // Bei Stop() wird waveOutReset() später aufgerufen.
                // Danach kann der Header wieder freigegeben werden.
                if (prepared)
                {
                    waveOutUnprepareHeader(
                        handle,
                        headerPointer,
                        (uint)headerSize);

                    prepared = false;
                }
            }
            finally
            {
                if (prepared)
                {
                    waveOutUnprepareHeader(
                        handle,
                        headerPointer,
                        (uint)Marshal.SizeOf(typeof(WAVEHDR)));
                }

                Marshal.FreeHGlobal(headerPointer);
                pinned.Free();
            }
        }

        private void WaveOutCallback(
            IntPtr hwo,
            uint message,
            IntPtr instance,
            IntPtr parameter1,
            IntPtr parameter2)
        {
            if (message == WaveOutDone)
                bufferDone.Set();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            bufferDone.Dispose();

            Logger.Info("OggPlayer wurde beendet.");
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void WaveOutProc(
            IntPtr hwo,
            uint message,
            IntPtr instance,
            IntPtr parameter1,
            IntPtr parameter2);

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEHDR
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags;
            public uint dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        [DllImport("winmm.dll", SetLastError = false)]
        private static extern int waveOutOpen(
            out IntPtr waveOut,
            uint deviceId,
            ref WAVEFORMATEX format,
            WaveOutProc callback,
            IntPtr instance,
            uint flags);

        [DllImport("winmm.dll", SetLastError = false)]
        private static extern int waveOutPrepareHeader(
            IntPtr waveOut,
            IntPtr header,
            uint headerSize);

        [DllImport("winmm.dll", SetLastError = false)]
        private static extern int waveOutUnprepareHeader(
            IntPtr waveOut,
            IntPtr header,
            uint headerSize);

        [DllImport("winmm.dll", SetLastError = false)]
        private static extern int waveOutWrite(
            IntPtr waveOut,
            IntPtr header,
            uint headerSize);

        [DllImport("winmm.dll", SetLastError = false)]
        private static extern int waveOutReset(
            IntPtr waveOut);

        [DllImport("winmm.dll", SetLastError = false)]
        private static extern int waveOutClose(
            IntPtr waveOut);
    }
}