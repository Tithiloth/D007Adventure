using System;
using System.Collections.Generic;
using System.IO;

namespace D007Adventure.Audio.Ogg
{
    // Liest einen OGG-Container und setzt die Daten aus mehreren Pages
    // zu vollständigen OGG-Paketen zusammen.
    public sealed class OggStreamReader : IDisposable
    {
        private readonly BinaryReader reader;
        private readonly Queue<OggPacket> packets;
        private readonly List<byte> packetBuffer;

        private int serialNumber;
        private bool serialNumberSet;
        private bool endOfStream;
        private bool disposed;

        public OggStreamReader(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            reader = new BinaryReader(stream);
            packets = new Queue<OggPacket>();
            packetBuffer = new List<byte>();
        }

        // Liefert das nächste vollständige OGG-Paket.
        public OggPacket ReadPacket()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(OggStreamReader));

            while (packets.Count == 0)
            {
                if (endOfStream)
                {
                    return new OggPacket(
                        new byte[0],
                        -1,
                        true);
                }

                ReadNextPage();
            }

            return packets.Dequeue();
        }

        private void ReadNextPage()
        {
            OggPage page;

            try
            {
                page = OggPage.Read(reader);
            }
            catch (EndOfStreamException)
            {
                if (packetBuffer.Count > 0)
                {
                    throw new InvalidDataException(
                        "OGG-Datei endet mitten in einem Paket.");
                }

                endOfStream = true;
                return;
            }

            if (!serialNumberSet)
            {
                serialNumber = page.BitstreamSerialNumber;
                serialNumberSet = true;
            }

            // Mehrere logische OGG-Streams können in einer Datei enthalten sein.
            // Für unseren Vorbis-Decoder verwenden wir den ersten Stream.
            if (page.BitstreamSerialNumber != serialNumber)
                return;

            bool continued = page.IsContinued;

            if (continued && packetBuffer.Count == 0)
            {
                throw new InvalidDataException(
                    "OGG-Page markiert ein fortgesetztes Paket, " +
                    "aber es existiert kein vorheriges Paket.");
            }

            if (!continued && packetBuffer.Count > 0)
            {
                throw new InvalidDataException(
                    "OGG-Paket wurde nicht ordnungsgemäß abgeschlossen.");
            }

            int dataOffset = 0;

            for (int i = 0; i < page.SegmentTable.Length; i++)
            {
                int segmentLength = page.SegmentTable[i];

                if (dataOffset + segmentLength > page.Data.Length)
                {
                    throw new InvalidDataException(
                        "OGG-Segment überschreitet die Page-Daten.");
                }

                for (int j = 0; j < segmentLength; j++)
                {
                    packetBuffer.Add(page.Data[dataOffset + j]);
                }

                dataOffset += segmentLength;

                // Ein Segment mit 255 Bytes bedeutet:
                // Das aktuelle Paket geht auf der nächsten Page weiter.
                if (segmentLength < 255)
                {
                    byte[] packetData = packetBuffer.ToArray();
                    packetBuffer.Clear();

                    packets.Enqueue(
                        new OggPacket(
                            packetData,
                            page.GranulePosition,
                            false));
                }
            }

            if (dataOffset != page.Data.Length)
            {
                throw new InvalidDataException(
                    "Nicht alle OGG-Pagedaten wurden verarbeitet.");
            }

            if (page.IsLastPage)
            {
                if (packetBuffer.Count > 0)
                {
                    throw new InvalidDataException(
                        "OGG-Datei endet mit einem unvollständigen Paket.");
                }

                endOfStream = true;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            reader.Dispose();
            packets.Clear();
            packetBuffer.Clear();
        }
    }

    // Enthält ein vollständiges OGG-Paket.
    // Enthält ein vollständiges OGG-Paket.
    public sealed class OggPacket
    {
        public byte[] Data { get; private set; }
        public long GranulePosition { get; private set; }
        public bool IsEndOfStream { get; private set; }

        public OggPacket(
            byte[] data,
            long granulePosition,
            bool isEndOfStream)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            GranulePosition = granulePosition;
            IsEndOfStream = isEndOfStream;
        }

        // Ermöglicht die Verwendung von OggPacket an Stellen,
        // an denen der bestehende Decoder ein byte[] erwartet.
        public static implicit operator byte[](OggPacket packet)
        {
            if (packet == null)
                return null;

            return packet.Data;
        }
    }
}