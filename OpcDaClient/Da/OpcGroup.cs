

using System.Runtime.InteropServices;


namespace OpcDaAsync.Da
{
    public class OpcGroup : OpcRcw.Da.IOPCDataCallback, IDisposable
    {
        private OpcRcw.Da.IOPCGroupStateMgt? m_StateManagement = null;
        private OpcRcw.Comn.IConnectionPointContainer? m_ConnectionPointContainer = null;
        private OpcRcw.Comn.IConnectionPoint? m_ConnectionPoint = null;
        private OpcRcw.Da.IOPCSyncIO? m_SyncIO = null;
        private OpcRcw.Da.IOPCAsyncIO2? m_Async2IO = null;
        private OpcRcw.Da.IOPCItemMgt? m_ItemManagement = null;
        private int m_connectionpoint_cookie = 0;

        #region public
        public string Name { get; private set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int RequestUpdateRate { get; set; } = 1000;
        public int RevisedUpdateRate
        {
            get { return revisedUpdateRate; }
            set { revisedUpdateRate = value; }
        }
        public int ClientGroupHandle { get; private set; }
        public int ServerGroupHandle
        {
            get { return serverGroupHandle; }
            set { serverGroupHandle = value; }
        }
        public float DeadBand { get; set; } = 0.0f;
        public GCHandle TimeBias
        {
            get
            {
                return timeBias;
            }
            set
            {
                timeBias = value;
            }
        }
        public GCHandle PercendDeadBand
        {
            get
            {
                return percendDeadBand;
            }
            set
            {
                percendDeadBand = value;
            }
        }
        public int LCID
        {
            get
            {
                return lcid;
            }
            set
            {
                lcid = value;
            }
        }
        public object? GroupPointer
        {
            get { return groupPointer; }
            set { groupPointer = value; }
        }

        private bool _bSubscribe = false;
        /// <summary>
        /// active subscribe
        /// </summary>
        public bool ActiveSubscribe
        {
            get
            {
                return _bSubscribe;
            }
            set
            {
                _bSubscribe = value;
                ActiveDataChanged(_bSubscribe);
            }
        }

        #endregion

        #region internal
        internal object? groupPointer = 0;
        internal int revisedUpdateRate = 0;
        internal int serverGroupHandle = 0;

        #endregion

        #region private 

        private static int _handle = 0;
        private int lcid = 0x407;
        private GCHandle timeBias = GCHandle.Alloc(0, GCHandleType.Pinned);
        private GCHandle percendDeadBand = GCHandle.Alloc(0, GCHandleType.Pinned);
        //private Guid riid = typeof(IOPCItemMgt).GUID;
        /// <summary>
        /// items
        /// </summary>
        private List<OpcItem> opcItems = new List<OpcItem> { };
        private bool disposedValue;

        #endregion

        #region Event

        public delegate void OnDataChangedHandler(OpcItem[] opcItems);
        /// <summary>
        /// datachange subscribe
        /// </summary>
        //public event EventHandler<OpcEventArgs>? OnDataChanged;\
        public event OnDataChangedHandler OnDataChanged;
        /// <summary>
        /// write async
        /// </summary>
        public event EventHandler<OpcEventArgs>? OnWriteCompleted;
        /// <summary>
        /// read async
        /// </summary>
        public event EventHandler<OpcEventArgs>? OnReadCompleted;

        #endregion
        internal OpcGroup(string name)
        {
            Name = name;
            ClientGroupHandle = ++_handle;

        }
        internal OpcGroup(string groupName, bool active, int reqUpdateRate, float deadBand)
        {
            Name = groupName;
            IsActive = active;
            RequestUpdateRate = reqUpdateRate;
            DeadBand = deadBand;
            ClientGroupHandle = ++_handle;
        }
        /// <summary>
        /// add opc items
        /// </summary>
        /// <param name="opcGroup"></param>
        public void AddOpcItem(OpcItem[] items)
        {

            IntPtr pResults = IntPtr.Zero;
            IntPtr pErrors = IntPtr.Zero;
            OpcRcw.Da.OPCITEMDEF[] itemDefyArray = new OpcRcw.Da.OPCITEMDEF[items.Length];
            int i = 0;
            int[] errors = new int[items.Length];
            int[] itemServerHandle = new int[items.Length];
            try
            {
                foreach (OpcItem item in items)
                {
                    if (item != null)
                    {
                        itemDefyArray[i].szAccessPath = item.AccessPath;
                        itemDefyArray[i].szItemID = item.ItemID;
                        itemDefyArray[i].bActive = item.IsActive ? 1 : 0;
                        itemDefyArray[i].hClient = item.ClientHandle;
                        itemDefyArray[i].dwBlobSize = item.BlobSize;
                        itemDefyArray[i].pBlob = item.Blob;
                        itemDefyArray[i].vtRequestedDataType = (short)item.DataType;
                        i++;
                    }
                }
                //添加OPC项组
                m_ItemManagement?.AddItems(items.Length, itemDefyArray, out pResults, out pErrors);
                IntPtr Pos = pResults;
                Marshal.Copy(pErrors, errors, 0, items.Length);
                for (int j = 0; j < items.Length; j++)
                {
                    if (errors[j] == 0)
                    {
                        object? o = Marshal.PtrToStructure(Pos, typeof(OpcRcw.Da.OPCITEMRESULT));
                        if (o != null)
                        {
                            var result = (OpcRcw.Da.OPCITEMRESULT)o;
                            itemServerHandle[j] = items[j].ServerHandle = result.hServer;
                            Marshal.DestroyStructure(Pos, typeof(OpcRcw.Da.OPCITEMRESULT));
                            opcItems.Add(items[j]);
                        }
                        Pos = new IntPtr(Pos.ToInt32() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMRESULT)));

                    }
                }
            }
            catch (COMException)
            {
                throw;
            }
            finally
            {
                if (pResults != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pResults);
                }
                if (pErrors != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pErrors);
                }
            }
        }

        /// <summary>
        /// async read
        /// </summary>
        public void ReadAsync()
        {
            IntPtr pErrors = IntPtr.Zero;
            try
            {
                if (m_Async2IO != null)
                {
                    int[] serverHandle = new int[opcItems.Count];
                    int[] PErrors = new int[opcItems.Count];
                    for (int j = 0; j < opcItems.Count; j++)
                    {
                        serverHandle[j] = opcItems[j].ServerHandle;
                    }
                    int cancelId = 0;
                    m_Async2IO.Read(opcItems.Count, serverHandle, 2, out cancelId, out pErrors);
                    Marshal.Copy(pErrors, PErrors, 0, opcItems.Count);
                }
            }
            catch (COMException)
            {
                throw;
            }
            finally
            {
                if (pErrors != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(pErrors);
                }
            }
        }
        /// <summary>
        /// async write
        /// </summary>
        /// <param name="values"></param>
        /// <param name="serverHandle"></param>
        /// <param name="errors"></param>
        /// <param name="opcGroup"></param>
        public void WriteAsync(object[] values, int[] serverHandle, out int[] errors, OpcGroup opcGroup)
        {
            IntPtr pErrors = IntPtr.Zero;
            errors = new int[values.Length];
            if (m_Async2IO != null)
            {
                try
                {
                    int cancelId, transactionID = 0;
                    m_Async2IO.Write(values.Length, serverHandle, values, transactionID, out cancelId, out pErrors);
                    Marshal.Copy(pErrors, errors, 0, values.Length);
                }
                catch (COMException)
                {
                    throw;
                }
                finally
                {
                    if (pErrors != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pErrors);
                    }
                }
            }
        }
        public void ReadSync()
        {
            //TODO sync read
        }
        public void WriteSync()
        {
            //TODO sync write
        }
        internal void InitIoInterfaces(object handle)
        {
            groupPointer = handle;
            //
            m_ItemManagement = (OpcRcw.Da.IOPCItemMgt)groupPointer;
            m_Async2IO = (OpcRcw.Da.IOPCAsyncIO2)groupPointer;
            m_SyncIO = (OpcRcw.Da.IOPCSyncIO)groupPointer;
            //group state object
            m_StateManagement = (OpcRcw.Da.IOPCGroupStateMgt)groupPointer;
            m_ConnectionPointContainer = (OpcRcw.Comn.IConnectionPointContainer)groupPointer;
            Guid iid = typeof(OpcRcw.Da.IOPCDataCallback).GUID;
            m_ConnectionPointContainer.FindConnectionPoint(ref iid, out m_ConnectionPoint);
            //创建客户端与服务端之间的连接
            m_ConnectionPoint.Advise(this, out m_connectionpoint_cookie);
        }

        /// <summary>
        /// Active/DeActive datachange callback
        /// </summary>
        private void ActiveDataChanged(bool active)
        {
            IntPtr pRequestedUpdateRate = IntPtr.Zero;
            IntPtr hClientGroup = IntPtr.Zero;
            IntPtr pTimeBias = IntPtr.Zero;
            IntPtr pDeadband = IntPtr.Zero;
            IntPtr pLCID = IntPtr.Zero;
            int nActive = 0;
            GCHandle hActive = GCHandle.Alloc(nActive, GCHandleType.Pinned);
            hActive.Target = active ? 1 : 0;
            try
            {
                hActive.Target = 1;
                int nRevUpdateRate = 0;
                m_StateManagement?.SetState(pRequestedUpdateRate,
                                            out nRevUpdateRate,
                                            hActive.AddrOfPinnedObject(),
                                            pTimeBias,
                                            pDeadband,
                                            pLCID,
                                            hClientGroup);
            }
            catch (COMException)
            {
                throw;
            }
            finally
            {
                hActive.Free();
            }
        }

        #region OpcRcw.Da.IOPCDataCallback
        public void OnDataChange(int dwTransid,
                                int hGroup,
                                int hrMasterquality,
                                int hrMastererror,
                                int dwCount,
                                int[] phClientItems,
                                object[] pvValues,
                                short[] pwQualities,
                                System.Runtime.InteropServices.ComTypes.FILETIME[] pftTimeStamps,
                                int[] pErrors)
        {
            if (OnDataChanged != null)
            {
                for (int i = 0; i < dwCount; i++)
                {
                    int index = opcItems.FindIndex(x => x.ClientHandle == phClientItems[i]);
                    if (index >= 0)
                    {
                        opcItems[index].Value=pvValues[i];
                        opcItems[index].Quality=pwQualities[i];
                        opcItems[index].TimeStamp= OpcDaClient.Comn.Convert.FileTimeToDateTime(pftTimeStamps[i]);
                    }
                }
            }
            Console.WriteLine("-==========OnDataChange Event==========-");

        }

        public void OnReadComplete(int dwTransid,
                                    int hGroup,
                                    int hrMasterquality,
                                    int hrMastererror,
                                    int dwCount,
                                    int[] phClientItems,
                                    object[] pvValues,
                                    short[] pwQualities,
                                    System.Runtime.InteropServices.ComTypes.FILETIME[] pftTimeStamps,
                                    int[] pErrors)
        {
            var e = new OpcEventArgs
            {
                GroupHandle = hGroup,
                Count = dwCount,
                Errors = pErrors,
                Values = pvValues,
                ClientItemsHandle = phClientItems
            };
            OnReadCompleted?.Invoke(this, e);
            Console.WriteLine("-==========OnReadComplete Event==========-");
        }

        public void OnWriteComplete(int dwTransid,
                                    int hGroup,
                                    int hrMastererr,
                                    int dwCount,
                                    int[] pClienthandles,
                                    int[] pErrors)
        {
            var e = new OpcEventArgs
            {
                Errors = pErrors
            };
            OnWriteCompleted?.Invoke(this, e);
            Console.WriteLine("-==========OnWriteComplete Event==========-");
        }

        public void OnCancelComplete(int dwTransid, int hGroup)
        {
            //TODO what should to do?
        }

        #endregion

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //托管对象
                }
                //非托管资源
                if (TimeBias.IsAllocated)
                {
                    TimeBias.Free();
                }
                if (PercendDeadBand.IsAllocated)
                {
                    PercendDeadBand.Free();
                }
                ActiveSubscribe = false;
                m_ConnectionPoint?.Unadvise(m_connectionpoint_cookie);
                m_connectionpoint_cookie = 0;
#pragma warning disable CA1416 // 验证平台兼容性
                if (null != m_ConnectionPoint) Marshal.ReleaseComObject(m_ConnectionPoint);
                m_ConnectionPoint = null;
                if (null != m_ConnectionPointContainer) Marshal.ReleaseComObject(m_ConnectionPointContainer);
                m_ConnectionPointContainer = null;
                if (m_Async2IO != null)
                {
                    Marshal.ReleaseComObject(m_Async2IO);
                    m_Async2IO = null;
                }
                if (m_SyncIO != null)
                {
                    Marshal.ReleaseComObject(m_SyncIO);
                    m_SyncIO = null;
                }
                if (m_StateManagement != null)
                {
                    Marshal.ReleaseComObject(m_StateManagement);
                    m_StateManagement = null;
                }
                if (groupPointer != null)
                {
                    Marshal.ReleaseComObject(groupPointer);
                    groupPointer = null;
                }
                m_ItemManagement = null;
#pragma warning restore CA1416 // 验证平台兼容性
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
