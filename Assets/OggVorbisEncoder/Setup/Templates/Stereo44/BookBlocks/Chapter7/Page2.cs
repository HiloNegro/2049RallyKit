﻿namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter7
{
    public class Page2 : IStaticCodeBook
    {
        public int Dimensions { get; } = 4;

        public byte[] LengthList { get; } = {
            3, 5, 5, 8, 8, 0, 5, 5, 8, 8, 0, 5, 5, 8, 8, 0,
            7, 7, 9, 9, 0, 0, 0, 9, 9, 5, 7, 7, 9, 9, 0, 8,
            8, 10, 10, 0, 8, 7, 10, 9, 0, 10, 10, 11, 11, 0, 0, 0,
            11, 11, 5, 7, 7, 9, 9, 0, 8, 8, 10, 10, 0, 7, 8, 9,
            10, 0, 10, 10, 11, 11, 0, 0, 0, 11, 11, 8, 9, 9, 11, 10,
            0, 11, 11, 12, 12, 0, 11, 10, 12, 12, 0, 13, 14, 14, 14, 0,
            0, 0, 14, 13, 8, 9, 9, 10, 11, 0, 11, 11, 12, 12, 0, 10,
            11, 12, 12, 0, 13, 13, 14, 14, 0, 0, 0, 13, 14, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 5, 8, 7, 11, 10, 0, 7, 7, 10, 10,
            0, 7, 7, 10, 10, 0, 9, 9, 11, 10, 0, 0, 0, 11, 11, 5,
            7, 8, 10, 11, 0, 7, 7, 10, 10, 0, 7, 7, 10, 10, 0, 9,
            9, 10, 11, 0, 0, 0, 11, 11, 8, 10, 9, 12, 12, 0, 10, 10,
            12, 12, 0, 10, 10, 12, 12, 0, 12, 12, 13, 13, 0, 0, 0, 13,
            13, 8, 9, 10, 12, 12, 0, 10, 10, 12, 12, 0, 10, 10, 11, 12,
            0, 12, 12, 13, 13, 0, 0, 0, 13, 13, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 5, 8, 8, 11, 11, 0, 7, 7, 10, 10, 0, 7, 7,
            10, 10, 0, 9, 9, 10, 11, 0, 0, 0, 11, 10, 5, 8, 8, 10,
            11, 0, 7, 7, 10, 10, 0, 7, 7, 10, 10, 0, 9, 9, 11, 10,
            0, 0, 0, 10, 11, 9, 10, 10, 12, 12, 0, 10, 10, 12, 12, 0,
            10, 10, 12, 12, 0, 12, 13, 13, 13, 0, 0, 0, 13, 12, 9, 10,
            10, 12, 12, 0, 10, 10, 12, 12, 0, 10, 10, 12, 12, 0, 13, 12,
            13, 13, 0, 0, 0, 12, 13, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            7, 10, 10, 14, 13, 0, 9, 9, 12, 12, 0, 9, 9, 12, 12, 0,
            10, 10, 12, 12, 0, 0, 0, 12, 12, 7, 10, 10, 13, 14, 0, 9,
            9, 12, 13, 0, 9, 9, 12, 12, 0, 10, 10, 12, 12, 0, 0, 0,
            12, 12, 9, 11, 11, 14, 13, 0, 11, 10, 13, 12, 0, 11, 11, 13,
            13, 0, 12, 12, 13, 13, 0, 0, 0, 13, 13, 9, 11, 11, 13, 14,
            0, 10, 11, 12, 13, 0, 11, 11, 13, 13, 0, 12, 12, 13, 13, 0,
            0, 0, 13, 13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9,
            11, 11, 14, 14, 0, 10, 11, 13, 13, 0, 11, 10, 13, 13, 0, 12,
            12, 13, 13, 0, 0, 0, 13, 12, 9, 11, 11, 14, 14, 0, 11, 10,
            13, 13, 0, 10, 11, 13, 13, 0, 12, 12, 14, 13, 0, 0, 0, 13,
            13
        };

        public CodeBookMapType MapType { get; } = CodeBookMapType.Implicit;
        public int QuantMin { get; } = -533725184;
        public int QuantDelta { get; } = 1611661312;
        public int Quant { get; } = 3;
        public int QuantSequenceP { get; } = 0;

        public int[] QuantList { get; } = {
            2,
            1,
            3,
            0,
            4
        };
    }
}