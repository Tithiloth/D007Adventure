using System;
using System.Collections.Generic;

namespace D007Adventure.Audio.Ogg
{
    // Entpackt ein Vorbis-Codebook inklusive Huffman-Baum und VQ-Werten.
    public sealed class VorbisCodebook
    {
        private readonly Dictionary<int, Dictionary<uint, int>> decodeTree;
        private float[,] values;

        public int Dimensions { get; private set; }
        public int Entries { get; private set; }
        public int LookupType { get; private set; }

        private VorbisCodebook()
        {
            decodeTree =
                new Dictionary<int, Dictionary<uint, int>>();
        }

        public static VorbisCodebook Read(OggBitReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            VorbisCodebook book =
                new VorbisCodebook();

            Logger.Info(
                "Codebook-Start. BitsRead vorher: " +
                reader.BitsRead);

            uint signature =
                reader.ReadBits(24);

            Logger.Info(
                "Codebook-Signatur gelesen: 0x" +
                signature.ToString("X6") +
                " | BitsRead: " +
                reader.BitsRead);

            if (signature != 0x564342u)
            {
                throw new InvalidOperationException(
                    "Ungültige Vorbis-Codebook-Signatur.");
            }

            book.Dimensions =
                (int)reader.ReadBits(16);

            book.Entries =
                (int)reader.ReadBits(24);

            if (book.Dimensions <= 0 ||
                book.Entries <= 0)
            {
                throw new InvalidOperationException(
                    "Ungültige Vorbis-Codebook-Größe.");
            }

            // Die Vorbis-Spezifikation begrenzt die Kombination
            // aus Dimension und Entry-Anzahl.
            if (ILog(book.Dimensions) +
                ILog(book.Entries) > 24)
            {
                throw new InvalidOperationException(
                    "Vorbis-Codebook besitzt ungültige Dimensionen.");
            }

            Logger.Info(
                "Codebook: Dimensionen=" +
                book.Dimensions +
                ", Entries=" +
                book.Entries +
                ", BitsRead=" +
                reader.BitsRead);

            int[] lengths =
                new int[book.Entries];

            bool ordered =
                reader.ReadBits(1) != 0;

            if (ordered)
            {
                ReadOrderedLengths(
                    reader,
                    book.Entries,
                    lengths);
            }
            else
            {
                ReadUnorderedLengths(
                    reader,
                    book.Entries,
                    lengths);
            }

            BuildHuffmanTree(
                book,
                lengths);

            book.LookupType =
                (int)reader.ReadBits(4);

            Logger.Info(
                "Codebook LookupType=" +
                book.LookupType +
                " | BitsRead=" +
                reader.BitsRead);

            if (book.LookupType < 0 ||
                book.LookupType > 2)
            {
                throw new InvalidOperationException(
                    "Ungültiger Vorbis-Codebook-Lookup-Typ.");
            }

            if (book.LookupType == 0)
            {
                return book;
            }

            float minimumValue =
                UnpackFloat32(
                    reader.ReadBits(32));

            float deltaValue =
                UnpackFloat32(
                    reader.ReadBits(32));

            int valueBits =
                (int)reader.ReadBits(4) + 1;

            bool sequenceP =
                reader.ReadBits(1) != 0;

            if (valueBits < 1 ||
                valueBits > 32)
            {
                throw new InvalidOperationException(
                    "Ungültige Vorbis-Codebook-Wortbreite.");
            }

            int lookupValues;

            if (book.LookupType == 1)
            {
                lookupValues =
                    Lookup1Values(
                        book.Entries,
                        book.Dimensions);
            }
            else
            {
                long total =
                    (long)book.Entries *
                    book.Dimensions;

                if (total > int.MaxValue)
                {
                    throw new InvalidOperationException(
                        "Vorbis-Codebook ist zu groß.");
                }

                lookupValues =
                    (int)total;
            }

            if (lookupValues <= 0)
            {
                throw new InvalidOperationException(
                    "Ungültige Anzahl an Codebook-Quantisierungswerten.");
            }

            Logger.Info(
                "Codebook VQ: LookupValues=" +
                lookupValues +
                ", ValueBits=" +
                valueBits +
                ", Sequence=" +
                sequenceP +
                " | BitsRead=" +
                reader.BitsRead);

            book.values =
                new float[
                    book.Entries,
                    book.Dimensions];

            // Lookup Type 1:
            // Die gelesenen Quantisierungswerte werden
            // algorithmisch auf die Vektoren verteilt.
            if (book.LookupType == 1)
            {
                float[] multiplicands =
                    new float[lookupValues];

                for (int i = 0;
                    i < lookupValues;
                    i++)
                {
                    uint raw =
                        reader.ReadBits(valueBits);

                    multiplicands[i] =
                        raw;
                }

                for (int entry = 0;
                    entry < book.Entries;
                    entry++)
                {
                    float last = 0.0f;
                    long indexDivisor = 1;

                    for (int dimension = 0;
                        dimension < book.Dimensions;
                        dimension++)
                    {
                        int index =
                            (int)(
                                (entry /
                                 indexDivisor) %
                                lookupValues);

                        float value =
                            minimumValue +
                            multiplicands[index] *
                            deltaValue +
                            last;

                        book.values[
                            entry,
                            dimension] =
                            value;

                        if (sequenceP)
                        {
                            last = value;
                        }

                        indexDivisor *=
                            lookupValues;
                    }
                }
            }
            else
            {
                // Lookup Type 2:
                // Für jeden Entry und jede Dimension
                // wird ein eigener Quantisierungswert gelesen.
                for (int entry = 0;
                    entry < book.Entries;
                    entry++)
                {
                    float last = 0.0f;

                    for (int dimension = 0;
                        dimension < book.Dimensions;
                        dimension++)
                    {
                        uint raw =
                            reader.ReadBits(valueBits);

                        float value =
                            minimumValue +
                            raw * deltaValue +
                            last;

                        book.values[
                            entry,
                            dimension] =
                            value;

                        if (sequenceP)
                        {
                            last = value;
                        }
                    }
                }
            }

            Logger.Info(
                "Codebook fertig. BitsRead: " +
                reader.BitsRead);

            return book;
        }

        private static void ReadUnorderedLengths(
            OggBitReader reader,
            int entries,
            int[] lengths)
        {
            bool sparse =
                reader.ReadBits(1) != 0;

            for (int i = 0;
                i < entries;
                i++)
            {
                if (sparse)
                {
                    bool used =
                        reader.ReadBits(1) != 0;

                    if (!used)
                    {
                        lengths[i] = 0;
                        continue;
                    }
                }

                lengths[i] =
                    (int)reader.ReadBits(5) + 1;

                if (lengths[i] > 32)
                {
                    throw new InvalidOperationException(
                        "Vorbis-Codeword ist länger als 32 Bit.");
                }
            }
        }

        private static void ReadOrderedLengths(
            OggBitReader reader,
            int entries,
            int[] lengths)
        {
            int currentEntry = 0;

            int currentLength =
                (int)reader.ReadBits(5) + 1;

            while (currentEntry < entries)
            {
                int remaining =
                    entries - currentEntry;

                int bitCount =
                    ILog(remaining);

                int number =
                    (int)reader.ReadBits(bitCount);

                if (number > remaining)
                {
                    throw new InvalidOperationException(
                        "Ungültige geordnete Vorbis-Codebook-Längen.");
                }

                if (number > 0)
                {
                    if (currentLength > 32)
                    {
                        throw new InvalidOperationException(
                            "Vorbis-Codeword ist länger als 32 Bit.");
                    }

                    for (int i = 0;
                        i < number;
                        i++)
                    {
                        lengths[currentEntry++] =
                            currentLength;
                    }
                }

                currentLength++;

                if (currentLength > 32 &&
                    currentEntry < entries)
                {
                    throw new InvalidOperationException(
                        "Vorbis-Codebook überschreitet 32 Bit.");
                }
            }
        }

        private static void BuildHuffmanTree(
            VorbisCodebook book,
            int[] lengths)
        {
            uint[] bitLengthCount =
                new uint[33];

            for (int i = 0;
                i < lengths.Length;
                i++)
            {
                int length =
                    lengths[i];

                if (length <= 0)
                    continue;

                if (length > 32)
                {
                    throw new InvalidOperationException(
                        "Vorbis-Codeword ist länger als 32 Bit.");
                }

                bitLengthCount[length]++;
            }

            uint[] firstCode =
                new uint[33];

            for (int bits = 1;
                bits <= 32;
                bits++)
            {
                firstCode[bits] =
                    (firstCode[bits - 1] +
                     bitLengthCount[bits - 1]) << 1;
            }

            uint[] nextCode =
                new uint[33];

            Array.Copy(
                firstCode,
                nextCode,
                firstCode.Length);

            for (int entry = 0;
                entry < lengths.Length;
                entry++)
            {
                int length =
                    lengths[entry];

                if (length <= 0)
                    continue;

                uint canonicalCode =
                    nextCode[length]++;

                uint reversedCode =
                    ReverseBits(
                        canonicalCode,
                        length);

                Dictionary<uint, int> level;

                if (!book.decodeTree.TryGetValue(
                    length,
                    out level))
                {
                    level =
                        new Dictionary<uint, int>();

                    book.decodeTree[
                        length] =
                        level;
                }

                if (level.ContainsKey(reversedCode))
                {
                    throw new InvalidOperationException(
                        "Doppeltes Vorbis-Huffman-Codewort.");
                }

                level[
                    reversedCode] =
                    entry;
            }
        }

        public int ReadScalar(
            OggBitReader reader)
        {
            uint code = 0;

            for (int length = 1;
                length <= 32;
                length++)
            {
                code |=
                    (uint)reader.ReadBit()
                    << (length - 1);

                Dictionary<uint, int> level;

                if (!decodeTree.TryGetValue(
                    length,
                    out level))
                {
                    continue;
                }

                int entry;

                if (level.TryGetValue(
                    code,
                    out entry))
                {
                    return entry;
                }
            }

            throw new InvalidOperationException(
                "Kein gültiges Vorbis-Codewort gefunden.");
        }

        public float[] DecodeVector(
            OggBitReader reader)
        {
            int entry =
                ReadScalar(reader);

            if (entry < 0 ||
                entry >= Entries)
            {
                throw new InvalidOperationException(
                    "Vorbis-Codebook-Entry liegt außerhalb des gültigen Bereichs.");
            }

            if (values == null)
            {
                throw new InvalidOperationException(
                    "Für dieses Vorbis-Codebook existieren keine VQ-Werte.");
            }

            float[] vector =
                new float[Dimensions];

            for (int i = 0;
                i < Dimensions;
                i++)
            {
                vector[i] =
                    values[
                        entry,
                        i];
            }

            return vector;
        }

        private static uint ReverseBits(
            uint value,
            int count)
        {
            uint result = 0;

            for (int i = 0;
                i < count;
                i++)
            {
                result |=
                    ((value >> i) & 1u)
                    << (count - i - 1);
            }

            return result;
        }

        private static float UnpackFloat32(
            uint value)
        {
            // Vorbis verwendet eine eigene 32-Bit-Float-Darstellung.
            // Die unteren 21 Bit bilden eine vorzeichenbehaftete Mantisse.
            int mantissa =
                (int)(value & 0x1FFFFFu);

            if ((mantissa & 0x100000) != 0)
            {
                mantissa -= 0x200000;
            }

            int exponent =
                (int)((value >> 21) & 0x3FFu);

            bool negative =
                (value & 0x80000000u) != 0;

            if (negative)
            {
                mantissa = -mantissa;
            }

            return
                mantissa *
                (float)Math.Pow(
                    2.0,
                    exponent - 788);
        }

        private static int Lookup1Values(
            int entries,
            int dimensions)
        {
            if (entries <= 0 ||
                dimensions <= 0)
            {
                return 0;
            }

            int low = 1;
            int high = entries;

            while (low <= high)
            {
                int middle =
                    low + ((high - low) / 2);

                bool fits =
                    PowerIsLessOrEqual(
                        middle,
                        dimensions,
                        entries);

                if (fits)
                {
                    low =
                        middle + 1;
                }
                else
                {
                    high =
                        middle - 1;
                }
            }

            return high;
        }

        private static bool PowerIsLessOrEqual(
            int value,
            int exponent,
            int limit)
        {
            if (exponent == 0)
                return true;

            long result = 1;

            for (int i = 0;
                i < exponent;
                i++)
            {
                if (result >
                    limit / value)
                {
                    return false;
                }

                result *= value;
            }

            return result <= limit;
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
    }
}