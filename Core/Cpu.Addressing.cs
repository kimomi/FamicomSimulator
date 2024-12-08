using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Core
{
    internal partial class Cpu
    {
        private void AddressingImmediate(out ushort address)
        {
            address = register.PC;
            register.PC++;
        }

        private void AddressingZeroPage(out ushort address)
        {
            address = ReadByte(register.PC);
            register.PC++;
        }

        private void AddressingZeroPageX(out ushort address)
        {
            address = (ushort)(ReadByte(register.PC) + register.X);
            register.PC++;
            address = (ushort)(address & 0xFF);
        }

        private void AddressingZeroPageY(out ushort address)
        {
            address = (ushort)(ReadByte(register.PC) + register.Y);
            register.PC++;
            address = (ushort)(address & 0xFF);
        }

        private void AddressingAbsolute(out ushort address)
        {
            address = ReadByte(register.PC);
            register.PC++;
            address |= (ushort)(ReadByte(register.PC) << 8);
            register.PC++;
        }

        private void AddressingAbsoluteX(out ushort address)
        {
            address = ReadByte(register.PC);
            register.PC++;
            address |= (ushort)(ReadByte(register.PC) << 8);
            register.PC++;
            address += register.X;
        }

        private void AddressingAbsoluteY(out ushort address)
        {
            address = ReadByte(register.PC);
            register.PC++;
            address |= (ushort)(ReadByte(register.PC) << 8);
            register.PC++;
            address += register.Y;
        }

        private void AddressingIndirect(out ushort address)
        {
            ushort a = ReadByte(register.PC);
            register.PC++;
            ushort b = ReadByte(register.PC);
            register.PC++;

            // 硬件bug也实现一下, cPlus1的高位是C的高位, 低位是C+1的低位
            ushort c = (ushort)(a | (b << 8));
            ushort cPlus1 = (ushort)((c & 0xFF00) | ((c + 1) & 0x00FF));
            address = (ushort)(ReadByte(c) | (ReadByte(cPlus1) << 8));
        }

        private void AddressingIndirectX(out ushort address)
        {
            var a = (byte)(ReadByte(register.PC) + register.X);
            register.PC++;
            address = (ushort)(ReadByte(a) | (ReadByte((byte)(a + 1)) << 8));
        }

        private void AddressingIndirectY(out ushort address)
        {
            var a = ReadByte(register.PC);
            register.PC++;
            address = (ushort)(ReadByte(a) | (ReadByte((byte)(a + 1)) << 8));
            address += register.Y;
        }

        private void AddressingRelative(out ushort address)
        {
            var a = ReadByte(register.PC);
            register.PC++;
            address = (ushort)(register.PC + (sbyte)a);
        }

        private string GetCurrentAssembleString()
        {
            OpConfig opConfig = AssembleCode.OpCodeArrangement[ReadByte(register.PC)];
            switch (opConfig.AddressingMode)
            {
                case AddressingMode.Implicit:
                    return opConfig.Name;
                case AddressingMode.Immediate:
                    return $"{opConfig.Name} #${ReadByte((ushort)(register.PC + 1)):X2}";
                case AddressingMode.ZeroPage:
                    return $"{opConfig.Name} ${ReadByte((ushort)(register.PC + 1)):X2}";
                case AddressingMode.ZeroPageX:
                    return $"{opConfig.Name} ${ReadByte((ushort)(register.PC + 1)):X2},X";
                case AddressingMode.ZeroPageY:
                    return $"{opConfig.Name} ${ReadByte((ushort)(register.PC + 1)):X2},Y";
                case AddressingMode.Absolute:
                    var rAbsolute = ReadByte((ushort)(register.PC + 1)) | (ReadByte((ushort)(register.PC + 2)) << 8);
                    return $"{opConfig.Name} ${rAbsolute:X4}";
                case AddressingMode.AbsoluteX:
                    var rAbsoluteX = ReadByte((ushort)(register.PC + 1)) | (ReadByte((ushort)(register.PC + 2)) << 8);
                    return $"{opConfig.Name} ${rAbsoluteX:X4},X";
                case AddressingMode.AbsoluteY:
                    var rAbsoluteY = ReadByte((ushort)(register.PC + 1)) | (ReadByte((ushort)(register.PC + 2)) << 8);
                    return $"{opConfig.Name} ${rAbsoluteY:X4},Y";
                case AddressingMode.IndirectX:
                    var rIndirectX = ReadByte((ushort)(register.PC + 1));
                    return $"{opConfig.Name} (${rIndirectX:X2},X)";
                case AddressingMode.IndirectY:
                    var rIndirectY = ReadByte((ushort)(register.PC + 1));
                    return $"{opConfig.Name} (${rIndirectY:X2}),Y";
                case AddressingMode.Relative:
                    var rRelative = (sbyte)ReadByte((ushort)(register.PC + 1)) + register.PC + 2;
                    return $"{opConfig.Name} ${rRelative:X4}";
                case AddressingMode.Accumulator:
                    return $"{opConfig.Name} A";
                case AddressingMode.Indirect:
                    var rIndirect = ReadByte((ushort)(register.PC + 1)) | (ReadByte((ushort)(register.PC + 2)) << 8);
                    return $"{opConfig.Name} (${rIndirect:X4})";
                default:
                    return opConfig.Name;
            }
        }
    }
}
