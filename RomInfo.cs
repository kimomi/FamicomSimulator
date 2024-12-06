using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator
{
    internal class RomInfo
    {
        public required byte[] DataPrgRom; // 程序只读存储器
        public required byte[] DataChrRom; // 角色只读存储器
        public uint CountPrgRom16kb; // 程序只读存储器16kb的数量
        public uint CountChrRom8kb; // 角色只读存储器8kb的数量

        public byte MapperNumber; // 映射器编号
        public byte VMirroring; // 是否竖直镜像（否为水平）
        public byte FourScreen; // 是否四屏模式
        public byte SaveRam; // 是否有SRAM(电池供电)

        public uint Reserved; // 保留以对齐
    }
}
