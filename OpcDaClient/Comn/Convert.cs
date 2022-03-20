using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Comn
{
    public static class Convert
    {
        public static DateTime FileTimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME ft)
        {
            long highbuf = ft.dwHighDateTime;
            long buffer = (highbuf << 32) + ft.dwLowDateTime;
            //return DateTime.FromFileTimeUtc(buffer);
            return DateTime.FromFileTime(buffer);
        }

    }
}
