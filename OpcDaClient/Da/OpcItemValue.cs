using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Da
{
    public class OpcItemValue
    {
        public string Name { get; set; } = "";
        public OpcDaAsync.Comn.OpcDataType DataType { get; set; } = OpcDaAsync.Comn.OpcDataType.Int;
        public object Value { get; set; } = 0;
        public DateTime Time { get; set; }
        public short Quality { get; set; }
    }
}
