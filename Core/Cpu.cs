﻿using FamicomSimulator.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FamicomSimulator.Core
{
    internal partial class Cpu
    {
        /// <summary>
        /// 可寻址内存
        /// </summary>

        public required Famicom Famicom { get; set; }
        public Ppu Ppu => Famicom.Ppu;

        internal CpuRegister register = new CpuRegister();

        internal void LoadROM(RomInfo romInfo)
        {
            int id2 = (int)((romInfo.Header.PrgRomSize / 1024 / 16) & 2);
            LoadPrgRom8K(romInfo, 0, 0);
            LoadPrgRom8K(romInfo, 1, 1);
            LoadPrgRom8K(romInfo, 2, id2);
            LoadPrgRom8K(romInfo, 3, id2 + 1);
            AtPower();
        }

        private void LoadPrgRom8K(RomInfo romInfo, int des, int src)
        {
            Array.Copy(romInfo.DataPrgRom, 8 * 1024 * src, Famicom.PrgBanks[4 + des], 0, 8 * 1024);
        }

        private void AtPower()
        {
            register.PC = ReadAddress((ushort)InterruptVector.RESET);
            register.A = 0;
            register.X = 0;
            register.Y = 0;
            register.SP = 0xFD;
            register.C = 0;
            register.Z = 0;
            register.I = 1;
            register.D = 0;
            register.B = 0;
            register.R = 1;
            register.V = 0;
            register.N = 0;
        }

        private void AtReset()
        {
            register.PC = ReadAddress((ushort)InterruptVector.RESET);
            register.SP -= 3;
            register.I = 1;
        }

        private void CheckFlagZ(byte v)
        {
            register.Z = v == 0 ? (byte)1 : (byte)0; 
        }

        private void CheckFlagS(byte v)
        {
            register.S = (((byte)(v & 0x80)) != 0) ? (byte)1 : (byte)0;
        }

        private void CheckFlagC(bool flag)
        { 
            register.C = flag ? (byte)1 : (byte)0;
        }

        private void CheckFlagV(bool flag)
        {
            register.V = flag ? (byte)1 : (byte)0;
        }

        private void CheckFlagZS(byte v)
        {
            CheckFlagZ(v);
            CheckFlagS(v);
        }


        internal void Tick()
        {
            var opCode = ReadByte(register.PC);
            register.PC++;

            var instruction = GetInstructionByOpCode(opCode);
            // 取值操作
            var address = Addressing(instruction.AddressingMode);
            // op操作
            Operating(instruction.Instruction, address, instruction.AddressingMode);
        }

        private void Operating(Instruction instruction, ushort address, AddressingMode addressingMode)
        {
            switch (instruction)
            {
                case Instruction.LDA:
                    LDA(address);
                    break;
                case Instruction.STA:
                    STA(address);
                    break;
                case Instruction.LDX:
                    LDX(address);
                    break;
                case Instruction.STX:
                    STX(address);
                    break;
                case Instruction.LDY:
                    LDY(address);
                    break;
                case Instruction.STY:
                    STY(address);
                    break;
                case Instruction.TAX:
                    TAX();
                    break;
                case Instruction.TXA:
                    TXA();
                    break;
                case Instruction.TAY:
                    TAY();
                    break;
                case Instruction.TYA:
                    TYA();
                    break;
                case Instruction.ADC:
                    ADC(address);
                    break;
                case Instruction.SBC:
                    SBC(address);
                    break;
                case Instruction.INC:
                    INC(address);
                    break;
                case Instruction.DEC:
                    DEC(address);
                    break;
                case Instruction.INX:
                    INX();
                    break;
                case Instruction.DEX:
                    DEX();
                    break;
                case Instruction.INY:
                    INY();
                    break;
                case Instruction.DEY:
                    DEY();
                    break;
                case Instruction.ASL:
                    ASL(address, addressingMode);
                    break;
                case Instruction.LSR:
                    LSR(address, addressingMode);
                    break;
                case Instruction.ROL:
                    ROL(address, addressingMode);
                    break;
                case Instruction.ROR:
                    ROR(address, addressingMode);
                    break;
                case Instruction.AND:
                    AND(address);
                    break;
                case Instruction.ORA:
                    ORA(address);
                    break;
                case Instruction.EOR:
                    EOR(address);
                    break;
                case Instruction.BIT:
                    BIT(address);
                    break;
                case Instruction.CMP:
                    CMP(address);
                    break;
                case Instruction.CPX:
                    CPX(address);
                    break;
                case Instruction.CPY:
                    CPY(address);
                    break;
                case Instruction.BCC:
                    BCC(address);
                    break;
                case Instruction.BCS:
                    BCS(address);
                    break;
                case Instruction.BEQ:
                    BEQ(address);
                    break;
                case Instruction.BNE:
                    BNE(address);
                    break;
                case Instruction.BPL:
                    BPL(address);
                    break;
                case Instruction.BMI:
                    BMI(address);
                    break;
                case Instruction.BVC:
                    BVC(address);
                    break;
                case Instruction.BVS:
                    BVS(address);
                    break;
                case Instruction.JMP:
                    JMP(address);
                    break;
                case Instruction.JSR:
                    JSR(address);
                    break;
                case Instruction.RTS:
                    RTS();
                    break;
                case Instruction.BRK:
                    BRK();
                    break;
                case Instruction.RTI:
                    RTI();
                    break;
                case Instruction.PHA:
                    PHA();
                    break;
                case Instruction.PLA:
                    PLA();
                    break;
                case Instruction.PHP:
                    PHP();
                    break;
                case Instruction.PLP:
                    PLP();
                    break;
                case Instruction.TXS:
                    TXS();
                    break;
                case Instruction.TSX:
                    TSX();
                    break;
                case Instruction.CLC:
                    CLC();
                    break;
                case Instruction.SEC:
                    SEC();
                    break;
                case Instruction.CLI:
                    CLI();
                    break;
                case Instruction.SEI:
                    SEI();
                    break;
                case Instruction.CLD:
                    CLD();
                    break;
                case Instruction.SED:
                    SED();
                    break;
                case Instruction.CLV:
                    CLV();
                    break;
                case Instruction.NOP:
                    NOP();
                    break;
                case Instruction.ALR:
                    ALR(address);
                    break;
                case Instruction.ANC:
                    ANC(address);
                    break;
                case Instruction.ARR:
                    ARR(address);
                    break;
                case Instruction.AXS:
                    AXS(address);
                    break;
                case Instruction.LAX:
                    LAX(address);
                    break;
                case Instruction.SAX:
                    SAX(address);
                    break;
                case Instruction.DCP:
                    DCP(address);
                    break;
                case Instruction.ISC:
                    ISC(address);
                    break;
                case Instruction.RLA:
                    RLA(address);
                    break;
                case Instruction.RRA:
                    RRA(address);
                    break;
                case Instruction.SLO:
                    SLO(address);
                    break;
                case Instruction.SRE:
                    SRE(address);
                    break;
                case Instruction.SHX:
                    SHX();
                    break;
                case Instruction.SHY:
                    SHY();
                    break;
                case Instruction.LAS:
                    LAS();
                    break;
                case Instruction.XAA:
                    XAA();
                    break;
                case Instruction.AHX:
                    AHX();
                    break;
                case Instruction.TAS:
                    TAS();
                    break;
                case Instruction.STP:
                    STP();
                    break;
            }
        }

        private ushort Addressing(AddressingMode addressingMode)
        {
            ushort address = 0;
            switch (addressingMode)
            {
                case AddressingMode.Immediate:
                    AddressingImmediate(out address);
                    break;
                case AddressingMode.ZeroPage:
                    AddressingZeroPage(out address);
                    break;
                case AddressingMode.ZeroPageX:
                    AddressingZeroPageX(out address);
                    break;
                case AddressingMode.ZeroPageY:
                    AddressingZeroPageY(out address);
                    break;
                case AddressingMode.Absolute:
                    AddressingAbsolute(out address);
                    break;
                case AddressingMode.AbsoluteX:
                    AddressingAbsoluteX(out address);
                    break;
                case AddressingMode.AbsoluteY:
                    AddressingAbsoluteY(out address);
                    break;
                case AddressingMode.IndirectX:
                    AddressingIndirectX(out address);
                    break;
                case AddressingMode.IndirectY:
                    AddressingIndirectY(out address);
                    break;
                case AddressingMode.Relative:
                    AddressingRelative(out address);
                    break;
                case AddressingMode.Implicit:
                    // 隐含寻址，不需要操作数
                    break;
                case AddressingMode.Accumulator:
                    // 操作数是累加器
                    break;
                case AddressingMode.Indirect:
                    AddressingIndirect(out address);
                    break;
                default:
                    break;
            }
            return address;
        }

        private OpConfig GetInstructionByOpCode(byte opcode)
        {
            return AssembleCode.OpCodeArrangement[opcode];
        }

        internal byte ReadByte(ushort address)
        {
            switch (address >> 13)
            {
                case 0: // [$0000, $2000) 系统主内存，2K内存四次镜像成8K
                    return Famicom.MainMemory[address % (1 << 11)];
                case 1: // [$2000, $4000) PPU 寄存器
                    return Ppu.ReadByteFromCpu(address);
                case 2: // [$4000, $6000) pAPU寄存器以及扩展区域
                    if (address < 0x4020)
                    {
                        byte data = 0;
                        switch (address & 0x1F)
                        {
                            case 0x16:
                                data = Famicom.ButtonStates[Famicom.ButtonIndex1 & Famicom.ButtonIndexMask];
                                Famicom.ButtonIndex1++;
                                break;
                            case 0x17:
                                data = Famicom.ButtonStates[8 + Famicom.ButtonIndex2 & Famicom.ButtonIndexMask];
                                Famicom.ButtonIndex2++;
                                break;
                        }
                        return data;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                case 3: // [$6000, $8000) 存档用SRAM区
                    return Famicom.SaveMemory[address & 0x1fff];
                case 4: // 程序代码区 PRG-ROM
                case 5: // 程序代码区 PRG-ROM
                case 6: // 程序代码区 PRG-ROM
                case 7: // 程序代码区 PRG-ROM
                    return Famicom.PrgBanks[address >> 13][address & 0x1fff];
                default:
                    return 0;
            }
        }

        internal void WriteByte(ushort address, byte value) 
        {
            switch (address >> 13)
            {
                case 0: // [$0000, $2000) 系统主内存，2K内存四次镜像成8K
                    Famicom.MainMemory[address % (1 << 11)] = value;
                    break;
                case 1: // [$2000, $4000) PPU 寄存器
                    Ppu.WriteByteFromCpu(address, value);
                    break;
                case 2: // [$4000, $6000) pAPU寄存器以及扩展区域
                    if (address < 0x4020)
                    {
                        switch (address & 0x1F)
                        {
                            case 0x14:
                                // 0x4014 DMA 访问精灵 RAM
                                Ppu.DMA(value);
                                break;
                            case 0x16:
                                WriteCtrlByte(value);
                                break;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
                case 3: // [$6000, $8000) 存档用SRAM区
                    Famicom.SaveMemory[address & 0x1fff] = value;
                    break;
                case 4: // 程序代码区 PRG-ROM
                case 5: // 程序代码区 PRG-ROM
                case 6: // 程序代码区 PRG-ROM
                case 7: // 程序代码区 PRG-ROM
                    Famicom.PrgBanks[address >> 13][address & 0x1fff] = value;
                    break;
                default:
                    break;
            }
        }

        private void WriteCtrlByte(byte value)
        {
            Famicom.ButtonIndexMask = (ushort)(((value & 1) != 0) ? 0 : 7);
            if ((value & 1) != 0)
            {
                Famicom.ButtonIndex1 = 0;
                Famicom.ButtonIndex2 = 0;
            }
        }

        internal ushort ReadAddress(ushort address)
        {
            var l = ReadByte((ushort)address);
            var m = (ReadByte((ushort)(address + 1)) << 8);
            return (ushort)(l | m);
        }

        internal void Push(byte v)
        {
            Famicom.MainMemory[0x100 + register.SP] = v;
            register.SP--;
        }

        internal byte Pull()
        {
            register.SP++;
            return Famicom.MainMemory[0x100 + register.SP];
        }

        internal enum InterruptVector : ushort
        { 
            NMI = 0xFFFA, // 不可屏蔽中断，发生在每次 VBlank 时
            RESET = 0xFFFC, // 重启
            IRQ_BRK = 0xFFFE, // 硬件（Mapper、APU）、软件（BRK指令）中断请求
        }

        /// <summary>
        /// D 和 R 在 NES 的 6502 中无作用，增加了音频支持
        /// </summary>
        [Flags]
        internal enum StatusRegisterFlag : byte
        { 
            C = 1 << 0, // 进位标志 Carry
            Z = 1 << 1, // 零标志 Zero
            I = 1 << 2, // 禁止中断标志 Interrupt
            D = 1 << 3, // 十进制模式标志 Decimal
            B = 1 << 4, // 软中断标志 Break
            R = 1 << 5, // 保留标志 Reserved 一直为1
            V = 1 << 6, // 溢出标志 Overflow
            S = 1 << 7, // 符号标志 Sign
            N = 1 << 7, // 符号标志 Negative
        }

        internal struct CpuRegister
        {
            internal ushort PC; // Program Counter
            internal byte A; // 累加器
            internal byte X; // X 寄存器
            internal byte Y; // Y 寄存器
            internal StatusRegisterFlag P; // 状态寄存器
            internal byte SP; // 栈指针 $100-$1FF

            internal byte C 
            { 
                get => BitUtil.GetBitValue((byte)P, 0);
                set => P = (StatusRegisterFlag)BitUtil.SetBitValue((byte)P, 0, value);
            }

            internal byte Z
            {
                get => BitUtil.GetBitValue((byte)P, 1);
                set => P = (StatusRegisterFlag)BitUtil.SetBitValue((byte)P, 1, value);
            }

            internal byte I
            {
                get => BitUtil.GetBitValue((byte)P, 2);
                set => P = (StatusRegisterFlag)BitUtil.SetBitValue((byte)P, 2, value);
            }

            internal byte D
            {
                get => BitUtil.GetBitValue((byte)P, 3);
                set => P = (StatusRegisterFlag)BitUtil.SetBitValue((byte)P, 3, value);
            }

            internal byte B
            {
                get => BitUtil.GetBitValue((byte)P, 4);
                set => P = (StatusRegisterFlag)BitUtil.SetBitValue((byte)P, 4, value);
            }

            internal byte R
            {
                get => BitUtil.GetBitValue((byte)P, 5);
                set => P = (StatusRegisterFlag)BitUtil.SetBitValue((byte)P, 5, value);
            }

            internal byte V
            {
                get => BitUtil.GetBitValue((byte)P, 6);
                set => P = (StatusRegisterFlag)BitUtil.SetBitValue((byte)P, 6, value);
            }

            internal byte S
            {
                get => BitUtil.GetBitValue((byte)P, 7);
                set => P = (StatusRegisterFlag)BitUtil.SetBitValue((byte)P, 7, value);
            }

            internal byte N
            {
                get => BitUtil.GetBitValue((byte)P, 7);
                set => P = (StatusRegisterFlag)BitUtil.SetBitValue((byte)P, 7, value);
            }
        }


        internal void DoVblank()
        {
            Ppu.SetVblankFlag();
            if ((Ppu.CtrlRgister & Ppu.PpuCtrlRegisterFlag.V) != 0)
            {
                NMI();
            }
        }

        public override string ToString()
        {
            var assemblyCode = GetCurrentAssembleString();
            return $"${register.PC:X4}  {assemblyCode, -20};    A:{register.A:X2} X:{register.X:X2} Y:{register.Y:X2} P:{(byte)register.P:X2} SP:{register.SP:X2}";
        }
    }
}
