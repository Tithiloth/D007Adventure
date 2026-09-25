using System;

namespace D007Adventure.Audio.Ogg
{
    // Dekodiert Vorbis Residue 0, 1 und 2.
    public sealed class VorbisResidue
    {
        private readonly int type;
        private readonly int begin;
        private readonly int end;
        private readonly int partitionSize;
        private readonly int classifications;
        private readonly int classbook;
        private readonly int[,] books;

        private VorbisResidue(
            int type,
            int begin,
            int end,
            int partitionSize,
            int classifications,
            int classbook,
            int[,] books)
        {
            this.type = type;
            this.begin = begin;
            this.end = end;
            this.partitionSize = partitionSize;
            this.classifications = classifications;
            this.classbook = classbook;
            this.books = books;
        }

        public static VorbisResidue Read(
            OggBitReader reader,
            VorbisCodebook[] codebooks)
        {
            int startBits = reader.BitsRead;

            int type = (int)reader.ReadBits(16);
            int begin = (int)reader.ReadBits(24);
            int end = (int)reader.ReadBits(24);
            int partitionSize = (int)reader.ReadBits(24) + 1;
            int classifications = (int)reader.ReadBits(6) + 1;
            int classbook = (int)reader.ReadBits(8);

            Logger.Info(
                "Residue: Type=" + type +
                ", Begin=" + begin +
                ", End=" + end +
                ", PartitionSize=" + partitionSize +
                ", Classifications=" + classifications +
                ", Classbook=" + classbook +
                ", BitsRead=" + reader.BitsRead +
                ", StartBits=" + startBits);

            if (type > 2)
            {
                throw new InvalidOperationException(
                    "Ungültiger Vorbis-Residue-Typ: " + type);
            }

            if (classbook < 0 || classbook >= codebooks.Length)
            {
                throw new InvalidOperationException(
                    "Residue-Classbook ist ungültig: " +
                    classbook +
                    " bei " +
                    codebooks.Length +
                    " Codebooks.");
            }

            int[,] books = new int[classifications, 8];

            for (int i = 0; i < classifications; i++)
            {
                int before = reader.BitsRead;

                int low = (int)reader.ReadBits(3);

                int flag = (int)reader.ReadBits(1);

                int high = 0;

                if (flag != 0)
                    high = (int)reader.ReadBits(5);

                int cascade = low | (high << 3);

                Logger.Info(
                    "Residue-Klasse " + i +
                    ": Low=" + low +
                    ", Flag=" + flag +
                    ", High=" + high +
                    ", Cascade=" + cascade +
                    ", BitsRead=" + reader.BitsRead +
                    ", BitsUsed=" + (reader.BitsRead - before));

                for (int pass = 0; pass < 8; pass++)
                {
                    books[i, pass] = -1;

                    if ((cascade & (1 << pass)) == 0)
                        continue;

                    int beforeBook = reader.BitsRead;

                    int bookNumber = (int)reader.ReadBits(8);

                    books[i, pass] = bookNumber;

                    Logger.Info(
                        "Residue-Klasse " + i +
                        ", Pass " + pass +
                        ": Codebook=" + bookNumber +
                        ", BitsRead=" + reader.BitsRead +
                        ", BitsUsed=" + (reader.BitsRead - beforeBook));

                    if (bookNumber >= codebooks.Length)
                    {
                        throw new InvalidOperationException(
                            "Residue-VQ-Codebook ist ungültig: " +
                            bookNumber +
                            " bei " +
                            codebooks.Length +
                            " Codebooks. " +
                            "Residue-Klasse=" +
                            i +
                            ", Pass=" +
                            pass +
                            ", Cascade=" +
                            cascade +
                            ", BitsRead=" +
                            reader.BitsRead);
                    }
                }
            }

            Logger.Info(
                "Residue komplett gelesen. BitsRead=" +
                reader.BitsRead +
                ", BitsUsed=" +
                (reader.BitsRead - startBits));

            return new VorbisResidue(
                type,
                begin,
                end,
                partitionSize,
                classifications,
                classbook,
                books);
        }

        public void Decode(
            OggBitReader reader,
            float[][] vectors,
            bool[] doNotDecode,
            VorbisCodebook[] codebooks,
            int blockSize)
        {
            int channels = vectors.Length;

            if (channels == 0)
                return;

            if (type == 2)
            {
                DecodeType2(
                    reader,
                    vectors,
                    doNotDecode,
                    codebooks,
                    blockSize);

                return;
            }

            int actualSize = blockSize / 2;

            int residueBegin = Math.Min(begin, actualSize);
            int residueEnd = Math.Min(end, actualSize);

            int count = residueEnd - residueBegin;

            if (count <= 0)
                return;

            int partitionsToRead = count / partitionSize;

            if (partitionsToRead <= 0)
                return;

            int classWords = codebooks[classbook].Dimensions;

            if (classWords <= 0)
            {
                throw new InvalidOperationException(
                    "Residue-Classbook besitzt keine gültigen Dimensionen.");
            }

            int[,] classificationsForChannel =
                new int[channels, partitionsToRead];

            for (int pass = 0; pass < 8; pass++)
            {
                int partitionCount = 0;

                while (partitionCount < partitionsToRead)
                {
                    if (pass == 0)
                    {
                        for (int channel = 0; channel < channels; channel++)
                        {
                            if (doNotDecode[channel])
                                continue;

                            int temp =
                                codebooks[classbook].ReadScalar(reader);

                            for (
                                int i = classWords - 1;
                                i >= 0;
                                i--)
                            {
                                int p = partitionCount + i;

                                if (p < 0 || p >= partitionsToRead)
                                    continue;

                                classificationsForChannel[channel, p] =
                                    temp % classifications;

                                temp /= classifications;
                            }
                        }
                    }

                    int limit =
                        Math.Min(
                            classWords,
                            partitionsToRead - partitionCount);

                    for (int i = 0; i < limit; i++)
                    {
                        for (int channel = 0; channel < channels; channel++)
                        {
                            if (doNotDecode[channel])
                                continue;

                            int classNumber =
                                classificationsForChannel[
                                    channel,
                                    partitionCount];

                            int bookNumber =
                                books[classNumber, pass];

                            if (bookNumber < 0)
                                continue;

                            VorbisCodebook book =
                                codebooks[bookNumber];

                            int offset =
                                residueBegin +
                                partitionCount * partitionSize;

                            DecodePartition(
                                reader,
                                vectors[channel],
                                offset,
                                partitionSize,
                                book,
                                type == 0);
                        }

                        partitionCount++;
                    }
                }
            }
        }

        private void DecodeType2(
            OggBitReader reader,
            float[][] vectors,
            bool[] doNotDecode,
            VorbisCodebook[] codebooks,
            int blockSize)
        {
            int channels = vectors.Length;

            int actualSize =
                (blockSize / 2) * channels;

            int residueBegin =
                Math.Min(begin, actualSize);

            int residueEnd =
                Math.Min(end, actualSize);

            int count =
                residueEnd - residueBegin;

            if (count <= 0)
                return;

            bool any = false;

            for (int i = 0; i < doNotDecode.Length; i++)
            {
                if (!doNotDecode[i])
                {
                    any = true;
                    break;
                }
            }

            if (!any)
                return;

            int partitions =
                count / partitionSize;

            int classWords =
                codebooks[classbook].Dimensions;

            if (classWords <= 0)
            {
                throw new InvalidOperationException(
                    "Residue-Classbook besitzt keine gültigen Dimensionen.");
            }

            int[,] classification =
                new int[1, partitions];

            float[] flat =
                new float[channels * (blockSize / 2)];

            for (int pass = 0; pass < 8; pass++)
            {
                int partitionCount = 0;

                while (partitionCount < partitions)
                {
                    if (pass == 0)
                    {
                        int temp =
                            codebooks[classbook].ReadScalar(reader);

                        for (int i = classWords - 1; i >= 0; i--)
                        {
                            int p =
                                partitionCount + i;

                            if (p < 0 || p >= partitions)
                                continue;

                            classification[0, p] =
                                temp % classifications;

                            temp /= classifications;
                        }
                    }

                    int limit =
                        Math.Min(
                            classWords,
                            partitions - partitionCount);

                    for (int i = 0; i < limit; i++)
                    {
                        int classNumber =
                            classification[0, partitionCount];

                        int bookNumber =
                            books[classNumber, pass];

                        if (bookNumber >= 0)
                        {
                            VorbisCodebook book =
                                codebooks[bookNumber];

                            int offset =
                                residueBegin +
                                partitionCount * partitionSize;

                            DecodePartition(
                                reader,
                                flat,
                                offset,
                                partitionSize,
                                book,
                                false);
                        }

                        partitionCount++;
                    }
                }
            }

            for (int i = 0; i < blockSize / 2; i++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    vectors[channel][i] +=
                        flat[i * channels + channel];
                }
            }
        }

        private static void DecodePartition(
            OggBitReader reader,
            float[] vector,
            int offset,
            int partitionSize,
            VorbisCodebook book,
            bool interleaved)
        {
            if (offset >= vector.Length)
                return;

            int size =
                Math.Min(
                    partitionSize,
                    vector.Length - offset);

            if (book.Dimensions <= 0)
            {
                throw new InvalidOperationException(
                    "Residue-Codebook hat keine Dimension.");
            }

            if (interleaved)
            {
                int step =
                    partitionSize / book.Dimensions;

                if (step <= 0)
                {
                    throw new InvalidOperationException(
                        "Residue-Partition ist kleiner als die Codebook-Dimension.");
                }

                for (int i = 0; i < step; i++)
                {
                    float[] value =
                        book.DecodeVector(reader);

                    for (int j = 0; j < book.Dimensions; j++)
                    {
                        int index =
                            offset +
                            i +
                            j * step;

                        if (index < vector.Length)
                        {
                            vector[index] += value[j];
                        }
                    }
                }
            }
            else
            {
                int position = 0;

                while (position < size)
                {
                    float[] value =
                        book.DecodeVector(reader);

                    for (
                        int j = 0;
                        j < book.Dimensions &&
                        position < size;
                        j++)
                    {
                        vector[offset + position++] += value[j];
                    }
                }
            }
        }
    }
}