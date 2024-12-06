using FamicomSimulator.Config;
using FamicomSimulator.Core;
using FamicomSimulator.Util;

namespace FamicomSimulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // load rom file
            (var ErrorCode, var romInfo) = FileUtil.LoadRom("../../../Others/nestest.nes");
            LogUtil.Log($"ErrorCode: {ErrorCode}");
            if (romInfo == null)
            {
                Console.ReadKey();
                return;
            }
            LogUtil.Log($"ROM PRG-ROM:{romInfo.DataPrgRom.Length / 1024 / 16} * 16 KB, CHR-ROM:{romInfo.DataChrRom.Length / 1024 / 8} * 8 KB, Mapper:{romInfo.Header.MapperNumber}");
            
            // load rom to cpu
            Famicom fc = new Famicom();
            fc.LoadROM(romInfo);
            var nmiAddress = fc.Cpu.GetInterruptAddress(Cpu.InterruptVector.NMI);
            var resetAddress = fc.Cpu.GetInterruptAddress(Cpu.InterruptVector.RESET);
            var irqbrkAddress = fc.Cpu.GetInterruptAddress(Cpu.InterruptVector.IRQ_BRK);
            LogUtil.Log($"NMI:${nmiAddress:X}, RESET:${resetAddress:X}, IRQ/BRK:${irqbrkAddress:X}");
            LogUtil.Log($"RESET OP Code:0x{fc.Cpu.ReadByte(resetAddress):X}");
            
            Console.ReadKey();
        }
    }
}
