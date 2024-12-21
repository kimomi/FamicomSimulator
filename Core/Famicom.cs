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
        }

        public void LoadROM(RomInfo romInfo)
        {
            RomInfo = romInfo;
            LoadMapper(romInfo);
        }

        private void LoadMapper(RomInfo romInfo)
        {
            var mapperNum = romInfo.Header.MapperNumber;
            var sumMapperNum = romInfo.Header.SubmapperNumber;
            // todo mapper

            Cpu.LoadROM(romInfo);
            // CHR-ROM
            for (int i = 0; i < 8; i++)
            {
                Array.Copy(romInfo.DataChrRom, i * 1024, Ppu.Banks[i], 0, 1024);
            }
        }

        private void Render()
        {
            using Bitmap a = new Bitmap(256, 240);
            {
                for (int i = 0; i < 256; i++)
                {
                    for (int j = 0; j < 240; j++)
                    {
                        var c = data[i * 240 + j];
                        a.SetPixel(i, j, Color.FromArgb(c.A, c.R, c.G, c.B));
                    }
                }
                a.Save("result.png");
            }
        }

        private PaletteData[] PaletteData = new PaletteData[16];
        private PaletteData[] data = new PaletteData[256 * 240];

        public void Tick()
        {
            for (int i = 0; i < 1000; i++)
            {
                Cpu.Tick();
            }
            LogUtil.Log("Render");
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
            var now = Ppu.Banks[8];
            var bpg = Ppu.Banks[((Ppu.CtrlRgister & Ppu.PpuCtrlRegisterFlag.B) != 0) ? 4 : 0];
            for (int i = 0; i < 256 * 240; i++)
            {
                var x = i % 256;
                var y = i / 256;
                data[i] = GetPixel(x, y, now, bpg);
            }

            Render();
        }

        private PaletteData GetPixel(int x, int y, byte[] nt, byte[] bg)
        {
            // 获取所在名称表
            var id = (x >> 3) + (y >> 3) * 32;
            var name = nt[id];
            // 查找对应图样表
            var nowp0 = name * 16;
            var nowp1 = nowp0 + 8;
            // Y坐标为平面内偏移
            int offset = y & 0x7;
            var p0 = bg[nowp0 + offset];
            var p1 = bg[nowp1 + offset];
            // X坐标为字节内偏移
            var shift = (~x) & 0x7;
            var mask = 1 << shift;
            // 计算低二位
            var low = ((p0 & mask) >> shift) | ((p1 & mask) >> shift << 1);
            // 计算所在属性表
            var aid = (x >> 5) + (y >> 5) * 8;
            var attr = nt[aid + (32 * 30)];
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
                LogUtil.Log(cpuState);
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
                Tick();
            }
            LogUtil.Log("Finish Test!");
        }
    }
}
