using FamicomSimulator.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Core
{
    internal partial class Cpu
    {
        private void LDA(ushort address)
        { 
            register.A = ReadByte(address);
            CheckFlagZS(register.A);
        }

        private void LDX(ushort address)
        {
            register.X = ReadByte(address);
            CheckFlagZS(register.X);
        }

        private void LDY(ushort address)
        {
            register.Y = ReadByte(address);
            CheckFlagZS(register.Y);
        }

        private void STA(ushort address)
        {
            WriteByte(address, register.A);
        }

        private void STX(ushort address)
        {
            WriteByte(address, register.X);
        }

        private void STY(ushort address)
        {
            WriteByte(address, register.Y);
        }

        private void ADC(ushort address)
        {
            var m = ReadByte(address);
            ushort result = (ushort)(register.A + m + register.C);
            CheckFlagC((byte)(result >> 8) != 0);
            byte byteResult = (byte)result;
            CheckFlagV(((register.A ^ m) & 0x80) == 0 && ((register.A ^ byteResult) & 0x80) != 0);
            register.A = byteResult;
            CheckFlagZS(register.A);
        }

        private void SBC(ushort address)
        {
            var m = ReadByte(address);
            ushort result = (ushort)(register.A - m - (1 - register.C));
            CheckFlagC((byte)(result >> 8) == 0);
            byte byteResult = (byte)result;
            CheckFlagV(((register.A ^ byteResult) & 0x80) != 0 && ((register.A ^ m) & 0x80) != 0);
            register.A = byteResult;
            CheckFlagZS(register.A);
        }

        private void INC(ushort address)
        {
            var m = ReadByte(address);
            m++;
            WriteByte(address, m);
            CheckFlagZS(m);
        }

        private void DEC(ushort address) 
        {
            var m = ReadByte(address);
            m--;
            WriteByte(address, m);
            CheckFlagZS(m);
        }

        private void AND(ushort address)
        {
            register.A &= ReadByte(address);
            CheckFlagZS(register.A);
        }

        private void ORA(ushort address)
        {
            register.A |= ReadByte(address);
            CheckFlagZS(register.A);
        }

        private void EOR(ushort address)
        {
            register.A ^= ReadByte(address);
            CheckFlagZS(register.A);
        }

        private void INX()
        {
            register.X++;
            CheckFlagZS(register.X);
        }

        private void DEX()
        {
            register.X--;
            CheckFlagZS(register.X);
        }

        private void INY()
        {
            register.Y++;
            CheckFlagZS(register.Y);
        }

        private void DEY()
        {
            register.Y--;
            CheckFlagZS(register.Y);
        }

        private void TAX()
        {
            register.X = register.A;
            CheckFlagZS(register.X);
        }

        private void TXA()
        { 
            register.A = register.X;
            CheckFlagZS(register.A);
        }

        private void TAY()
        {
            register.Y = register.A;
            CheckFlagZS(register.Y);
        }

        private void TYA()
        {
            register.A = register.Y;
            CheckFlagZS(register.A);
        }

        private void TSX()
        {
            register.X = register.SP;
            CheckFlagZS(register.X);
        }

        private void TXS()
        {
            register.SP = register.X;
        }

        private void CLC()
        {
            register.C = 0;
        }

        private void SEC()
        {
            register.C = 1;
        }

        private void CLD()
        {
            register.D = 0;
        }

        private void SED()
        {
            register.D = 1;
        }

        private void CLV()
        {
            register.V = 0;
        }

        private void CLI()
        {
            register.I = 0;
        }

        private void SEI()
        {
            register.I = 1;
        }

        private void CMP(ushort address)
        { 
            ushort m = (ushort)((ushort)register.A - (ushort)ReadByte(address));
            register.C = m < 0x100 ? (byte)1 : (byte)0;
            CheckFlagZS((byte)m);
        }

        private void CPX(ushort address)
        {
            ushort m = (ushort)((ushort)register.X - (ushort)ReadByte(address));
            register.C = m < 0x100 ? (byte)1 : (byte)0;
            CheckFlagZS((byte)m);
        }

        private void CPY(ushort address)
        {
            ushort m = (ushort)((ushort)register.Y - (ushort)ReadByte(address));
            register.C = m < 0x100 ? (byte)1 : (byte)0;
            CheckFlagZS((byte)m);
        }

        private void BIT(ushort address)
        {
            var m = ReadByte(address);
            register.V = BitUtil.GetBitValue(m, 6);
            register.S = BitUtil.GetBitValue(m, 7);
            register.Z = ((register.A & m) == 0) ? (byte)1 : (byte)0;
        }

        private void ASL(ushort address, AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.Accumulator)
            {
                // ASL A
                CheckFlagC(register.A >> 7 != 0);
                register.A <<= 1;
                CheckFlagZS(register.A);
            }
            else
            {
                var m = ReadByte(address);
                CheckFlagC(m >> 7 != 0);
                m <<= 1;
                WriteByte(address, m);
                CheckFlagZS(m);
            }
        }

        private void LSR(ushort address, AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.Accumulator)
            {
                // LSR A
                CheckFlagC((register.A & 1) != 0);
                register.A >>= 1;
                CheckFlagZS(register.A);
            }
            else
            {
                var m = ReadByte(address);
                CheckFlagC((m & 1) != 0);
                m >>= 1;
                WriteByte(address, m);
                CheckFlagZS(m);
            }
        }

        private void ROL(ushort address, AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.Accumulator)
            {
                // ROL A
                ushort m = (ushort)(register.A << 1);
                if (register.C > 0)
                {
                    m |= 1;
                }
                CheckFlagC(m > 0xFF);
                register.A = (byte)m;
                CheckFlagZS(register.A);
            }
            else
            {
                ushort m = (ushort)(ReadByte(address) << 1);
                if (register.C > 0)
                {
                    m |= 1;
                }
                CheckFlagC(m > 0xFF);
                WriteByte(address, (byte)m);
                CheckFlagZS((byte)m);
            }
        }

        private void ROR(ushort address, AddressingMode addressingMode)
        {
            if (addressingMode == AddressingMode.Accumulator)
            {
                // ROR A
                ushort m = register.A;
                if (register.C > 0)
                {
                    m |= 0x100;
                }
                CheckFlagC((m & 1) != 0);
                m = (ushort)(m >> 1);
                register.A = (byte)m;
                CheckFlagZS(register.A);
            }
            else
            {
                ushort m = ReadByte(address);
                if (register.C > 0)
                {
                    m |= 0x100;
                }
                CheckFlagC((m & 1) != 0);
                m = (ushort)(m >> 1);
                WriteByte(address, (byte)m);
                CheckFlagZS((byte)m);
            }
        }

        private void PHA()
        {
            Push(register.A);
        }

        private void PLA()
        {
            register.A = Pull();
            CheckFlagZS(register.A);
        }

        private void PHP()
        {
            Push((byte)((byte)register.P | (byte)StatusRegisterFlag.B | (byte)StatusRegisterFlag.R));
        }

        private void PLP()
        {
            register.P = (StatusRegisterFlag)Pull();
            register.R = 1;
            register.B = 0;
        }

        private void JMP(ushort address)
        {
            register.PC = address;
        }

        private void BEQ(ushort address)
        {
            if (register.Z != 0)
                register.PC = address;
        }

        private void BNE(ushort address)
        {
            if (register.Z == 0)
                register.PC = address;
        }

        private void BCS(ushort address)
        {
            if (register.C != 0)
                register.PC = address;
        }

        private void BCC(ushort address)
        {
            if (register.C == 0)
                register.PC = address;
        }

        private void BMI(ushort address)
        {
            if (register.S != 0)
                register.PC = address;
        }

        private void BPL(ushort address)
        {
            if (register.S == 0)
                register.PC = address;
        }

        private void BVS(ushort address)
        {
            if (register.V != 0)
                register.PC = address;
        }

        private void BVC(ushort address)
        {
            if (register.V == 0)
                register.PC = address;
        }

        private void JSR(ushort address)
        {
            register.PC--;
            Push((byte)(register.PC >> 8));
            Push((byte)(register.PC & 0xFF));
            register.PC = address;
        }

        private void RTS()
        {
            register.PC = (ushort)(Pull() | (Pull() << 8));
            register.PC++;
        }

        private void NOP()
        { 
            
        }

        private void BRK()
        {
            register.PC++;
            Push((byte)(register.PC >> 8));
            Push((byte)(register.PC & 0xFF));
            Push((byte)((byte)register.P | (byte)StatusRegisterFlag.B | (byte)StatusRegisterFlag.R));
            register.I = 1;
            register.PC = ReadAddress((ushort)InterruptVector.IRQ_BRK);
        }

        private void RTI()
        {
            register.P = (StatusRegisterFlag)Pull();
            register.R = 1;
            register.B = 0;

            register.PC = (ushort)(Pull() | (Pull() << 8));
        }

        private void ALR(ushort address)
        {
            register.A &= ReadByte(address);
            CheckFlagC((register.A & 1) != 0);
            register.A >>= 1;
            CheckFlagZS(register.A);
        }

        private void ANC(ushort address) 
        {
            register.A &= ReadByte(address);
            CheckFlagZ(register.A);
            register.C = register.S;
        }

        private void ARR(ushort address)
        {
            register.A &= ReadByte(address);
            register.A = (byte)((register.A >> 1) | (register.C << 7));
            CheckFlagZS(register.A);
            CheckFlagC((register.A >> 6) != 0);
            CheckFlagV((((register.A >> 5) ^ (register.A >> 6)) & 1) != 0);
        }

        private void AXS(ushort address)
        { 
            var m = (register.A & register.X) - ReadByte(address);
            register.X = (byte)m;
            CheckFlagZS(register.X);
            CheckFlagC((m & 0x8000) == 0);
        }

        private void LAX(ushort address)
        {
            register.A = register.X = ReadByte(address);
            CheckFlagZS(register.A);
        }

        private void SAX(ushort address)
        {
            WriteByte(address, (byte)(register.A & register.X));
        }

        private void DCP(ushort address)
        {
            var m = ReadByte(address);
            m--;
            WriteByte(address, m);

            ushort r16 = (ushort)((ushort)register.A - (ushort)m);
            CheckFlagC(r16 < 0x100);
            CheckFlagZS((byte)r16);
        }

        private void ISC(ushort address)
        {
            var m = ReadByte(address);
            m++;
            WriteByte(address, m);
            ushort r16 = (ushort)((ushort)register.A - (ushort)m - (1 - register.C));
            CheckFlagC(r16 >> 8 == 0);
            CheckFlagV(((register.A ^ m) & 0x80) != 0 && ((register.A ^ (byte)r16) & 0x80) != 0);
            register.A = (byte)r16;
            CheckFlagZS(register.A);
        }

        private void RLA(ushort address)
        {
            var src = ReadByte(address);
            src <<= 1;
            if (register.C != 0)
            {
                src |= 0x1;
            }
            CheckFlagC(src > 0xFF);
            var r8 = (byte)src;
            WriteByte(address, r8);

            register.A &= r8;
            CheckFlagZS(register.A);
        }

        private void RRA(ushort address)
        {
            ushort src = ReadByte(address);
            if (register.C != 0)
            {
                src |= 0x100;
            }
            CheckFlagC((src & 1) != 0);
            src >>= 1;
            WriteByte(address, (byte)src);

            ushort r16 = (ushort)(register.A + src + register.C);
            CheckFlagC((r16 >> 8) != 0);
            CheckFlagV(((register.A ^ src) & 0x80) == 0 && ((register.A ^ (byte)r16) & 0x80) != 0);
            register.A = (byte)r16;
            CheckFlagZS(register.A);
        }

        private void SLO(ushort address)
        {
            var temp = ReadByte(address);
            CheckFlagC(temp >> 7 != 0);
            temp <<= 1;
            WriteByte(address, temp);

            register.A |= temp;
            CheckFlagZS(register.A);
        }

        private void SRE(ushort address)
        {
            var temp = ReadByte(address);
            CheckFlagC((temp & 1) != 0);
            temp >>= 1;
            WriteByte(address, temp);

            register.A ^= temp;
            CheckFlagZS(register.A);
        }

        private void SHX()
        {
            throw new NotImplementedException();
        }

        private void SHY()
        {
            throw new NotImplementedException();
        }

        private void LAS()
        {
            throw new NotImplementedException();
        }

        private void XAA()
        {
            throw new NotImplementedException();
        }

        private void AHX()
        {
            throw new NotImplementedException();
        }

        private void TAS()
        {
            throw new NotImplementedException();
        }

        private void STP()
        {
            throw new NotImplementedException();
        }
    }
}
