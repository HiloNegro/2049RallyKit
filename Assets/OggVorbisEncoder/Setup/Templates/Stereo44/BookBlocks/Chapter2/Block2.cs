﻿namespace OggVorbisEncoder.Setup.Templates.Stereo44.BookBlocks.Chapter2
{
    public class Block2 : IStaticBookBlock
    {
        public IStaticCodeBook[][] Books { get; } =
        {
            new IStaticCodeBook[] {null},
            new IStaticCodeBook[] {null, null, new Page1()},
            new IStaticCodeBook[] {null, null, new Page2()},
            new IStaticCodeBook[] {null, null, new Page3()},
            new IStaticCodeBook[] {null, null, new Page4()},
            new IStaticCodeBook[] {null, null, new Page5()},
            new IStaticCodeBook[] {null, null, new Page6()},
            new IStaticCodeBook[] {new Page7_0(), new Page7_1()},
            new IStaticCodeBook[] {new Page8_0(), new Page8_1()},
            new IStaticCodeBook[] {new Page9_0(), new Page9_1(), new Page9_2()}
        };
    }
}