using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D007Adventure.Audio.Ogg
{
    // Vollständiger Vorbis-I-Decoder für OGG-Vorbis-Pakete.
    public sealed class VorbisDecoder
    {
        private readonly int blockSize0;
        private readonly int blockSize1;
        private readonly int channels;
        private readonly int sampleRate;
        private readonly VorbisCodebook[] codebooks;
        private readonly VorbisFloor[] floors;
        private readonly VorbisResidue[] residues;
        private readonly VorbisMapping[] mappings;
        private readonly VorbisMode[] modes;

        private float[][] previousFrame;
        private int previousBlockSize;
        private bool havePreviousFrame;

        public int Channels
        {
            get { return channels; }
        }

        public int SampleRate
        {
            get { return sampleRate; }
        }

        public int BlockSize0
        {
            get { return blockSize0; }
        }

        public int BlockSize1
        {
            get { return blockSize1; }
        }

        private VorbisDecoder(
            byte[] identification,
            byte[] comment,
            byte[] setup)
        {
            int parsedSampleRate;
            int parsedChannels;
            int parsedBlockSize0;
            int parsedBlockSize1;

            ParseIdentification(
                identification,
                out parsedSampleRate,
                out parsedChannels,
                out parsedBlockSize0,
                out parsedBlockSize1);

            ParseComment(comment);

            VorbisCodebook[] parsedCodebooks;
            VorbisFloor[] parsedFloors;
            VorbisResidue[] parsedResidues;
            VorbisMapping[] parsedMappings;
            VorbisMode[] parsedModes;

            ParseSetup(
                setup,
                parsedChannels,
                out parsedCodebooks,
                out parsedFloors,
                out parsedResidues,
                out parsedMappings,
                out parsedModes);

            sampleRate = parsedSampleRate;
            channels = parsedChannels;
            blockSize0 = parsedBlockSize0;
            blockSize1 = parsedBlockSize1;

            codebooks = parsedCodebooks;
            floors = parsedFloors;
            residues = parsedResidues;
            mappings = parsedMappings;
            modes = parsedModes;

            previousFrame = null;
            previousBlockSize = 0;
            havePreviousFrame = false;
        }

        public static VorbisDecoder Create(OggStreamReader stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            OggPacket identificationPacket =
                stream.ReadPacket();

            OggPacket commentPacket =
                stream.ReadPacket();

            OggPacket setupPacket =
                stream.ReadPacket();

            if (identificationPacket == null ||
                commentPacket == null ||
                setupPacket == null ||
                identificationPacket.IsEndOfStream ||
                commentPacket.IsEndOfStream ||
                setupPacket.IsEndOfStream)
            {
                throw new InvalidDataException(
                    "Vorbis-Header fehlen.");
            }

            byte[] identification =
                identificationPacket.Data;

            byte[] comment =
                commentPacket.Data;

            byte[] setup =
                setupPacket.Data;

            Logger.Info(
                "Vorbis Identification-Paket: " +
                identification.Length +
                " Bytes.");

            Logger.Info(
                "Vorbis Comment-Paket: " +
                comment.Length +
                " Bytes.");

            Logger.Info(
                "Vorbis Setup-Paket: " +
                setup.Length +
                " Bytes.");

            ValidateHeader(
                identification,
                1,
                "Identification");

            ValidateHeader(
                comment,
                3,
                "Comment");

            ValidateHeader(
                setup,
                5,
                "Setup");

            return new VorbisDecoder(
                identification,
                comment,
                setup);
        }

        public List<float[]> DecodeAll(OggStreamReader stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            List<float[]> output =
                new List<float[]>();

            while (true)
            {
                OggPacket packetObject =
                    stream.ReadPacket();

                if (packetObject == null ||
                    packetObject.IsEndOfStream)
                {
                    break;
                }

                byte[] packet =
                    packetObject.Data;

                if (packet == null ||
                    packet.Length == 0)
                {
                    continue;
                }

                float[][] frame;
                int blockSize;

                try
                {
                    frame = DecodeAudioPacket(
                        packet,
                        out blockSize);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Error(
                        "Vorbis-Audiopaket konnte nicht dekodiert werden: " +
                        ex.Message);

                    continue;
                }

                if (frame == null)
                    continue;

                if (!havePreviousFrame)
                {
                    previousFrame = frame;
                    previousBlockSize = blockSize;
                    havePreviousFrame = true;
                    continue;
                }

                float[][] overlapped =
                    Overlap(
                        previousFrame,
                        previousBlockSize,
                        frame,
                        blockSize);

                AppendInterleaved(
                    output,
                    overlapped);

                previousFrame = frame;
                previousBlockSize = blockSize;
            }

            if (havePreviousFrame)
            {
                int start =
                    previousBlockSize / 2;

                int count =
                    previousBlockSize - start;

                float[][] tail =
                    new float[channels][];

                for (int channel = 0;
                     channel < channels;
                     channel++)
                {
                    tail[channel] =
                        new float[count];

                    Array.Copy(
                        previousFrame[channel],
                        start,
                        tail[channel],
                        0,
                        count);
                }

                AppendInterleaved(
                    output,
                    tail);
            }

            return output;
        }

        private float[][] DecodeAudioPacket(
            byte[] packet,
            out int blockSize)
        {
            if (packet == null)
                throw new ArgumentNullException(
                    nameof(packet));

            if (packet.Length == 0)
            {
                blockSize = 0;
                return null;
            }

            OggBitReader reader =
                new OggBitReader(packet);

            // Audio-Paket-Typbit.
            // 0 = Audio
            // 1 = Header
            if (reader.ReadBit() != 0)
            {
                blockSize = 0;
                return null;
            }

            int modeBits =
                ILog(modes.Length - 1);

            int modeNumber;

            if (modeBits == 0)
            {
                modeNumber = 0;
            }
            else
            {
                modeNumber =
                    (int)reader.ReadBits(modeBits);
            }

            if (modeNumber < 0 ||
                modeNumber >= modes.Length)
            {
                throw new InvalidOperationException(
                    "Ungültiger Vorbis-Mode.");
            }

            VorbisMode mode =
                modes[modeNumber];

            blockSize =
                mode.BlockFlag
                    ? blockSize1
                    : blockSize0;

            bool previousWindowFlag = true;
            bool nextWindowFlag = true;

            if (mode.BlockFlag)
            {
                previousWindowFlag =
                    reader.ReadBit() != 0;

                nextWindowFlag =
                    reader.ReadBit() != 0;
            }

            if (mode.Mapping < 0 ||
                mode.Mapping >= mappings.Length)
            {
                throw new InvalidOperationException(
                    "Ungültiges Vorbis-Mapping.");
            }

            VorbisMapping mapping =
                mappings[mode.Mapping];

            float[][] floorsDecoded =
                new float[channels][];

            bool[] noResidue =
                new bool[channels];

            for (int channel = 0;
                 channel < channels;
                 channel++)
            {
                int submap =
                    mapping.Mux[channel];

                if (submap < 0 ||
                    submap >= mapping.Submaps)
                {
                    throw new InvalidOperationException(
                        "Ungültiges Vorbis-Submap.");
                }

                int floorNumber =
                    mapping.SubmapFloor[submap];

                if (floorNumber < 0 ||
                    floorNumber >= floors.Length)
                {
                    throw new InvalidOperationException(
                        "Ungültige Vorbis-Floor-Nummer.");
                }

                floorsDecoded[channel] =
                    floors[floorNumber].Decode(
                        reader,
                        blockSize / 2,
                        codebooks);

                noResidue[channel] =
                    floors[floorNumber].IsUnused;
            }

            for (int i = mapping.CouplingSteps - 1;
                 i >= 0;
                 i--)
            {
                int magnitude =
                    mapping.Magnitude[i];

                int angle =
                    mapping.Angle[i];

                if (!noResidue[magnitude] ||
                    !noResidue[angle])
                {
                    noResidue[magnitude] = false;
                    noResidue[angle] = false;
                }
            }

            float[][] residue =
                new float[channels][];

            for (int channel = 0;
                 channel < channels;
                 channel++)
            {
                residue[channel] =
                    new float[blockSize / 2];
            }

            for (int submap = 0;
                 submap < mapping.Submaps;
                 submap++)
            {
                List<int> channelList =
                    new List<int>();

                for (int channel = 0;
                     channel < channels;
                     channel++)
                {
                    if (mapping.Mux[channel] ==
                        submap)
                    {
                        channelList.Add(channel);
                    }
                }

                if (channelList.Count == 0)
                    continue;

                float[][] submapVectors =
                    new float[channelList.Count][];

                bool[] submapFlags =
                    new bool[channelList.Count];

                for (int i = 0;
                     i < channelList.Count;
                     i++)
                {
                    int channel =
                        channelList[i];

                    submapVectors[i] =
                        residue[channel];

                    submapFlags[i] =
                        noResidue[channel];
                }

                int residueNumber =
                    mapping.SubmapResidue[submap];

                if (residueNumber < 0 ||
                    residueNumber >= residues.Length)
                {
                    throw new InvalidOperationException(
                        "Ungültige Vorbis-Residue-Nummer.");
                }

                residues[residueNumber].Decode(
                    reader,
                    submapVectors,
                    submapFlags,
                    codebooks,
                    blockSize);
            }

            for (int i = mapping.CouplingSteps - 1;
                 i >= 0;
                 i--)
            {
                float[] magnitude =
                    residue[mapping.Magnitude[i]];

                float[] angle =
                    residue[mapping.Angle[i]];

                for (int j = 0;
                     j < magnitude.Length;
                     j++)
                {
                    float m =
                        magnitude[j];

                    float a =
                        angle[j];

                    float newMagnitude;
                    float newAngle;

                    if (m > 0.0f)
                    {
                        if (a > 0.0f)
                        {
                            newMagnitude = m;
                            newAngle = m - a;
                        }
                        else
                        {
                            newAngle = m;
                            newMagnitude = m + a;
                        }
                    }
                    else
                    {
                        if (a > 0.0f)
                        {
                            newMagnitude = m;
                            newAngle = m + a;
                        }
                        else
                        {
                            newAngle = m;
                            newMagnitude = m - a;
                        }
                    }

                    magnitude[j] =
                        newMagnitude;

                    angle[j] =
                        newAngle;
                }
            }

            float[][] time =
                new float[channels][];

            VorbisMdct mdct =
                new VorbisMdct(blockSize);

            double[] window =
                BuildWindow(
                    blockSize,
                    mode.BlockFlag,
                    previousWindowFlag,
                    nextWindowFlag,
                    blockSize0);

            for (int channel = 0;
                 channel < channels;
                 channel++)
            {
                float[] spectrum =
                    residue[channel];

                if (!noResidue[channel])
                {
                    for (int i = 0;
                         i < spectrum.Length;
                         i++)
                    {
                        spectrum[i] *=
                            floorsDecoded[channel][i];
                    }
                }
                else
                {
                    Array.Clear(
                        spectrum,
                        0,
                        spectrum.Length);
                }

                float[] samples =
                    mdct.Inverse(spectrum);

                for (int i = 0;
                     i < samples.Length;
                     i++)
                {
                    samples[i] =
                        (float)(
                            samples[i] *
                            window[i]);
                }

                time[channel] =
                    samples;
            }

            return time;
        }

        private static float[][] Overlap(
            float[][] previous,
            int previousSize,
            float[][] current,
            int currentSize)
        {
            int count =
                previousSize / 4 +
                currentSize / 4;

            float[][] result =
                new float[previous.Length][];

            int currentOffset =
                (currentSize - previousSize) / 4;

            for (int channel = 0;
                 channel < previous.Length;
                 channel++)
            {
                result[channel] =
                    new float[count];

                for (int i = 0;
                     i < count;
                     i++)
                {
                    int previousIndex =
                        previousSize / 2 + i;

                    int currentIndex =
                        currentOffset + i;

                    float value = 0.0f;

                    if (previousIndex >= 0 &&
                        previousIndex <
                        previous[channel].Length)
                    {
                        value +=
                            previous[channel][previousIndex];
                    }

                    if (currentIndex >= 0 &&
                        currentIndex <
                        current[channel].Length)
                    {
                        value +=
                            current[channel][currentIndex];
                    }

                    result[channel][i] =
                        value;
                }
            }

            return result;
        }

        private static void AppendInterleaved(
            List<float[]> output,
            float[][] channelsData)
        {
            if (channelsData == null ||
                channelsData.Length == 0)
            {
                return;
            }

            int count =
                channelsData[0].Length;

            float[] block =
                new float[
                    count *
                    channelsData.Length];

            for (int i = 0;
                 i < count;
                 i++)
            {
                for (int channel = 0;
                     channel < channelsData.Length;
                     channel++)
                {
                    block[
                        i *
                        channelsData.Length +
                        channel] =
                        channelsData[channel][i];
                }
            }

            output.Add(block);
        }

        private static double[] BuildWindow(
            int n,
            bool longBlock,
            bool previousFlag,
            bool nextFlag,
            int shortSize)
        {
            double[] window =
                new double[n];

            int center =
                n / 2;

            int leftStart;
            int leftEnd;
            int leftN;

            int rightStart;
            int rightEnd;
            int rightN;

            if (longBlock &&
                !previousFlag)
            {
                leftStart =
                    n / 4 -
                    shortSize / 4;

                leftEnd =
                    n / 4 +
                    shortSize / 4;

                leftN =
                    shortSize / 2;
            }
            else
            {
                leftStart = 0;
                leftEnd = center;
                leftN = center;
            }

            if (longBlock &&
                !nextFlag)
            {
                rightStart =
                    n * 3 / 4 -
                    shortSize / 4;

                rightEnd =
                    n * 3 / 4 +
                    shortSize / 4;

                rightN =
                    shortSize / 2;
            }
            else
            {
                rightStart = center;
                rightEnd = n;
                rightN = center;
            }

            for (int i = leftStart;
                 i < leftEnd &&
                 i < n;
                 i++)
            {
                double x =
                    (double)(
                        i -
                        leftStart +
                        0.5) /
                    leftN *
                    Math.PI;

                double sine =
                    Math.Sin(x);

                window[i] =
                    Math.Sin(
                        Math.PI / 2.0 *
                        sine *
                        sine);
            }

            for (int i = leftEnd;
                 i < rightStart &&
                 i < n;
                 i++)
            {
                window[i] = 1.0;
            }

            for (int i = rightStart;
                 i < rightEnd &&
                 i < n;
                 i++)
            {
                double x =
                    (double)(
                        i -
                        rightStart +
                        0.5) /
                    rightN *
                    Math.PI +
                    Math.PI / 2.0;

                double sine =
                    Math.Sin(x);

                window[i] =
                    Math.Sin(
                        Math.PI / 2.0 *
                        sine *
                        sine);
            }

            return window;
        }

        private static void ParseIdentification(
            byte[] packet,
            out int rate,
            out int channelCount,
            out int size0,
            out int size1)
        {
            ValidateHeader(
                packet,
                1,
                "Identification");

            using (MemoryStream stream =
                   new MemoryStream(packet))
            using (BinaryReader reader =
                   new BinaryReader(stream))
            {
                reader.ReadByte();
                reader.ReadBytes(6);

                uint version =
                    reader.ReadUInt32();

                if (version != 0)
                {
                    throw new InvalidDataException(
                        "Nicht unterstützte Vorbis-Version.");
                }

                channelCount =
                    reader.ReadByte();

                uint sampleRateValue =
                    reader.ReadUInt32();

                if (sampleRateValue >
                    int.MaxValue)
                {
                    throw new InvalidDataException(
                        "Vorbis-Samplerate ist zu groß.");
                }

                rate =
                    (int)sampleRateValue;

                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();

                byte blocks =
                    reader.ReadByte();

                int exponent0 =
                    blocks & 0x0F;

                int exponent1 =
                    blocks >> 4;

                size0 =
                    1 << exponent0;

                size1 =
                    1 << exponent1;

                byte framing =
                    reader.ReadByte();

                if (framing != 1)
                {
                    throw new InvalidDataException(
                        "Vorbis-Framing-Bit fehlt.");
                }

                if (channelCount <= 0 ||
                    rate <= 0 ||
                    exponent0 < 6 ||
                    exponent1 < 6 ||
                    exponent0 > 13 ||
                    exponent1 > 13 ||
                    size1 < size0)
                {
                    throw new InvalidDataException(
                        "Ungültige Vorbis-Audiodaten.");
                }
            }
        }

        private static void ParseComment(
            byte[] packet)
        {
            ValidateHeader(
                packet,
                3,
                "Comment");

            int position = 7;

            if (packet.Length <
                position + 4)
            {
                throw new InvalidDataException(
                    "Vorbis-Comment-Header ist zu kurz.");
            }

            uint vendorLength =
                ReadUInt32(
                    packet,
                    position);

            position += 4;

            if (vendorLength >
                packet.Length - position)
            {
                throw new InvalidDataException(
                    "Vorbis-Vendorstring ist ungültig.");
            }

            position +=
                checked((int)vendorLength);

            if (position + 4 >
                packet.Length)
            {
                throw new InvalidDataException(
                    "Vorbis-Kommentaranzahl fehlt.");
            }

            uint commentCount =
                ReadUInt32(
                    packet,
                    position);

            position += 4;

            for (uint i = 0;
                 i < commentCount;
                 i++)
            {
                if (position + 4 >
                    packet.Length)
                {
                    throw new InvalidDataException(
                        "Vorbis-Kommentarlänge fehlt.");
                }

                uint length =
                    ReadUInt32(
                        packet,
                        position);

                position += 4;

                if (length >
                    packet.Length - position)
                {
                    throw new InvalidDataException(
                        "Vorbis-Kommentarfeld ist ungültig.");
                }

                position +=
                    checked((int)length);
            }

            if (position >= packet.Length)
            {
                throw new InvalidDataException(
                    "Vorbis-Comment-Framing-Bit fehlt.");
            }

            if ((packet[position] & 1) == 0)
            {
                throw new InvalidDataException(
                    "Vorbis-Comment-Framing-Bit fehlt.");
            }
        }

        private static void ParseSetup(
    byte[] packet,
    int channelCount,
    out VorbisCodebook[] parsedCodebooks,
    out VorbisFloor[] parsedFloors,
    out VorbisResidue[] parsedResidues,
    out VorbisMapping[] parsedMappings,
    out VorbisMode[] parsedModes)
        {
            if (!HasHeader(packet, 5))
            {
                throw new InvalidDataException(
                    "Ungültiger Vorbis-Setup-Header.");
            }

            if (channelCount <= 0)
            {
                throw new InvalidDataException(
                    "Ungültige Vorbis-Kanalanzahl.");
            }

            // Der erste Teil des Setup-Pakets besteht aus:
            // 1 Byte Pakettyp
            // 6 Byte "vorbis"
            // Danach beginnt der eigentliche bitweise Setup-Inhalt.
            byte[] payload =
                new byte[packet.Length - 7];

            Array.Copy(
                packet,
                7,
                payload,
                0,
                payload.Length);

            if (payload.Length < 4)
            {
                throw new InvalidDataException(
                    "Vorbis-Setup-Paket ist zu kurz.");
            }

            // Nur zur Kontrolle ausgeben.
            // Wichtig:
            // Diese Bytes werden hier NICHT aus dem BitReader gelesen.
            Logger.Info(
                "Vorbis Setup erste Bytes: " +
                payload[0].ToString("X2") + " " +
                payload[1].ToString("X2") + " " +
                payload[2].ToString("X2") + " " +
                payload[3].ToString("X2"));

            OggBitReader reader =
                new OggBitReader(payload);

            // Die ersten 8 Bit enthalten:
            // Anzahl der Codebooks minus 1.
            int codebookCount =
                (int)reader.ReadBits(8) + 1;

            if (codebookCount <= 0)
            {
                throw new InvalidDataException(
                    "Ungültige Vorbis-Codebook-Anzahl.");
            }

            Logger.Info(
                "Vorbis Codebooks: " +
                codebookCount);

            // Jetzt steht der Reader genau am Anfang
            // der ersten Codebook-Struktur.
            //
            // VorbisCodebook.Read() liest dort selbst:
            // 24 Bit Codebook-Signatur.
            parsedCodebooks =
                new VorbisCodebook[codebookCount];

            for (int i = 0;
                 i < codebookCount;
                 i++)
            {
                parsedCodebooks[i] =
                    VorbisCodebook.Read(reader);
            }

            Logger.Info(
                "Vorbis Codebooks fertig. BitsRead: " +
                reader.BitsRead);

            // Anzahl der Time-Konfigurationen.
            int timeCount =
                (int)reader.ReadBits(6) + 1;

            for (int i = 0;
                 i < timeCount;
                 i++)
            {
                uint timeType =
                    reader.ReadBits(16);

                if (timeType != 0)
                {
                    throw new InvalidDataException(
                        "Nicht unterstützte Vorbis-Time-Konfiguration.");
                }
            }

            Logger.Info(
                "Vorbis Time-Konfigurationen fertig. BitsRead: " +
                reader.BitsRead);

            // Anzahl der Floor-Konfigurationen.
            int floorCount =
                (int)reader.ReadBits(6) + 1;

            parsedFloors =
                new VorbisFloor[floorCount];

            for (int i = 0;
                 i < floorCount;
                 i++)
            {
                parsedFloors[i] =
                    VorbisFloor.ReadSetup(
                        reader,
                        parsedCodebooks);
            }

            Logger.Info(
                "Vorbis Floors fertig. Anzahl=" +
                floorCount +
                ", BitsRead=" +
                reader.BitsRead);

            Logger.Info(
                "Vorbis Residues beginnen. BitsRead=" +
                reader.BitsRead);

            // Anzahl der Residue-Konfigurationen.
            int residueCount =
                (int)reader.ReadBits(6) + 1;

            parsedResidues =
                new VorbisResidue[residueCount];

            for (int i = 0;
                 i < residueCount;
                 i++)
            {
                parsedResidues[i] =
                    VorbisResidue.Read(
                        reader,
                        parsedCodebooks);
            }

            // Anzahl der Mapping-Konfigurationen.
            int mappingCount =
                (int)reader.ReadBits(6) + 1;

            parsedMappings =
                new VorbisMapping[mappingCount];

            for (int i = 0;
                 i < mappingCount;
                 i++)
            {
                parsedMappings[i] =
                    VorbisMapping.Read(
                        reader,
                        channelCount,
                        floorCount,
                        residueCount);
            }

            // Anzahl der Modes.
            int modeCount =
                (int)reader.ReadBits(6) + 1;

            parsedModes =
                new VorbisMode[modeCount];

            for (int i = 0;
                 i < modeCount;
                 i++)
            {
                parsedModes[i] =
                    VorbisMode.Read(
                        reader,
                        mappingCount);
            }

            // Das letzte Bit des Setup-Headers
            // ist das Vorbis-Framing-Bit.
            if (reader.ReadBit() == 0)
            {
                throw new InvalidDataException(
                    "Vorbis-Setup-Framing-Bit fehlt.");
            }
        }

        private static void ValidateHeader(
            byte[] packet,
            byte expectedType,
            string name)
        {
            if (packet == null ||
                packet.Length < 7)
            {
                throw new InvalidDataException(
                    "Vorbis-" +
                    name +
                    "-Paket ist zu kurz.");
            }

            if (packet[0] != expectedType)
            {
                throw new InvalidDataException(
                    "Ungültiger Vorbis-" +
                    name +
                    "-Pakettyp.");
            }

            if (packet[1] != (byte)'v' ||
                packet[2] != (byte)'o' ||
                packet[3] != (byte)'r' ||
                packet[4] != (byte)'b' ||
                packet[5] != (byte)'i' ||
                packet[6] != (byte)'s')
            {
                throw new InvalidDataException(
                    "Ungültige Vorbis-" +
                    name +
                    "-Signatur.");
            }
        }

        private static uint ReadUInt32(
            byte[] data,
            int offset)
        {
            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data));
            }

            if (offset < 0 ||
                offset + 4 > data.Length)
            {
                throw new InvalidDataException(
                    "Unerwartetes Ende eines Vorbis-Headers.");
            }

            return
                (uint)data[offset] |
                ((uint)data[offset + 1] << 8) |
                ((uint)data[offset + 2] << 16) |
                ((uint)data[offset + 3] << 24);
        }

        private static int ILog(
            int value)
        {
            int result = 0;

            while (value > 0)
            {
                result++;
                value >>= 1;
            }

            return result;
        }

    // Prüft, ob ein Paket ein gültiger Vorbis-Header des angegebenen Typs ist.
private static bool HasHeader(
    byte[] packet,
    byte type)
        {
            return packet != null &&
                   packet.Length >= 7 &&
                   packet[0] == type &&
                   packet[1] == (byte)'v' &&
                   packet[2] == (byte)'o' &&
                   packet[3] == (byte)'r' &&
                   packet[4] == (byte)'b' &&
                   packet[5] == (byte)'i' &&
                   packet[6] == (byte)'s';
        }
    }
}