using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Core
{
    public class Famicom
    {
        public Cpu Cpu = new Cpu();
        public RomInfo? RomInfo;

        public void LoadROM(RomInfo romInfo)
        {
            RomInfo = romInfo;
            Cpu.LoadROM(romInfo);
        }
    }
}
