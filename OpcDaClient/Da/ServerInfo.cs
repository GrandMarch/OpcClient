using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Da
{
    public class ServerInfo
    {
        public DateTime StartTime { get; internal set; } = new DateTime(0);
        public DateTime CurrentTime { get; internal set; } = new DateTime(0);
        public DateTime LastUpdateTime { get; internal set; } = new DateTime(0);
        public OpcRcw.Da.OPCSERVERSTATE ServerState { get; internal set; } = OpcRcw.Da.OPCSERVERSTATE.OPC_STATUS_NOCONFIG;
        public string Version { get; internal set; } = "UNKOWN";
        public string VendorInfo { get; internal set; } = "UNKOWN";
    }
}
