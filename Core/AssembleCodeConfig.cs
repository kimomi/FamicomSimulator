using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Core
{
    /// <summary>
    /// 汇编指令类型
    /// </summary>
    internal enum InstructionType
    {
        Access,
        Transfer,
        Arithmetic,
        Shift,
        Bitwise,
        Compare,
        Branch,
        Jump,
        Stack,
        Flags,
        Other,
    }

    /// <summary>
    /// 汇编指令
    /// </summary>
    internal enum Instruction
    {
        LDA,
        STA,
        LDX,
        STX,
        LDY,
        STY,

        TAX,
        TXA,
        TAY,
        TYA,

        ADC,
        SBC,
        INC,
        DEC,
        INX,
        DEX,
        INY,
        DEY,

        ASL,
        LSR,
        ROL,
        ROR,

        AND,
        ORA,
        EOR,
        BIT,

        CMP,
        CPX,
        CPY,

        BCC,
        BCS,
        BEQ,
        BNE,
        BPL,
        BMI,
        BVC,
        BVS,

        JMP,
        JSR,
        RTS,
        BRK,
        RTI,

        PHA,
        PLA,
        PHP,
        PLP,
        TXS,
        TSX,

        CLC,
        SEC,
        CLI,
        SEI,
        CLD,
        SED,
        CLV,

        NOP,
        ALR,
        ANC,
        ARR,
        AXS,
        LAX,
        SAX,
        DCP,
        ISC,
        RLA,
        RRA,
        SLO,
        SRE,
        SHX,
        SHY,
        LAS,
        XAA,
        AHX,
        TAS,
        STP,
    }

    /// <summary>
    /// 寻址方式
    /// </summary>
    internal enum AddressingMode
    {
        Implicit,
        Immediate, // #i
        ZeroPage, // d
        ZeroPageX, // d,x
        ZeroPageY, // d,y
        Absolute, // a
        AbsoluteX, // a,x
        AbsoluteY, // a,y
        IndirectX, // (d,x)
        IndirectY, // (d),y
        Relative, // *+d
        Accumulator, // A
        Indirect, // (a)
    }
}
