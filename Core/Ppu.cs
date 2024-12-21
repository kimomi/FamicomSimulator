using FamicomSimulator.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Core
{
    internal class Ppu
    {
        public required Famicom Famicom { get; internal set; }
        private Cpu _cpu => Famicom.Cpu;

        public Ppu()
        {
            for (int i = 0; i < Banks.Length; i++)
            {
                Banks[i] = new byte[1024];
            }
        }

        /// <summary>
        /// 可寻址内存
        /// </summary>
        //internal byte[] Memory = new byte[1 << 14]; // PPU 内存可寻址区域共 16 KB
        internal byte[][] Banks = new byte[0x4000 / 0x0400][]; // 内存地址库

        public byte[] Spindexes = new byte[0x20]; // 64 bytes of 精灵调色板索引
        public byte[] Sprites = new byte[0x100]; // 256 bytes of 精灵数据

        // VRAM 地址
        public ushort VRamAddr;
        // 滚动偏移
        public byte[] Scroll = new byte[2];
        // 滚动偏移双写位置记录
        public byte Writex2;
        // 显存读取缓冲值
        public byte pseudo;

        public PpuCtrlRegisterFlag CtrlRgister;
        public PpuMaskRgisterFlag MaskRgister;
        public PpuStatusRgisterFlag StatusRgister;
        public byte OamAddrRgister;
        public byte OamDataRgister;
        public byte PpuScrollRgister;
        public byte PpuAddrRgister;
        public byte PpuDataRgister;
        public byte OamDmaRgister;

        internal void LoadROM(RomInfo romInfo)
        {
            foreach (var bank in Banks)
            {
                Array.Clear(bank);
            }
            Array.Clear(Spindexes);
            Array.Clear(Sprites);
            Array.Clear(Scroll);

            // CHR-ROM
            for (int i = 0; i < 8; i++)
            {
                Array.Copy(romInfo.DataChrRom, i * 1024, Banks[i], 0, 1024);
            }

            // 4屏
            if (romInfo.Header.FourScreen)
            {
                Banks[0x8] = Famicom.VideoMemory[0];
                Banks[0x9] = Famicom.VideoMemory[1];
                Banks[0xa] = Famicom.VideoMemoryEx[0];
                Banks[0xb] = Famicom.VideoMemoryEx[1];
            }
            // 横版
            else if (romInfo.Header.HardwiredNametaleLayoot)
            {
                Banks[0x8] = Famicom.VideoMemory[0];
                Banks[0x9] = Famicom.VideoMemory[1];
                Banks[0xa] = Famicom.VideoMemory[0];
                Banks[0xb] = Famicom.VideoMemory[1];
            }
            // 纵版
            else
            {
                Banks[0x8] = Famicom.VideoMemory[0];
                Banks[0x9] = Famicom.VideoMemory[0];
                Banks[0xa] = Famicom.VideoMemory[1];
                Banks[0xb] = Famicom.VideoMemory[1];
            }

            // 镜像
            Banks[0xc] = Banks[0x8];
            Banks[0xd] = Banks[0x9];
            Banks[0xe] = Banks[0xa];
            Banks[0xf] = Banks[0xb];
        }

        [Flags]
        internal enum PpuCtrlRegisterFlag : byte
        {
            N0 = 1 << 0, //
            N1 = 1 << 1, // nametable address (0 = $2000; 1 = $2400; 2 = $2800; 3 = $2C00)
            I = 1 << 2, // VRAM address increment per CPU read/write of PPUDATA (0: add 1, going across; 1: add 32, going down)
            S = 1 << 3, // Sprite pattern table address for 8x8 sprites (0: $0000; 1: $1000; ignored in 8x16 mode)
            B = 1 << 4, // Background pattern table address (0: $0000; 1: $1000)
            H = 1 << 5, // Sprite size (0: 8x8 pixels; 1: 8x16 pixels – see PPU OAM#Byte 1)
            P = 1 << 6, // PPU master/slave select (0: read backdrop from EXT pins; 1: output color on EXT pins)
            V = 1 << 7, // Vblank NMI enable (0: off, 1: on)
        }

        [Flags]
        internal enum PpuMaskRgisterFlag : byte
        {
            g = 1 << 0, // Grayscale (0: normal color, 1: produce a monochrome display)
            m = 1 << 1, // 1: Show background in leftmost 8 pixels of screen, 0: Hide
            M = 1 << 2, // 1: Show sprites in leftmost 8 pixels of screen, 0: Hide
            b = 1 << 3, // 1: Enable background rendering
            s = 1 << 4, // 1: Enable sprite rendering
            R = 1 << 5, // Emphasize red (green on PAL/Dendy)
            G = 1 << 6, // Emphasize green (red on PAL/Dendy)
            B = 1 << 7, // Emphasize blue
        }

        [Flags]
        internal enum PpuStatusRgisterFlag : byte
        {
            // 前五位 (PPU open bus or 2C05 PPU identifier)
            O = 1 << 5, // Sprite overflow flag
            S = 1 << 6, // Sprite 0 hit flag
            V = 1 << 7, // Vblank flag, cleared on read. Unreliable
        }

        public const ushort PpuCtrlRgisterAddr = 0X2000;
        public const ushort PpuMaskRgisterAddr = 0X2001;
        public const ushort PpuStatusRgisterAddr = 0X2002;
        public const ushort OamAddrRgisterAddr = 0X2003;
        public const ushort OamDataRgisterAddr = 0X2004;
        public const ushort PpuScrollRgisterAddr = 0X2005;
        public const ushort PpuAddrRgisterAddr = 0X2006;
        public const ushort PpuDataRgisterAddr = 0X2007;
        public const ushort OamDmaRgisterAddr = 0X4014;
        public const int Status_SpriteOverFlowFlag_Num = 5;
        public const int Status_Sprite0HitFlag_Num = 6;
        public const int Status_VblankFlag_Num = 7;

        internal byte ReadByteFromCpu(ushort address)
        {
            byte data = 0x00;
            var num = address & 0x7; // PPU 寄存器为 8 字节镜像
            switch (num)
            {
                case 0:
                case 1:
                    Debug.Fail("只写寄存器不可读取");
                    break;
                case 2:
                    data = (byte)StatusRgister;
                    // 读取后会清除VBlank状态
                    StatusRgister = (PpuStatusRgisterFlag)BitUtil.SetBitValue(data, Status_VblankFlag_Num, 0);
                    break;
                case 3:
                    Debug.Fail("只写寄存器不可读取");
                    break;
                case 4:
                    data = Sprites[OamAddrRgister];
                    OamAddrRgister++;
                    break;
                case 5:
                    // 双写寄存器
                    break;
                case 6:
                    // 双写寄存器
                    break;
                case 7:
                    // PPU VRAM读写端口
                    data = ReadPpuAddress(VRamAddr);
                    var isVramAddressAdd = (byte)CtrlRgister & (byte)PpuCtrlRegisterFlag.I;
                    if (isVramAddressAdd == 0)
                    {
                        VRamAddr += 1;
                    }
                    else
                    {
                        VRamAddr += 32;
                    }
                    break;
                default:
                    break;
            }
            return data;
        }

        internal void WriteByteFromCpu(ushort address, byte data)
        {
            var num = address & 0x7; // PPU 寄存器为 8 字节镜像
            switch (num)
            {
                case 0:
                    CtrlRgister = (PpuCtrlRegisterFlag)data;
                    break;
                case 1:
                    MaskRgister = (PpuMaskRgisterFlag)data;
                    break;
                case 2:
                    Debug.Fail("只读寄存器不可写入");
                    break;
                case 3:
                    OamAddrRgister = data;
                    break;
                case 4:
                    Sprites[OamAddrRgister] = data;
                    OamAddrRgister += 1;
                    break;
                case 5:
                    Scroll[Writex2 & 1] = data;
                    Writex2++;
                    break;
                case 6:
                    if ((Writex2 & 1) != 0) // 写入高字节
                    {
                        VRamAddr = (ushort)((VRamAddr | 0xFF00) | data);
                    }
                    else // 写入低字节
                    {
                        VRamAddr = (ushort)((VRamAddr | 0x00FF) | (data << 8));
                    }
                    Writex2++;
                    break;
                case 7:
                    WritePpuAddress(VRamAddr, data);
                    var isVramAddressAdd = (byte)CtrlRgister & (byte)PpuCtrlRegisterFlag.I;
                    if (isVramAddressAdd == 0)
                    {
                        VRamAddr += 1;
                    }
                    else
                    {
                        VRamAddr += 32;
                    }
                    break;
                default:
                    break;
            }
        }

        private byte ReadPpuAddress(ushort address)
        {
            var realAddress = (ushort)(address & 0x3FFF);
            if (realAddress < 0x3F00) // 使用BANK读取
            {
                ushort index = (ushort)(realAddress >> 10);
                ushort offset = (ushort)(realAddress & 0x3FF);
                Debug.Assert(index < Banks.Length);
                byte data = pseudo;
                pseudo = Banks[index][offset];
                return data;
            }
            else // 调色板索引
            {
                pseudo = Spindexes[realAddress & 0x1F];
                return pseudo;
            }
        }

        private void WritePpuAddress(ushort address, byte data)
        { 
            var realAddress = (ushort)(address & 0x3FFF);
            if (realAddress < 0x3F00) // 使用BANK写入
            {
                Debug.Assert(realAddress >= 0x2000);
                ushort index = (ushort)(realAddress >> 10);
                ushort offset = (ushort)(realAddress & 0x3FF);
                Debug.Assert(index < Banks.Length);
                Banks[index][offset] = data;
            }
            else // 调色板索引
            {
                if ((realAddress & 0x03) != 0)
                {
                    Spindexes[realAddress & 0x1F] = data;
                }
                else // 镜像$3F00/$3F04/$3F08/$3F0C
                {
                    var offset = realAddress & 0x0F;
                    Spindexes[offset] = data;
                    Spindexes[offset | 0x10] = data;
                }
            }
        }

        internal void SetVblankFlag()
        {
            StatusRgister |= PpuStatusRgisterFlag.V;
        }
    }
}
