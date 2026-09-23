using System;
using System.IO;
using System.Text;

namespace D007Adventure.Audio.Ogg
{
    // Repräsentiert eine einzelne OGG-Page.
    public sealed class OggPage
    {
        public byte Version { get; private set; }
        public byte HeaderType { get; private set; }
        public long GranulePosition { get; private set; }
        public int BitstreamSerialNumber { get; private set; }
        public int PageSequenceNumber { get; private set; }
        public uint Checksum { get; private set; }
        public byte[] SegmentTable { get; private set; }
        public byte[] Data { get; private set; }

        public bool IsContinued { get { return (HeaderType & 0x01) != 0; } }
        public bool IsFirstPage { get { return (HeaderType & 0x02) != 0; } }
        public bool IsLastPage { get { return (HeaderType & 0x04) != 0; } }

        private OggPage() { }

        public static OggPage Read(BinaryReader reader)
        {
            byte[] signatureBytes = reader.ReadBytes(4);
            if (signatureBytes.Length != 4)
                throw new EndOfStreamException();

            if (Encoding.ASCII.GetString(signatureBytes) != "OggS")
                throw new InvalidDataException("Ungültige OGG-Signatur.");

            OggPage page = new OggPage();
            page.Version = reader.ReadByte();
            if (page.Version != 0)
                throw new InvalidDataException("Nicht unterstützte OGG-Version.");

            page.HeaderType = reader.ReadByte();
            page.GranulePosition = reader.ReadInt64();
            page.BitstreamSerialNumber = reader.ReadInt32();
            page.PageSequenceNumber = reader.ReadInt32();
            page.Checksum = reader.ReadUInt32();

            int segmentCount = reader.ReadByte();
            page.SegmentTable = reader.ReadBytes(segmentCount);
            if (page.SegmentTable.Length != segmentCount)
                throw new EndOfStreamException("Unvollständige OGG-Segmenttabelle.");

            int length = 0;
            for (int i = 0; i < page.SegmentTable.Length; i++)
                length += page.SegmentTable[i];

            page.Data = reader.ReadBytes(length);
            if (page.Data.Length != length)
                throw new EndOfStreamException("Unvollständige OGG-Pagedaten.");

            return page;
        }

        public bool VerifyChecksum()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(new byte[] { (byte)'O', (byte)'g', (byte)'g', (byte)'S' });
                writer.Write(Version);
                writer.Write(HeaderType);
                writer.Write(GranulePosition);
                writer.Write(BitstreamSerialNumber);
                writer.Write(PageSequenceNumber);
                writer.Write((uint)0);
                writer.Write((byte)SegmentTable.Length);
                writer.Write(SegmentTable);
                writer.Write(Data);
                writer.Flush();
                return CalculateCrc(stream.ToArray()) == Checksum;
            }
        }

        private static uint CalculateCrc(byte[] data)
        {
            uint crc = 0;

            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (uint)data[i] << 24;

                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x80000000u) != 0)
                        crc = (crc << 1) ^ 0x04C11DB7u;
                    else
                        crc <<= 1;
                }
            }

            return crc;
        }
    }
}
