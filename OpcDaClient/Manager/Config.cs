using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Manager
{
    internal class Config
    {
        public string OpcServer { get; set; } = "";
        public string Host { get; set; } = "127.0.0.1";
        public Group[] Groups { get; set; } = new Group[0];
        public class Item
        {
            public string Name { get; set; } = "";
            public Comn.OpcDataType DataType { get; set; } = Comn.OpcDataType.Int;
        }
        public class Group
        {
            public string Name { get; set; } = Guid.NewGuid().ToString();
            public bool Active { get; set; } = true;
            public float DeadBand { get; set; } = 0.0f;
            public int UpdateRate { get; set; } = 1000;
            public Item[] Items { get; set; } = new Item[0];
        }
    }
}
