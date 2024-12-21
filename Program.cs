using FamicomSimulator.Config;
using FamicomSimulator.Core;
using FamicomSimulator.Util;

namespace FamicomSimulator
{
    internal class Program
    {
        private static bool _exit = false;

        public static void Main()
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

            Console.CancelKeyPress += ConsoleCancelKeyPress;
            while (!_exit)
            {
                fc.Tick();
            }
            LogUtil.Log("Exit Loop!");
        }

        private static void ConsoleCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _exit = true;
        }
    }
}
