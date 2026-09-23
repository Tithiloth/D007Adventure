using System;

namespace D007Adventure.Audio.Ogg
{
    // Liest einen Vorbis-Bitstream bitweise in LSb-first-Reihenfolge.
    public sealed class OggBitReader
    {
        private readonly byte[] data;
        private int bytePosition;
        private int bitPosition;

        public int BitsRead { get; private set; }

        public bool EndOfStream
        {
            get { return bytePosition >= data.Length; }
        }

        public OggBitReader(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            this.data = data;
        }

        public int ReadBit()
        {
            if (bytePosition >= data.Length)
                throw new InvalidOperationException("Ende des Vorbis-Bitstreams erreicht.");

            int value = (data[bytePosition] >> bitPosition) & 1;
            bitPosition++;

            if (bitPosition == 8)
            {
                bitPosition = 0;
                bytePosition++;
            }

            BitsRead++;
            return value;
        }

        public uint ReadBits(int count)
        {
            if (count < 0 || count > 32)
                throw new ArgumentOutOfRangeException(nameof(count));

            uint value = 0;
            for (int i = 0; i < count; i++)
                value |= (uint)ReadBit() << i;

            return value;
        }

        public void SkipBits(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (int i = 0; i < count; i++)
                ReadBit();
        }
    }
}
