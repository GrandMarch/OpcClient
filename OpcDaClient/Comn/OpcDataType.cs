using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaAsync.Comn
{
    public enum OpcDataType:short
    {
        Boolean =11,
        Byte =17,
        Short=2,
        Word=18,
        Int =3,
        UInt =19,
        Float=4,
        Double=5,
        Long=20,
        String=8
    }
}
