using FamicomSimulator.Config;

namespace FamicomSimulator.Util
{
    internal static class FileUtil
    {
        public static (ErrorCode, RomInfo?) LoadRom(string path)
        {
            if (!System.IO.File.Exists(path))
            { 
                return (ErrorCode.FILE_NOT_FOUND, null);
            }

            try
            {
                var data = File.ReadAllBytes(path);
                var romInfo = new RomInfo(data);
                return (ErrorCode.OK, romInfo);
            }
            catch (InvalidDataException)
            {
                return (ErrorCode.FILE_ILLEGAL, null);
            }
            catch (Exception)
            {
                return (ErrorCode.FILE_NOT_FOUND, null);
            }
        }
    }
}
