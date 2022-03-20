using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaAsync.Da
{
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
