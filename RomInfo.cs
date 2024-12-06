using FamicomSimulator.Config;
using System.Runtime.Intrinsics.Arm;

namespace FamicomSimulator
{
    public class RomInfo
    {
        public Header Header; // ROM头部信息
        public byte[]? TrainerArea; // 部分游戏才有，比如为了兼容早期的ROM卡带或者模拟器，有些游戏在ROM文件的头部有一个512字节的Trainer区域
        public byte[] DataPrgRom; // 程序只读存储器
        public byte[] DataChrRom; // 角色只读存储器
        public byte[]? MiscellaneousRomArea; // 其它ROM区域，不一定有

        public RomInfo(byte[] data)
        {
            if (data.Length < 16)
            {
                throw new InvalidDataException("Invalid NES Data Format! Read header failed!");
            }
            var currentByteCount = 0;
            Header = new Header(data);
            currentByteCount += 16;

            if (Header.HasTrainerArea) // 不一定有这部分区域
            {
                if (data.Length < currentByteCount + 512)
                {
                    throw new InvalidDataException("Invalid NES Data Format! Read trainer aread failed!");
                }
                TrainerArea = new byte[512];
                Array.Copy(data, currentByteCount, TrainerArea, 0, 512);
                currentByteCount += 512;
            }

            if (data.Length < currentByteCount + Header.PrgRomSize)
            {
                throw new InvalidDataException("Invalid NES Data Format! Read PRG-ROM failed!");
            }
            DataPrgRom = new byte[Header.PrgRomSize];
            Array.Copy(data, currentByteCount, DataPrgRom, 0, Header.PrgRomSize);
            currentByteCount += (int)Header.PrgRomSize;

            if (data.Length < currentByteCount + Header.ChrRomSize)
            {
                throw new InvalidDataException("Invalid NES Data Format! Read CHR-ROM failed!");
            }
            DataChrRom = new byte[Header.ChrRomSize];
            Array.Copy(data, currentByteCount, DataChrRom, 0, Header.ChrRomSize);
            currentByteCount += (int)Header.ChrRomSize;

            if (currentByteCount < data.Length) // 不一定有这部分区域
            {
                MiscellaneousRomArea = new byte[data.Length - currentByteCount];
                Array.Copy(data, currentByteCount, MiscellaneousRomArea, 0, MiscellaneousRomArea.Length);
            }
        }
    }

    public class Header
    {
        public bool HardwiredNametaleLayoot; // false 竖直（水平镜像），true 水平（数值镜像）
        public bool BatterAndOtherNonVolatileMemory; // false 无电池，true 有电池
        public bool HasTrainerArea;
        public bool AlternativeNametables; // false 没有， true 有 （不同含义，4KB RAM 在 PPU $2000-2FFF、4屏等）

        public ConsoleType ConsoleType; // 主机类型
        public bool IsNes20; // Nes 2.0 格式

        public int MapperNumber; // 映射器编号
        public byte SubmapperNumber; // 子映射器编号

        public CpuTimeMode CpuTimeMode; // CPU时间模式
        public byte? PpuType;
        public byte? HardwareType;
        public byte? ExtendedConsoleType;

        public byte RomNumers; // 拥有的各种ROM的数量
        public byte DefaultExpansionDevice; // 扩展设备类型

        public uint PrgRomSize;
        public uint ChrRomSize;

        public Header(byte[] data)
        {
            // header
            if (data[0] != 'N' || data[1] != 'E' || data[2] != 'S' || data[3] != 0x1A)
            {
                // 类型异常
                throw new InvalidDataException("Invalid NES Data Format! File must begin with \"NES\"");
            }

            // prgROM size
            var prgRomSizeLSB = data[4];
            // chrROM size
            var chrRomSizeLSB = data[5];

            // Flags 6
            var flag6 = data[6];
            HardwiredNametaleLayoot = (flag6 & 1) == 1;
            BatterAndOtherNonVolatileMemory = (flag6 & (1 << 1)) == 1;
            HasTrainerArea = (flag6 & (1 << 2)) == 1;
            AlternativeNametables = (flag6 & (1 << 3)) == 1;
            MapperNumber |= (flag6 & 0xF0) >> 4;

            // Flags 7
            var flag7 = data[7];
            ConsoleType = (ConsoleType)(flag7 & 3);
            IsNes20 = (flag7 & 0x0C) == 0x08;
            MapperNumber |= (flag7 & 0xF0);

            // Flags 8
            var flag8 = data[8];
            MapperNumber |= (flag8 & 0xF) << 8;
            SubmapperNumber = (byte)((flag8 & 0xF0) >> 4);

            // Flags 9
            var flag9 = data[9];
            var prgRomSizeMSB = flag9 & 0xF;
            var chrRomSizeMSB = (flag9 & 0xF0) >> 4;

            // Flags 10
            var flag10 = data[10];
            var prgRamShiftCount = flag10 & 0xF; // 64 << shift count size
            var prgNvRamShiftCount = (flag10 & 0xF0) >> 4;

            // Flags 11
            var flag11 = data[11];
            var chrRamShiftCount = flag11 & 0xF; // 64 << shift count size
            var chrNvRamShiftCount = (flag11 & 0xF0) >> 4;

            // Flags 12
            var flag12 = data[12];
            CpuTimeMode = (CpuTimeMode)(flag12 & 3);

            // Flags 13
            var flag13 = data[13];
            if ((flag7 & 3) == 1)
            {
                PpuType = (byte)(flag13 & 0xF);
                HardwareType = (byte)((flag13 & 0xF0) >> 4);
            }
            else if ((flag7 & 3) == 3)
            {
                ExtendedConsoleType = (byte)(flag13 & 0xF);
            }

            // Flags 14
            var flag14 = data[4];
            RomNumers = (byte)(flag14 & 3);

            // Flags 15
            var flag15 = data[15];
            DefaultExpansionDevice = (byte)(flag14 & 0x3F);

            if (prgRomSizeMSB < 0xF)
            {
                PrgRomSize = (uint)((prgRomSizeMSB << 8) + prgRomSizeLSB) * 16 * 1024;
            }
            else
            {
                var e = (prgRomSizeLSB & 0xFC) >> 2;
                var m = prgRomSizeLSB & 3;
                PrgRomSize = (uint)(((int)Math.Pow(2, e)) * (m * 2 + 1));
            }

            if (chrRomSizeMSB < 0xF)
            {
                ChrRomSize = (uint)((chrRomSizeMSB << 8) + chrRomSizeLSB) * 8 * 1024;
            }
            else
            {
                var e = (chrRomSizeLSB & 0xFC) >> 2;
                var m = chrRomSizeLSB & 3;
                ChrRomSize = (uint)(((int)Math.Pow(2, e)) * (m * 2 + 1));
            }
        }
    }

    public enum ConsoleType : byte
    { 
        NES, // Nintendo Entertainment System/Family Computer
        NVS, // Nintendo Vs. System
        NP, // Nintendo Playchoice 10
        ECT, // Extended Console Type
    }

    public enum CpuTimeMode
    {
        RP2C02, // NTSC NES
        RP2C07, // Licensed PAL NES
        MultipleReigion, // Multiple-region
        UA6538, // Dendy
    }
}
