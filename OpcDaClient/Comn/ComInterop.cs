using OpcDaClient.Da;
using OpcRcw.Comn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Comn
{
    internal class ComInterop
    {
        private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        private struct COSERVERINFO
        {
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszName;
            public IntPtr pAuthInfo;
            public uint dwReserved2;
        };
        private struct MULTI_QI
        {
            public IntPtr iid;
            [MarshalAs(UnmanagedType.IUnknown)]
            public object? pItf;
            public uint hr;
        }

        [DllImport("ole32.dll")]
        private static extern void CoCreateInstanceEx(ref Guid clsid,
                                                    [MarshalAs(UnmanagedType.IUnknown)] object punkOuter,
                                                    uint dwClsCtx,
                                                    [In] ref COSERVERINFO pServerInfo,
                                                    uint dwCount,
                                                    [In, Out] MULTI_QI[] pResults);
        /// <summary>
        /// Creates an instance of a COM server.
        /// </summary>
        public static object? CreateInstance(Guid clsid, string hostName/*, NetworkCredential credential*/)
        {
            COSERVERINFO coserverInfo = new();
            coserverInfo.pwszName = hostName;
            coserverInfo.pAuthInfo = IntPtr.Zero;
            coserverInfo.dwReserved1 = 0;
            coserverInfo.dwReserved2 = 0;

            GCHandle hIID = GCHandle.Alloc(IID_IUnknown, GCHandleType.Pinned);

            MULTI_QI[] results = new MULTI_QI[1];

            results[0].iid = hIID.AddrOfPinnedObject();
            results[0].pItf = null;
            results[0].hr = 0;

            try
            {
                // check whether connecting locally or remotely.
                uint clsctx = 0x01 | 0x04;

                if (hostName != null && hostName.Length > 0 && hostName != "localhost")
                {
                    clsctx = 0x04 | 0x10;
                }

                // create an instance.
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
                CoCreateInstanceEx(
                    ref clsid,
                    null,
                    clsctx,
                    ref coserverInfo,
                    1,
                    results);
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (hIID.IsAllocated) hIID.Free();
            }

            if (results[0].hr != 0)
            {
                throw new ExternalException("CoCreateInstanceEx: " + (int)results[0].hr);
            }
            return results[0].pItf;
        }
        public static void RealseComServer(object m_server)
        {
            if (m_server != null && m_server.GetType().IsCOMObject)
            {
#pragma warning disable CA1416 // 验证平台兼容性
                Marshal.ReleaseComObject(m_server);
#pragma warning restore CA1416 // 验证平台兼容性
            }
        }

        /// <summary>
        /// Reads the guids from the enumerator.
        /// </summary>
        public static Guid[] ReadClasses(IOPCEnumGUID enumerator)
        {
            List<Guid> guids = new List<Guid>();

            int fetched = 0;
            Guid[] buffer = new Guid[10];

            do
            {
                try
                {
                    IntPtr pGuids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Guid)) * buffer.Length);

                    try
                    {
                        enumerator.Next(buffer.Length, pGuids, out fetched);

                        if (fetched > 0)
                        {
                            IntPtr pos = pGuids;

                            for (int ii = 0; ii < fetched; ii++)
                            {
                                object? o = Marshal.PtrToStructure(pos, typeof(Guid));
                                if (o != null)
                                {

                                    buffer[ii] = (Guid)o;
                                }
                                //pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(Guid)));
                                pos = IntPtr.Add(pos, Marshal.SizeOf(typeof(Guid)));// (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(Guid)));
                                guids.Add(buffer[ii]);
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pGuids);
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
            while (fetched > 0);

            return guids.ToArray();
        }
    }
}
