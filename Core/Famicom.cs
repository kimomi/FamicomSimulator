using FamicomSimulator.Config;
using FamicomSimulator.Util;
using OpenTK.Platform;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Drawing;
using static FamicomSimulator.Config.GlobalData;
using static FamicomSimulator.Core.Ppu;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FamicomSimulator.Core
{
    public class Famicom
    {
        internal Cpu Cpu;
        internal Ppu Ppu;
        public RomInfo? RomInfo;

        public byte[][] PrgBanks = new byte[0x10000 >> 13][];
        public byte[] SaveMemory = new byte[8 * 1024];
        public byte[] VideoMemory = new byte[2 * 1024];
        public byte[] VideoMemoryEx = new byte[2 * 1024];
        public byte[] MainMemory = new byte[2 * 1024];

        internal ushort ButtonIndex1;
        internal ushort ButtonIndex2;
        internal ushort ButtonIndexMask;
        internal byte[] ButtonStates = new byte[16];

        public Famicom()
        {
            Cpu = new Cpu()
            { 
                Famicom = this
            };
            Ppu = new Ppu()
            {
                Famicom = this
            };
            for (int i = 0; i < PrgBanks.Length; i++)
            {
                PrgBanks[i] = new byte[8 * 1024];
            }
            PrgBanks[0] = MainMemory;
            PrgBanks[3] = SaveMemory;
        }

        public void LoadROM(RomInfo romInfo)
        {
            RomInfo = romInfo;
            LoadMapper();
            Reset();
        }

        private void LoadMapper()
        {
            if (RomInfo == null) return;

            var mapperNum = RomInfo.Header.MapperNumber;
            var sumMapperNum = RomInfo.Header.SubmapperNumber;
        }

        private void Reset()
        {
            if (RomInfo == null) return;

            // todo mapper
            Cpu.LoadROM(RomInfo);
            Ppu.LoadROM(RomInfo);
        }

        private PaletteData[] PaletteData = new PaletteData[16];
        public const int WIDTH = 256;
        public const int HEIGHT = 256;
        internal uint[] GraphicData = new uint[WIDTH * HEIGHT];

        public void Tick()
        {
            for (int i = 0; i < 10000; i++)
            {
                Cpu.Tick();
            }

            Cpu.DoVblank();

            KeyInput();

            // 生成调色板颜色
            for (int i = 0; i < 16; i++)
            {
                PaletteData[i] = GlobalData.Palettes[Ppu.Spindexes[i]];
            }
            PaletteData[4 * 1] = PaletteData[0];
            PaletteData[4 * 2] = PaletteData[0];
            PaletteData[4 * 3] = PaletteData[0];

            // 背景
            var now = 8 * 1024;
            var bpg = ((Ppu.CtrlRgister & Ppu.PpuCtrlRegisterFlag.B) != 0) ? (4 * 1024) : 0;
            for (int i = 0; i < WIDTH * HEIGHT; i++)
            {
                var x = i % WIDTH;
                var y = i / WIDTH;
                GraphicData[i] = GetPixel(x, y, now, bpg);
            }

            SubRnder();
        }

        private void SubRnder()
        {
            // 生成调色板颜色
            for (int i = 0; i < 16; i++)
            {
                PaletteData[i] = GlobalData.Palettes[Ppu.Spindexes[i]];
            }
            PaletteData[4 * 1] = PaletteData[0];
            PaletteData[4 * 2] = PaletteData[0];
            PaletteData[4 * 3] = PaletteData[0];

            // 设置为背景色
            for (int i = 0; i < WIDTH * HEIGHT; i++)
            {
                GraphicData[i] = PaletteData[0];
            }

            // 精灵
            var spp = ((Ppu.CtrlRgister & PpuCtrlRegisterFlag.S) != 0) ? (4 * 1024) : 0;
            for (int i = 63; i >= 0; i--)
            {
                var ptr = i * 4;
                byte yy = Ppu.Sprites[ptr + 0];
                byte ii = Ppu.Sprites[ptr + 1];
                byte aa = Ppu.Sprites[ptr + 2];
                byte xx = Ppu.Sprites[ptr + 3];
                if (yy >= 0xEF) continue;

                var nowp0 = spp + ii * 16;
                var nowp1 = nowp0 + 8;
                byte high = (byte)((aa & 3) << 2);
                // 水平翻转
                if ((aa & 0x40) != 0)
                {
                    for (var j = 0; j < 8; ++j)
                    {
                        ExpandLine8r(Ppu.Banks[nowp0 + j], Ppu.Banks[nowp1 + j], high, xx + (yy + j + 1) * 256);
                    }
                }
                else
                {
                    for (var j = 0; j < 8; ++j)
                    {
                        ExpandLine8(Ppu.Banks[nowp0 + j], Ppu.Banks[nowp1 + j], high, xx + (yy + j + 1) * 256);
                    }
                }
            }
        }

        private void ExpandLine8(byte p0, byte p1, byte high, int index)
        {
            // 0 - D7
            byte low0 = (byte)(((p0 & 0x80) >> 7) | ((p1 & (byte)0x80) >> 6));
            PaletteData[high] = GraphicData[index + 0];
            GraphicData[index + 0] = PaletteData[high | low0];
            // 1 - D6
            byte low1 = (byte)(((p0 & 0x40) >> 6) | ((p1 & (byte)0x40) >> 5));
            PaletteData[high] = GraphicData[index + 1];
            GraphicData[index + 1] = PaletteData[high | low1];
            // 2 - D5
            byte low2 = (byte)(((p0 & (byte)0x20) >> 5) | ((p1 & (byte)0x20) >> 4));
            PaletteData[high] = GraphicData[index + 2];
            GraphicData[index + 2] = PaletteData[high | low2];
            // 3 - D4
            byte low3 = (byte)(((p0 & (byte)0x10) >> 4) | ((p1 & (byte)0x10) >> 3));
            PaletteData[high] = GraphicData[index + 3];
            GraphicData[index + 3] = PaletteData[high | low3];
            // 4 - D3
            byte low4 = (byte)(((p0 & (byte)0x08) >> 3) | ((p1 & (byte)0x08) >> 2));
            PaletteData[high] = GraphicData[index + 4];
            GraphicData[index + 4] = PaletteData[high | low4];
            // 5 - D2
            byte low5 = (byte)(((p0 & (byte)0x04) >> 2) | ((p1 & (byte)0x04) >> 1));
            PaletteData[high] = GraphicData[index + 5];
            GraphicData[index + 5] = PaletteData[high | low5];
            // 6 - D1
            byte low6 = (byte)(((p0 & (byte)0x02) >> 1) | ((p1 & (byte)0x02) >> 0));
            PaletteData[high] = GraphicData[index + 6];
            GraphicData[index + 6] = PaletteData[high | low6];
            // 7 - D0
            byte low7 = (byte)(((p0 & (byte)0x01) >> 0) | ((p1 & (byte)0x01) << 1));
            PaletteData[high] = GraphicData[index + 7];
            GraphicData[index + 7] = PaletteData[high | low7];
        }

        private void ExpandLine8r(byte p0, byte p1, byte high, int index)
        {
            // 7 - D7
            byte low0 = (byte)(((p0 & 0x80) >> 7) | ((p1 & (byte)0x80) >> 6));
            PaletteData[high] = GraphicData[index + 7];
            GraphicData[index + 7] = PaletteData[high | low0];
            // 6 - D6
            byte low1 = (byte)(((p0 & 0x40) >> 6) | ((p1 & (byte)0x40) >> 5));
            PaletteData[high] = GraphicData[index + 6];
            GraphicData[index + 6] = PaletteData[high | low1];
            // 5 - D5
            byte low2 = (byte)(((p0 & (byte)0x20) >> 5) | ((p1 & (byte)0x20) >> 4));
            PaletteData[high] = GraphicData[index + 5];
            GraphicData[index + 5] = PaletteData[high | low2];
            // 4 - D4
            byte low3 = (byte)(((p0 & (byte)0x10) >> 4) | ((p1 & (byte)0x10) >> 3));
            PaletteData[high] = GraphicData[index + 4];
            GraphicData[index + 4] = PaletteData[high | low3];
            // 3 - D3
            byte low4 = (byte)(((p0 & (byte)0x08) >> 3) | ((p1 & (byte)0x08) >> 2));
            PaletteData[high] = GraphicData[index + 3];
            GraphicData[index + 3] = PaletteData[high | low4];
            // 2 - D2
            byte low5 = (byte)(((p0 & (byte)0x04) >> 2) | ((p1 & (byte)0x04) >> 1));
            PaletteData[high] = GraphicData[index + 2];
            GraphicData[index + 2] = PaletteData[high | low5];
            // 1 - D1
            byte low6 = (byte)(((p0 & (byte)0x02) >> 1) | ((p1 & (byte)0x02) >> 0));
            PaletteData[high] = GraphicData[index + 1];
            GraphicData[index + 1] = PaletteData[high | low6];
            // 0 - D0
            byte low7 = (byte)(((p0 & (byte)0x01) >> 0) | ((p1 & (byte)0x01) << 1));
            PaletteData[high] = GraphicData[index + 0];
            GraphicData[index + 0] = PaletteData[high | low7];
        }

        private readonly List<Keys> CtrlKeyList = new List<Keys>()
        {
            Keys.J, Keys.K, Keys.U, Keys.I, Keys.W, Keys.S, Keys.A, Keys.D,
            Keys.KeyPad2, Keys.KeyPad3, Keys.KeyPad5, Keys.KeyPad6, Keys.Up, Keys.Down, Keys.Left, Keys.Right,
        };

        private void KeyInput()
        {
            if (WindowUtil.GameWindow == null)
            {
                return;
            }

            for (int i = 0; i < CtrlKeyList.Count; i++)
            {
                ButtonStates[i] = (byte)(WindowUtil.GameWindow.IsKeyPressed(CtrlKeyList[i]) ? 1 : 0);
            }
        }

        private PaletteData GetPixel(int x, int y, int nt, int bg)
        {
            // 获取所在名称表
            var id = (x >> 3) + (y >> 3) * 32;
            var name = Ppu.Banks[nt + id];
            // 查找对应图样表
            var nowp0 = name * 16;
            var nowp1 = nowp0 + 8;
            // Y坐标为平面内偏移
            int offset = y & 0x7;
            var p0 = Ppu.Banks[nowp0 + offset];
            var p1 = Ppu.Banks[nowp1 + offset];
            // X坐标为字节内偏移
            var shift = (~x) & 0x7;
            var mask = 1 << shift;
            // 计算低二位
            var low = ((p0 & mask) >> shift) | ((p1 & mask) >> shift << 1);
            // 计算所在属性表
            var aid = (x >> 5) + (y >> 5) * 8;
            var attr = Ppu.Banks[nt + aid + (32 * 30)];
            // 获取属性表内位偏移
            var aoffset = ((x & 0x10) >> 3) | ((y & 0x10) >> 2);
            // 计算高两位
            var high = (attr & (3 << aoffset)) >> aoffset << 2;
            // 合并作为颜色
            var index = high | low;

            return PaletteData[index];
        }

        public void RunTest()
        {
            Cpu.register.PC = 0xC000;
            var testFileLines = File.ReadAllLines("../../../Others/nestest.log");
            for (int i = 0; i < testFileLines.Length; i++)
            {
                var cpuState = Cpu.ToString();
                //LogUtil.Log(cpuState);
                var myAsm = cpuState.Substring(7, 19).Trim();
                var refAsm = testFileLines[i].Substring(16, 19).Split('@')[0].Trim();

                var myRState = cpuState.Substring(30).Trim();
                var refRState = testFileLines[i].Substring(testFileLines[i].IndexOf("A:"), 25).Trim();
                if ((myAsm != refAsm &&
                    !(refAsm.Contains('=') && refAsm.Split('=')[0].Trim() == myAsm) &&
                    !(myAsm.Contains("ISC") && refAsm.Contains("ISB")))
                    || (myRState != refRState))
                {
                    LogUtil.Error($"line:{i} myAsm:={myAsm}=, refAsm:={refAsm}=, myR:={myRState}=, refR:={refRState}=");
                    LogUtil.Error("Test Failed!");
                    break;
                }
                Cpu.Tick();
            }
            LogUtil.Log("Finish Test!");
        }
    }
}
