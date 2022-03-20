using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaAsync.Da
{

    public interface IOpcItem
    {

    }
    public class OpcItem
    {
        private static int _hanle = 0;
        /// <summary>
        /// item name,you can give a shorter or human kindly name
        /// </summary>
        public string Name { get; private set; } = "";
        /// <summary>
        /// mostly blank
        /// </summary>
        public string AccessPath { get; private set; } = "";
        /// <summary>
        /// item的数据类型
        /// </summary>
        public Comn.OpcDataType DataType { get; private set; }
        /// <summary>
        /// 数据项在opc server的完全名称
        /// </summary>
        public string ItemID { get; private set; }
        /// <summary>
        /// active(1) or not(0)
        /// </summary>
        public bool IsActive { get; set; } = true;
        public int BlobSize { get; set; } = 0;
        public IntPtr Blob { get; set; } = IntPtr.Zero;
        public object? Value { get; set; }
        public int ClientHandle { get; private set; }
        public int ServerHandle { get; set; }
        public OpcItem(string itemId, Comn.OpcDataType dataType)
        {
            Name = itemId;
            ItemID = itemId;
            DataType = dataType;
            ClientHandle = ++_hanle;
        }
        public OpcItem(string name, string itemId, Comn.OpcDataType dataType) : this(itemId, dataType)
        {
            Name = name;
        }

    }
}
