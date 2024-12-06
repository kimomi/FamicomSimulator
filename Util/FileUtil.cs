using FamicomSimulator.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                using var f = new FileStream(path, FileMode.Open, FileAccess.Read);

                // header
                if (f.ReadByte() != 'N')
                {
                    return (ErrorCode.FILE_ILLEGAL_HEADER, null);
                }
                if (f.ReadByte() != 'E')
                {
                    return (ErrorCode.FILE_ILLEGAL_HEADER, null);
                }
                if (f.ReadByte() != 'S')
                {
                    return (ErrorCode.FILE_ILLEGAL_HEADER, null);
                }
                if (f.ReadByte() != 0x1A)
                {
                    return (ErrorCode.FILE_ILLEGAL_HEADER, null);
                }

                // prgROM size
                var prgRomSize = f.ReadByte() * 16 * 1024;
                // chrROM size
                var chrRomSize = f.ReadByte() * 8 * 1024;

                LogUtil.Log($"prgRomSize: {prgRomSize} byte, chrRomSize: {chrRomSize} byte");

                var prgRom = new byte[prgRomSize];
                var chrRom = new byte[chrRomSize];
            }
            catch (Exception)
            {
                return (ErrorCode.FILE_ILLEGAL, null);
            }

            return (ErrorCode.OK, null);
        }
    }
}
