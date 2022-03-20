﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Da
{
    public class OpcServer:IDisposable  
    {
        #region public

        public List<OpcGroup> OpcGroups { get; private set; } = new List<OpcGroup>(10);
        //DWORD dwGroupCount;
        //DWORD dwBandWidth;
        public ServerInfo ServerStatus { get; private set; } = new ServerInfo();

        /// <summary>
        /// opcserver name 
        /// </summary>
        public string ProgramID { get; private set; } = "";
        /// <summary>
        /// host
        /// </summary>
        public string Host { get; private set; } = "localhost";
        public bool IsConnected { get; private set; } = false;

        #endregion

        private OpcRcw.Da.IOPCServer? m_OpcServer = null;
        private bool disposedValue;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="host">ip or host name</param>
        /// <param name="programId">opc server's prog id</param>
        public OpcServer(string host, string programId)
        {
            Host = host;
            ProgramID = programId;
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="programId">opc server's prog id</param>
        public OpcServer(string programId)
        {
            Host = "localhost";
            ProgramID = programId;
        }
        /// <summary>
        /// connect to sepcified server
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            if (!string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(ProgramID))
            {
                try
                {
#pragma warning disable CA1416 // 验证平台兼容性
                    Type? opcServerType = Type.GetTypeFromProgID(ProgramID, Host, true);
#pragma warning restore CA1416 // 验证平台兼容性
                    if (opcServerType != null)
                    {
                        object? o = Activator.CreateInstance(opcServerType);
                        if (o != null) m_OpcServer = (OpcRcw.Da.IOPCServer)o;
                        IsConnected = true;
                    }
                    return true;
                }
                catch (Exception)
                {
                    IsConnected = false;
                    throw;
                }
            }
            return false;
        }

        /// <summary>
        /// add opc group
        /// </summary>
        /// <param name="groupName">group name</param>
        /// <returns></returns>
        public OpcGroup AddGroup(string groupName)
        {
            //Groups.Add(group);
            //TODO make a new group
            OpcGroup group = new OpcGroup(groupName);
            Guid riid = typeof(OpcRcw.Da.IOPCItemMgt).GUID;
            try
            {
                m_OpcServer?.AddGroup(group.Name,
                    group.IsActive ? 1 : 0,//IsActive
                    group.RequestUpdateRate,//RequestedUpdateRate 1000ms
                    group.ClientGroupHandle,
                    group.TimeBias.AddrOfPinnedObject(),
                    group.PercendDeadBand.AddrOfPinnedObject(),
                    group.LCID,
                    out group.serverGroupHandle,
                    out group.revisedUpdateRate,
                    ref riid,
                    out group.groupPointer);
                //group state object
                if (group.groupPointer != null)
                {
                    group.InitIoInterfaces(group.groupPointer);
                    OpcGroups.Add(group);
                }
                else
                {
                    throw new ArgumentNullException("AddGroup retrun null");
                }
                return group;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// add group
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="active"></param>
        /// <param name="reqUpdateRate"></param>
        /// <param name="deadBand"></param>
        /// <returns></returns>
        public OpcGroup AddGroup(string groupName, bool active, int reqUpdateRate, float deadBand)
        {
            OpcGroup group = new OpcGroup(groupName, active,reqUpdateRate,deadBand);
            Guid riid = typeof(OpcRcw.Da.IOPCItemMgt).GUID;
            try
            {
                m_OpcServer?.AddGroup(group.Name,
                    group.IsActive ? 1 : 0,//IsActive
                    group.RequestUpdateRate,//RequestedUpdateRate 1000ms
                    group.ClientGroupHandle,
                    group.TimeBias.AddrOfPinnedObject(),
                    group.PercendDeadBand.AddrOfPinnedObject(),
                    group.LCID,
                    out group.serverGroupHandle,
                    out group.revisedUpdateRate,
                    ref riid,
                    out group.groupPointer);
                //group state object
                if (group.groupPointer != null)
                {
                    group.InitIoInterfaces(group.groupPointer);
                    OpcGroups.Add(group);
                }
                else
                {
                    throw new ArgumentNullException("AddGroup retrun null");
                }
                return group;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void RemoveGroup(OpcGroup group)
        {   
            if (OpcGroups.Contains(group))
            {
                m_OpcServer?.RemoveGroup(group.ServerGroupHandle, 1);
                OpcGroups.Remove(group);
            }
        }

        /// <summary>
        /// 服务器状态
        /// </summary>
        /// <returns></returns>
        public ServerInfo GetServerStatus()
        {
            IntPtr statusPtr=IntPtr.Zero;
            m_OpcServer?.GetStatus(out statusPtr);
            OpcRcw.Da.OPCSERVERSTATUS status;
            ServerStatus = new ServerInfo();
            if (statusPtr != IntPtr.Zero)
            {
                object? o = Marshal.PtrToStructure(statusPtr, typeof(OpcRcw.Da.OPCSERVERSTATUS));
                if (null != o)
                {
                    status = (OpcRcw.Da.OPCSERVERSTATUS)o;
                    ServerStatus.Version = status.wMajorVersion.ToString()+"."+status.wMinorVersion.ToString()+"."+status.wBuildNumber.ToString();
                    ServerStatus.ServerState = status.dwServerState;
                    ServerStatus.StartTime = new DateTime(status.ftStartTime.dwHighDateTime << 32 + status.ftStartTime.dwLowDateTime);
                    ServerStatus.CurrentTime = new DateTime(status.ftCurrentTime.dwHighDateTime << 32 + status.ftCurrentTime.dwLowDateTime);
                    ServerStatus.LastUpdateTime = new DateTime(status.ftLastUpdateTime.dwHighDateTime << 32 + status.ftLastUpdateTime.dwLowDateTime);
                    ServerStatus.VendorInfo = status.szVendorInfo;
                }
            }
            return ServerStatus;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //ServerObj.RemoveGroup(pSvrGroupHandle, 0);
                for(int i=0;i<OpcGroups.Count;i++)
                    RemoveGroup(OpcGroups[i]);
#pragma warning disable CA1416 // 验证平台兼容性
                if (m_OpcServer != null)
                {
                    Marshal.ReleaseComObject(m_OpcServer);
                    m_OpcServer = null;
                }
#pragma warning restore CA1416 // 验证平台兼容性
                if (disposing)
                {
                    OpcGroups.Clear();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
