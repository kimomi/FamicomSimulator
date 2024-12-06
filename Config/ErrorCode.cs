using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Config
{
    public enum ErrorCode
    {
        OK = 0,

        FILE_NOT_FOUND = 100,
        FILE_ILLEGAL,
        FILE_ILLEGAL_HEADER,
    }
}
