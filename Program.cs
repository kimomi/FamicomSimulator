using FamicomSimulator.Config;
using FamicomSimulator.Util;

namespace FamicomSimulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            (var ErrorCode, var romInfo) = FileUtil.LoadRom("../../../Others/nestest.nes");
            LogUtil.Log($"ErrorCode: {ErrorCode}");
            if (romInfo != null)
            {
                LogUtil.Log($"ROM PRG-ROM:{romInfo.DataPrgRom.Length / 1024 / 16} * 16 KB, CHR-ROM:{romInfo.DataChrRom.Length / 1024 / 8} * 8 KB");
            }
            Console.ReadKey();
        }
    }
}
