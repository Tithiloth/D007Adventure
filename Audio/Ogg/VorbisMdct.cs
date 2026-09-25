using System;

namespace D007Adventure.Audio.Ogg
{
    // Direkte inverse MDCT. Sie ist bewusst einfach gehalten und benötigt keine externe Bibliothek.
    public sealed class VorbisMdct
    {
        private readonly int blockSize;
        private readonly int coefficientCount;
        private readonly double[,] cosine;

        public VorbisMdct(int blockSize)
        {
            if (blockSize < 64 || blockSize > 8192 || (blockSize & (blockSize - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(blockSize));

            this.blockSize = blockSize;
            coefficientCount = blockSize / 2;
            cosine = new double[blockSize, coefficientCount];

            for (int n = 0; n < blockSize; n++)
            {
                for (int k = 0; k < coefficientCount; k++)
                {
                    double angle = Math.PI / coefficientCount *
                        (n + 0.5 + coefficientCount / 2.0) * (k + 0.5);
                    cosine[n, k] = Math.Cos(angle);
                }
            }
        }

        public float[] Inverse(float[] input)
        {
            if (input == null || input.Length != coefficientCount)
                throw new ArgumentException("Die MDCT benötigt N/2 Koeffizienten.");

            float[] output = new float[blockSize];
            double scale = 2.0 / coefficientCount;

            for (int n = 0; n < blockSize; n++)
            {
                double sum = 0.0;
                for (int k = 0; k < coefficientCount; k++)
                    sum += input[k] * cosine[n, k];

                output[n] = (float)(sum * scale);
            }

            return output;
        }

        public static double Window(int index, int size)
        {
            double x = (index + 0.5) / size * Math.PI;
            return Math.Sin(0.5 * Math.PI * Math.Sin(x) * Math.Sin(x));
        }
    }
}
