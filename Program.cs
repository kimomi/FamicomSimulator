using FamicomSimulator.Config;
using FamicomSimulator.Core;
using FamicomSimulator.Util;
using OpenTK.Platform;

namespace FamicomSimulator
{
    internal class Program
    {
        private static Famicom _fc = new Famicom();

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
            _fc.LoadROM(romInfo);

            WindowUtil.Show(Famicom.WIDTH * 2, Famicom.HEIGHT * 2, "FamicomSimulator", Update);
        }

        private static void Update()
        {
            _fc.Tick();

            WindowUtil.DrawData(Famicom.WIDTH, Famicom.HEIGHT, _fc.GraphicData);
        }
    }
}
