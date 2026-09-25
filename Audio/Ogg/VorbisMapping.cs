using System;

namespace D007Adventure.Audio.Ogg
{
    // Beschreibt ein Vorbis-Mapping inklusive Kanal-Kopplung.
    public sealed class VorbisMapping
    {
        public int Submaps { get; private set; }
        public int[] Mux { get; private set; }
        public int[] SubmapFloor { get; private set; }
        public int[] SubmapResidue { get; private set; }
        public int CouplingSteps { get; private set; }
        public int[] Magnitude { get; private set; }
        public int[] Angle { get; private set; }

        public static VorbisMapping Read(OggBitReader reader, int channels,
            int floorCount, int residueCount)
        {
            int type = (int)reader.ReadBits(16);
            if (type != 0)
                throw new InvalidOperationException("Nur Vorbis-Mapping Typ 0 ist gültig.");

            VorbisMapping mapping = new VorbisMapping();
            bool submapFlag = reader.ReadBits(1) != 0;
            mapping.Submaps = submapFlag ? (int)reader.ReadBits(4) + 1 : 1;

            bool couplingFlag = reader.ReadBits(1) != 0;
            if (couplingFlag)
            {
                mapping.CouplingSteps = (int)reader.ReadBits(8) + 1;
                int bits = ILog(channels - 1);
                mapping.Magnitude = new int[mapping.CouplingSteps];
                mapping.Angle = new int[mapping.CouplingSteps];

                for (int i = 0; i < mapping.CouplingSteps; i++)
                {
                    mapping.Magnitude[i] = (int)reader.ReadBits(bits);
                    mapping.Angle[i] = (int)reader.ReadBits(bits);

                    if (mapping.Magnitude[i] >= channels || mapping.Angle[i] >= channels ||
                        mapping.Magnitude[i] == mapping.Angle[i])
                        throw new InvalidOperationException("Ungültige Vorbis-Kopplung.");
                }
            }
            else
            {
                mapping.CouplingSteps = 0;
                mapping.Magnitude = new int[0];
                mapping.Angle = new int[0];
            }

            if (reader.ReadBits(2) != 0)
                throw new InvalidOperationException("Vorbis-Mapping-Reserviertbits sind gesetzt.");

            mapping.Mux = new int[channels];
            if (submapFlag)
            {
                for (int i = 0; i < channels; i++)
                {
                    mapping.Mux[i] = (int)reader.ReadBits(4);
                    if (mapping.Mux[i] >= mapping.Submaps)
                        throw new InvalidOperationException("Vorbis-Mux verweist auf ein unbekanntes Submap.");
                }
            }

            mapping.SubmapFloor = new int[mapping.Submaps];
            mapping.SubmapResidue = new int[mapping.Submaps];

            for (int i = 0; i < mapping.Submaps; i++)
            {
                if (reader.ReadBits(8) != 0)
                    throw new InvalidOperationException("Vorbis-Time-Configuration ist nicht Typ 0.");

                mapping.SubmapFloor[i] = (int)reader.ReadBits(8);
                mapping.SubmapResidue[i] = (int)reader.ReadBits(8);

                if (mapping.SubmapFloor[i] >= floorCount || mapping.SubmapResidue[i] >= residueCount)
                    throw new InvalidOperationException("Vorbis-Mapping verweist auf eine unbekannte Instanz.");
            }

            return mapping;
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
