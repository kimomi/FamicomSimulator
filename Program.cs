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
            for (int i = 0; i < testFileLines.Length; i++)
            {
                var cpuState = fc.Cpu.ToString();
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
                fc.Tick();
            }
            LogUtil.Log("Finish Test!");

            Console.ReadKey();
        }
    }
}
