namespace MusicGQL.Features.ServerLibrary.Audio;

public static class Mp3HeaderReader
{
    // Very small MP3 frame header parser to estimate constant bitrate from first valid frame
    // Supports common bitrates; returns null if header not found
    public static int? TryReadBitrateKbps(Stream stream)
    {
        try
        {
            if (!stream.CanSeek)
                return null;
            long originalPos = stream.Position;
            stream.Position = 0;

            Span<byte> buffer = stackalloc byte[4];

            while (stream.Read(buffer) == 4)
            {
                // Frame sync 11 bits: 0xFFE
                if (buffer[0] == 0xFF && (buffer[1] & 0xE0) == 0xE0)
                {
                    int versionBits = (buffer[1] >> 3) & 0x03; // MPEG Audio version ID
                    int layerBits = (buffer[1] >> 1) & 0x03; // Layer description
                    int bitrateIndex = (buffer[2] >> 4) & 0x0F;
                    int samplingRateIndex = (buffer[2] >> 2) & 0x03;

                    if (
                        layerBits == 0x01
                        && samplingRateIndex != 0x03
                        && bitrateIndex != 0x0F
                        && bitrateIndex != 0x00
                    )
                    {
                        // Layer III
                        int[,] bitrateTable = versionBits switch
                        {
                            0b11 => new int[,] // MPEG1, Layer III
                            {
                                {
                                    0,
                                    32,
                                    40,
                                    48,
                                    56,
                                    64,
                                    80,
                                    96,
                                    112,
                                    128,
                                    160,
                                    192,
                                    224,
                                    256,
                                    320,
                                },
                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                            },
                            0b10 or 0b00 => new int[,] // MPEG2/2.5, Layer III
                            {
                                { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160 },
                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                            },
                            _ => new int[,]
                            {
                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                            },
                        };

                        int kbps = bitrateTable[0, bitrateIndex];
                        if (kbps > 0)
                        {
                            stream.Position = originalPos;
                            return kbps;
                        }
                    }
                }

                // Slide window by 1 byte
                stream.Position = stream.Position - 3;
            }

            stream.Position = originalPos;
            return null;
        }
        catch
        {
            return null;
        }
    }
}
