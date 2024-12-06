using FamicomSimulator.Config;
using FamicomSimulator.Util;

namespace FamicomSimulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            (var ErrorCode, var romInfo) = FileUtil.LoadRom("C:\\Develop\\FamicomSimulator\\Others\\nestest.nes");
            LogUtil.Log($"Error: {ErrorCode}");
        }
    }
}
