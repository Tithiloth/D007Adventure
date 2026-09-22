using System;
using System.Collections.Generic;

namespace D007Adventure.Audio.Ogg
{
    // Dekodiert Vorbis Floor 0 und Floor 1.
    public sealed class VorbisFloor
    {
        private readonly int type;
        private Floor0 floor0;
        private Floor1 floor1;

        private VorbisFloor(int type)
        {
            this.type = type;
        }

        public bool IsUnused { get; private set; }

        public static VorbisFloor ReadSetup(
            OggBitReader reader,
            VorbisCodebook[] books)
        {
            int startBits = reader.BitsRead;

            int type = (int)reader.ReadBits(16);

            Logger.Info(
                "Floor-Setup Start: Type=" +
                type +
                ", StartBits=" +
                startBits);

            VorbisFloor floor =
                new VorbisFloor(type);

            if (type == 0)
            {
                floor.floor0 =
                    Floor0.Read(
                        reader,
                        books);
            }
            else if (type == 1)
            {
                floor.floor1 =
                    Floor1.Read(
                        reader,
                        books);
            }
            else
            {
                throw new InvalidOperationException(
                    "Nicht unterstützter Vorbis-Floor-Typ: " +
                    type);
            }

            Logger.Info(
                "Floor-Setup Ende: Type=" +
                type +
                ", BitsUsed=" +
                (reader.BitsRead - startBits) +
                ", EndBits=" +
                reader.BitsRead);

            return floor;
        }
        public float[] Decode(OggBitReader reader, int n, VorbisCodebook[] books)
        {
            if (type == 0)
            {
                float[] result = floor0.Decode(reader, n, books);
                IsUnused = floor0.LastUnused;
                return result;
            }

            float[] floor1Result = floor1.Decode(reader, n, books);
            IsUnused = floor1.LastUnused;
            return floor1Result;
        }

        private sealed class Floor0
        {
            private readonly int order;
            private readonly int rate;
            private readonly int barkMapSize;
            private readonly int amplitudeBits;
            private readonly int amplitudeOffset;
            private readonly int numberOfBooks;
            private readonly int[] bookList;

            public bool LastUnused { get; private set; }

            private Floor0(int order, int rate, int barkMapSize, int amplitudeBits,
                int amplitudeOffset, int numberOfBooks, int[] bookList)
            {
                this.order = order;
                this.rate = rate;
                this.barkMapSize = barkMapSize;
                this.amplitudeBits = amplitudeBits;
                this.amplitudeOffset = amplitudeOffset;
                this.numberOfBooks = numberOfBooks;
                this.bookList = bookList;
            }

            public static Floor0 Read(OggBitReader reader, VorbisCodebook[] books)
            {
                int order = (int)reader.ReadBits(8);
                int rate = (int)reader.ReadBits(16);
                int barkMapSize = (int)reader.ReadBits(16);
                int amplitudeBits = (int)reader.ReadBits(6);
                int amplitudeOffset = (int)reader.ReadBits(8);
                int numberOfBooks = (int)reader.ReadBits(4) + 1;
                int[] list = new int[numberOfBooks];

                for (int i = 0; i < numberOfBooks; i++)
                {
                    list[i] = (int)reader.ReadBits(8);
                    if (list[i] < 0 || list[i] >= books.Length)
                        throw new InvalidOperationException("Floor0 verweist auf ein unbekanntes Codebook.");
                }

                return new Floor0(order, rate, barkMapSize, amplitudeBits,
                    amplitudeOffset, numberOfBooks, list);
            }

            public float[] Decode(OggBitReader reader, int n, VorbisCodebook[] books)
            {
                LastUnused = false;
                float[] output = new float[n];
                int amplitude = (int)reader.ReadBits(amplitudeBits);

                if (amplitude == 0)
                {
                    LastUnused = true;
                    return output;
                }

                int bookBits = ILog(numberOfBooks);
                int bookNumber = bookBits == 0 ? 0 : (int)reader.ReadBits(bookBits);
                if (bookNumber >= numberOfBooks)
                    throw new InvalidOperationException("Ungültige Floor0-Codebooknummer.");

                List<float> coefficients = new List<float>();
                float last = 0.0f;

                while (coefficients.Count < order)
                {
                    float[] vector = books[bookList[bookNumber]].DecodeVector(reader);
                    for (int i = 0; i < vector.Length; i++)
                    {
                        vector[i] += last;
                        last = vector[i];
                        coefficients.Add(vector[i]);
                        if (coefficients.Count == order)
                            break;
                    }
                }

                double[] lsp = new double[order];
                for (int i = 0; i < order; i++)
                    lsp[i] = coefficients[i] / 65536.0 * Math.PI;

                double amplitudeValue = amplitude / ((1 << amplitudeBits) - 1.0);
                amplitudeValue = amplitudeValue * amplitudeOffset;

                for (int i = 0; i < n; i++)
                {
                    int bark = BarkMap(i, n);
                    double omega = Math.PI * bark / barkMapSize;
                    double sum = 0.0;

                    for (int j = 0; j < order; j++)
                    {
                        double angle = omega * (j + 1);
                        sum += Math.Cos(angle) * lsp[j];
                    }

                    double p = 1.0;
                    double q = 1.0;
                    for (int j = 0; j < order; j += 2)
                    {
                        p *= 1.0 - 2.0 * Math.Cos(omega) * Math.Cos(lsp[j]) + Math.Cos(lsp[j]) * Math.Cos(lsp[j]);
                        if (j + 1 < order)
                            q *= 1.0 - 2.0 * Math.Cos(omega) * Math.Cos(lsp[j + 1]) + Math.Cos(lsp[j + 1]) * Math.Cos(lsp[j + 1]);
                    }

                    double denominator = Math.Sqrt(Math.Max(1e-20, p * q));
                    double gain = Math.Pow(10.0, (amplitudeValue - denominator * 10.0) / 20.0);
                    output[i] = (float)Math.Max(0.0, Math.Min(1.0, gain));
                }

                return output;
            }

            private int BarkMap(int i, int n)
            {
                double frequency = (double)rate * i / (2.0 * n);
                double bark = 13.1 * Math.Atan(0.00074 * frequency)
                    + 2.24 * Math.Atan(0.0000000185 * frequency * frequency)
                    + 0.0001 * frequency;

                double maxBark = 13.1 * Math.Atan(0.00074 * (rate * 0.5))
                    + 2.24 * Math.Atan(0.0000000185 * (rate * 0.5) * (rate * 0.5))
                    + 0.0001 * (rate * 0.5);

                int result = (int)Math.Floor(bark * barkMapSize / Math.Max(1e-12, maxBark));
                return Math.Max(0, Math.Min(barkMapSize - 1, result));
            }
        }

        private sealed class Floor1
        {
            private readonly int partitions;
            private readonly int[] partitionClass;
            private readonly int[] classDimensions;
            private readonly int[] classSubclasses;
            private readonly int[] classMasterbooks;
            private readonly int[][] subclassBooks;
            private readonly int multiplier;
            private readonly int[] xList;

            public bool LastUnused { get; private set; }

            private Floor1(int partitions, int[] partitionClass, int[] classDimensions,
                int[] classSubclasses, int[] classMasterbooks, int[][] subclassBooks,
                int multiplier, int[] xList)
            {
                this.partitions = partitions;
                this.partitionClass = partitionClass;
                this.classDimensions = classDimensions;
                this.classSubclasses = classSubclasses;
                this.classMasterbooks = classMasterbooks;
                this.subclassBooks = subclassBooks;
                this.multiplier = multiplier;
                this.xList = xList;
            }

            public static Floor1 Read(
    OggBitReader reader,
    VorbisCodebook[] books)
            {
                int startBits = reader.BitsRead;

                int partitions =
                    (int)reader.ReadBits(5);

                Logger.Info(
                    "Floor1: Partitions=" +
                    partitions +
                    ", BitsRead=" +
                    reader.BitsRead);

                int[] partitionClass =
                    new int[partitions];

                int maxClass = -1;

                for (int i = 0;
                     i < partitions;
                     i++)
                {
                    partitionClass[i] =
                        (int)reader.ReadBits(4);

                    maxClass =
                        Math.Max(
                            maxClass,
                            partitionClass[i]);
                }

                int classes =
                    maxClass + 1;

                Logger.Info(
                    "Floor1: Classes=" +
                    classes +
                    ", MaxClass=" +
                    maxClass +
                    ", BitsRead=" +
                    reader.BitsRead);

                int[] dimensions =
                    new int[classes];

                int[] subclasses =
                    new int[classes];

                int[] masterbooks =
                    new int[classes];

                int[][] subbooks =
                    new int[classes][];

                for (int i = 0;
                     i < classes;
                     i++)
                {
                    dimensions[i] =
                        (int)reader.ReadBits(3) + 1;

                    subclasses[i] =
                        (int)reader.ReadBits(2);

                    if (subclasses[i] != 0)
                    {
                        masterbooks[i] =
                            (int)reader.ReadBits(8);

                        if (masterbooks[i] < 0 ||
                            masterbooks[i] >= books.Length)
                        {
                            throw new InvalidOperationException(
                                "Floor1 Masterbook ist ungültig: " +
                                masterbooks[i] +
                                " bei " +
                                books.Length +
                                " Codebooks.");
                        }
                    }
                    else
                    {
                        masterbooks[i] = -1;
                    }

                    int count =
                        1 << subclasses[i];

                    subbooks[i] =
                        new int[count];

                    for (int j = 0;
                         j < count;
                         j++)
                    {
                        int book =
                            (int)reader.ReadBits(8) - 1;

                        if (book < -1 ||
                            book >= books.Length)
                        {
                            throw new InvalidOperationException(
                                "Floor1 Subclass-Codebook ist ungültig: " +
                                book +
                                " bei " +
                                books.Length +
                                " Codebooks.");
                        }

                        subbooks[i][j] =
                            book;
                    }

                    Logger.Info(
                        "Floor1 Klasse " +
                        i +
                        ": Dimensions=" +
                        dimensions[i] +
                        ", Subclasses=" +
                        subclasses[i] +
                        ", Masterbook=" +
                        masterbooks[i] +
                        ", Subbooks=" +
                        count +
                        ", BitsRead=" +
                        reader.BitsRead);
                }

                int multiplier =
                    (int)reader.ReadBits(2) + 1;

                int rangeBits =
                    (int)reader.ReadBits(4);

                if (rangeBits < 1 ||
                    rangeBits > 16)
                {
                    throw new InvalidOperationException(
                        "Ungültige Floor1-Rangebits: " +
                        rangeBits);
                }

                Logger.Info(
                    "Floor1: Multiplier=" +
                    multiplier +
                    ", RangeBits=" +
                    rangeBits +
                    ", BitsRead=" +
                    reader.BitsRead);

                List<int> x =
                    new List<int>();

                x.Add(0);
                x.Add(1 << rangeBits);

                for (int i = 0;
                     i < partitions;
                     i++)
                {
                    int c =
                        partitionClass[i];

                    for (int j = 0;
                         j < dimensions[c];
                         j++)
                    {
                        if (x.Count >= 65)
                        {
                            throw new InvalidOperationException(
                                "Floor1 enthält zu viele X-Werte.");
                        }

                        int value =
                            (int)reader.ReadBits(rangeBits);

                        if (value < 0 ||
                            value >= (1 << rangeBits))
                        {
                            throw new InvalidOperationException(
                                "Ungültiger Floor1-X-Wert: " +
                                value);
                        }

                        x.Add(value);
                    }
                }

                for (int i = 0;
                     i < x.Count;
                     i++)
                {
                    for (int j = i + 1;
                         j < x.Count;
                         j++)
                    {
                        if (x[i] == x[j])
                        {
                            throw new InvalidOperationException(
                                "Floor1-X-Werte müssen eindeutig sein.");
                        }
                    }
                }

                Logger.Info(
                    "Floor1 Setup fertig: XCount=" +
                    x.Count +
                    ", BitsUsed=" +
                    (reader.BitsRead - startBits) +
                    ", EndBits=" +
                    reader.BitsRead);

                return new Floor1(
                    partitions,
                    partitionClass,
                    dimensions,
                    subclasses,
                    masterbooks,
                    subbooks,
                    multiplier,
                    x.ToArray());
            }

            public float[] Decode(OggBitReader reader, int n, VorbisCodebook[] books)
            {
                LastUnused = false;
                float[] output = new float[n];

                if (reader.ReadBit() == 0)
                {
                    LastUnused = true;
                    return output;
                }

                int range = new int[] { 256, 128, 86, 64 }[multiplier - 1];
                int[] y = new int[xList.Length];
                bool[] step2 = new bool[xList.Length];

                int yBits = ILog(range - 1);
                y[0] = (int)reader.ReadBits(yBits);
                y[1] = (int)reader.ReadBits(yBits);
                int offset = 2;

                for (int i = 0; i < partitions; i++)
                {
                    int c = partitionClass[i];
                    int dim = classDimensions[c];
                    int subBits = classSubclasses[c];
                    int csub = (1 << subBits) - 1;
                    int cval = 0;

                    if (subBits > 0)
                        cval = books[classMasterbooks[c]].ReadScalar(reader);

                    for (int j = 0; j < dim && offset < y.Length; j++)
                    {
                        int book = subclassBooks[c][cval & csub];
                        cval >>= subBits;
                        y[offset++] = book >= 0 ? books[book].ReadScalar(reader) : 0;
                    }
                }

                int[] finalY = new int[y.Length];
                finalY[0] = y[0];
                finalY[1] = y[1];
                step2[0] = true;
                step2[1] = true;

                for (int i = 2; i < y.Length; i++)
                {
                    int low = LowNeighbor(xList, i);
                    int high = HighNeighbor(xList, i);
                    int predicted = RenderPoint(xList[low], finalY[low], xList[high], finalY[high], xList[i]);
                    int highRoom = range - predicted;
                    int lowRoom = predicted;
                    int room = 2 * Math.Min(lowRoom, highRoom);
                    int value = y[i];

                    if (value != 0)
                    {
                        step2[low] = true;
                        step2[high] = true;
                        step2[i] = true;

                        if (value >= room)
                        {
                            if (highRoom > lowRoom)
                                finalY[i] = value - lowRoom + predicted;
                            else
                                finalY[i] = predicted - value + highRoom - 1;
                        }
                        else
                        {
                            if ((value & 1) != 0)
                                finalY[i] = predicted - ((value + 1) / 2);
                            else
                                finalY[i] = predicted + value / 2;
                        }
                    }
                    else
                    {
                        step2[i] = false;
                        finalY[i] = predicted;
                    }

                    finalY[i] = Math.Max(0, Math.Min(range - 1, finalY[i]));
                }

                List<int> order = new List<int>();
                for (int i = 0; i < xList.Length; i++)
                    order.Add(i);
                order.Sort((a, b) => xList[a].CompareTo(xList[b]));

                int hx = 0;
                int lx = 0;
                int ly = finalY[order[0]] * multiplier;
                int hy = ly;

                for (int k = 1; k < order.Count; k++)
                {
                    int i = order[k];
                    if (!step2[i])
                        continue;

                    hy = finalY[i] * multiplier;
                    hx = xList[i];
                    RenderLine(lx, ly, hx, hy, output, n);
                    lx = hx;
                    ly = hy;
                }

                if (hx < n)
                    RenderLine(hx, hy, n, hy, output, n);

                for (int i = 0; i < output.Length; i++)
                {
                    int index = (int)Math.Max(0, Math.Min(255, output[i]));
                    output[i] = InverseDb(index);
                }

                return output;
            }

            private static int LowNeighbor(int[] x, int index)
            {
                int best = 0;
                for (int i = 0; i < index; i++)
                    if (x[i] < x[index] && x[i] > x[best])
                        best = i;
                return best;
            }

            private static int HighNeighbor(int[] x, int index)
            {
                int best = 1;
                for (int i = 0; i < index; i++)
                    if (x[i] > x[index] && x[i] < x[best])
                        best = i;
                return best;
            }

            private static int RenderPoint(int x0, int y0, int x1, int y1, int x)
            {
                int dy = y1 - y0;
                int adx = x1 - x0;
                int ady = Math.Abs(dy);
                int off = (ady * (x - x0)) / adx;
                return dy < 0 ? y0 - off : y0 + off;
            }

            private static void RenderLine(int x0, int y0, int x1, int y1, float[] target, int length)
            {
                if (x1 <= x0)
                    return;

                int dy = y1 - y0;
                int adx = x1 - x0;
                int ady = Math.Abs(dy);
                int baseValue = dy / adx;
                int y = y0;
                int error = 0;
                int sy = dy < 0 ? baseValue - 1 : baseValue + 1;
                ady -= Math.Abs(baseValue) * adx;

                if (x0 >= 0 && x0 < length)
                    target[x0] = y;

                for (int x = x0 + 1; x < x1 && x < length; x++)
                {
                    error += ady;
                    if (error >= adx)
                    {
                        error -= adx;
                        y += sy;
                    }
                    else
                    {
                        y += baseValue;
                    }

                    if (x >= 0)
                        target[x] = y;
                }
            }

            private static float InverseDb(int value)
            {
                return (float)Math.Pow(10.0, ((double)value - 255.0) * 140.0 / (255.0 * 20.0));
            }
        }

        private static int ILog(int value)
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
