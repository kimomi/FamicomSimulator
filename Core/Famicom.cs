using FamicomSimulator.Config;
using FamicomSimulator.Util;
using System;
using System.Drawing;
using static FamicomSimulator.Config.GlobalData;

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
        internal uint[] GraphicData = new uint[256 * 240];

        static int num = 0;

        public void Tick()
        {
            for (int i = 0; i < 10000; i++)
            {
                Cpu.Tick();
                //LogUtil.Log(Cpu.ToString());
                //if (num ++ < 100000)
                //{
                //    File.AppendAllText("log.txt", i + " - " + Cpu.ToString() + "\n");
                //}
            }

            Cpu.DoVblank();

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
            for (int i = 0; i < 256 * 240; i++)
            {
                var x = i % 256;
                var y = 240 - i / 256;
                GraphicData[i] = GetPixel(x, y, now, bpg).ToUint();
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
