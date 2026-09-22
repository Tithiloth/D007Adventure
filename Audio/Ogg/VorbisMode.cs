using System;

namespace D007Adventure.Audio.Ogg
{
    // Beschreibt einen Vorbis-Mode.
    public sealed class VorbisMode
    {
        public bool BlockFlag { get; private set; }
        public int Mapping { get; private set; }

        public static VorbisMode Read(OggBitReader reader, int mappingCount)
        {
            VorbisMode mode = new VorbisMode();
            mode.BlockFlag = reader.ReadBits(1) != 0;

            if (reader.ReadBits(16) != 0)
                throw new InvalidOperationException("Vorbis-Window-Type ist nicht 0.");

            if (reader.ReadBits(16) != 0)
                throw new InvalidOperationException("Vorbis-Transform-Type ist nicht 0.");

            mode.Mapping = (int)reader.ReadBits(8);
            if (mode.Mapping >= mappingCount)
                throw new InvalidOperationException("Vorbis-Mode verweist auf ein unbekanntes Mapping.");

            return mode;
        }
    }
}
