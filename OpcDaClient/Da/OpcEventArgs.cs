using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Da
{
    public delegate void OnDataChangedHandler(ItemReadResult[] opcItems);
    public delegate void OnWriteCompletedHandler(ItemWriteResult[] opcItems);
    public delegate void OnReadCompletedHandler(ItemReadResult[] opcItems);
    public class ItemReadResult
    {
        public string Name { get; set; } = "";
        public Comn.OpcDataType DataType { get; set; } = Comn.OpcDataType.Int;
        public object Value { get; set; } = 0;
        public DateTime TimeStamp { get; set; }
        public short Quality { get; set; }
    }
    public class ItemWriteResult
    {
        public string Name { get; set; } = "";
        public int Error { get; set; } = 0;
    }

    public class OpcEventArgs: EventArgs
    {
        /// <summary>
        /// group handel
        /// </summary>
        public int GroupHandle { get; set; }
        /// <summary>
        /// item lenght
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// item value
        /// </summary>
        public object[]? Values { get; set; }
        /// <summary>
        /// error info
        /// </summary>
        public int[]? Errors { get; set; }
        /// <summary>
        /// items handel
        /// </summary>
        public int[]? ClientItemsHandle { get; set; }
    }
}
