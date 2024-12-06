using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Core
{
    public class Cpu
    {
        /// <summary>
        /// 可寻址内存
        /// </summary>
        public byte[] Memory = new byte[1 << 16]; // 内存可寻址区域共 64 KB

        /// <summary>
        /// 单个 Bank 为 8KB，共 8 块
        /// 0: [$0000, $2000) 系统主内存，2K内存四次镜像成8K
        /// 1: [$2000, $4000) PPU 寄存器
        /// 2: [$4000, $6000) pAPU寄存器以及扩展区域
        /// 3: [$6000, $8000) 存档用SRAM区
        /// 剩下的全是 程序代码区 PRG-ROM
        /// </summary>
        public readonly int[] Banks = new int[8];

        public void LoadROM(RomInfo romInfo)
        {
            if (romInfo.DataPrgRom.Length / 1024 / 16 == 1)
            {
                Array.Copy(romInfo.DataPrgRom, 0, Memory, 0x8000, romInfo.DataPrgRom.Length);
                Array.Copy(romInfo.DataPrgRom, 0, Memory, 0xC000, romInfo.DataPrgRom.Length); // 镜像
            }
            else
            {
                Array.Copy(romInfo.DataPrgRom, 0, Memory, 0x8000, romInfo.DataPrgRom.Length);
            }
        }

        public byte ReadByte(ushort address)
        {
            switch ((address & 0xC000) >> 13)
            {
                case 0: // [$0000, $2000) 系统主内存，2K内存四次镜像成8K
                    return Memory[address & (1 << 11)];
                case 1: // [$2000, $4000) PPU 寄存器
                    throw new NotImplementedException();
                case 2: // [$4000, $6000) pAPU寄存器以及扩展区域
                    throw new NotImplementedException();
                case 3: // [$6000, $8000) 存档用SRAM区
                    return Memory[address];
                case 4: // 程序代码区 PRG-ROM
                case 5: // 程序代码区 PRG-ROM
                case 6: // 程序代码区 PRG-ROM
                case 7: // 程序代码区 PRG-ROM
                    return Memory[address];
                default:
                    return 0;
            }
        }

        public void WriteByte(ushort address, byte value) 
        {
            switch ((address & 0xC0) >> 13)
            {
                case 0: // [$0000, $2000) 系统主内存，2K内存四次镜像成8K
                    Memory[address & (1 << 11)] = value;
                    break;
                case 1: // [$2000, $4000) PPU 寄存器
                    throw new NotImplementedException();
                case 2: // [$4000, $6000) pAPU寄存器以及扩展区域
                    throw new NotImplementedException();
                case 3: // [$6000, $8000) 存档用SRAM区
                    Memory[address] = value;
                    break;
                case 4: // 程序代码区 PRG-ROM
                case 5: // 程序代码区 PRG-ROM
                case 6: // 程序代码区 PRG-ROM
                case 7: // 程序代码区 PRG-ROM
                    Memory[address] = value;
                    break;
                default:
                    break;
            }
        }

        public enum InterruptVector
        { 
            NMI = 0xFFFA, // 不可屏蔽中断，发生在每次 VBlank 时
            RESET = 0xFFFC, // 重启
            IRQ_BRK = 0xFFFE, // 硬件（Mapper、APU）、软件（BRK指令）中断请求
        }

        public ushort GetInterruptAddress(InterruptVector vector)
        {
            var l = ReadByte((ushort)vector);
            var m = (ReadByte((ushort)(vector + 1)) << 8);
            return (ushort)(l | m);
        }
    }
}
