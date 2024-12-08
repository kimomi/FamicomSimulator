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
            fc.Cpu.register.PC = 0xC000;
            var testFileLines = File.ReadAllLines("../../../Others/nestest.log");
            for (int i = 0; i < 1000; i++)
            {
                var cpuState = fc.Cpu.ToString();
                LogUtil.Log($"         {cpuState}");
                LogUtil.Log(testFileLines[i]);
                var myAsm = cpuState.Substring(7, 19).Trim();
                var refAsm = testFileLines[i].Substring(16, 19).Trim();
                if (myAsm != refAsm && 
                    !(refAsm.Contains('=') && refAsm.Split('=')[0].Trim() == myAsm))
                {
                    LogUtil.Error($"myAsm:{myAsm}, refAsm:{refAsm}");
                }
                fc.Tick();
            }

            Console.ReadKey();
        }
    }
}
