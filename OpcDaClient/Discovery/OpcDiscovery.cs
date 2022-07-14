using OpcRcw.Comn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Discovery
{
    internal class OpcDiscovery
    {
        /// <summary>
        /// The CLSID for the OPC Server Enumerator.
        /// </summary>
        private static readonly Guid OPCEnumCLSID = new("13486D51-4821-11D2-A494-3CB306C10000");
        private static readonly Guid CATID_OPC_DA10 = new("63D5F430-CFE4-11d1-B2C8-0060083BA1FB");
        private static readonly Guid CATID_OPC_DA20 = new("63D5F432-CFE4-11d1-B2C8-0060083BA1FB");
        private static readonly Guid CATID_OPC_DA30 = new("CC603642-66D7-48f1-B69A-B625E73652D7");
        private IOPCServerList2? m_server = null;
        private IOPCServerList? oPCServerList = null;
        private string host = "";

        public OpcDiscovery()
        {

        }
        public ServerInfo? GetOpcServer(string serverName, string host)
        {
            this.host = host;
            ServerInfo? result = null;
            Guid id = typeof(IOPCServerList2).GUID;
            object? o_Server = Comn.ComInterop.CreateInstance(OPCEnumCLSID, host);
            if (o_Server == null)
                throw new ExternalException("GetOpcServerCLSID: CreateInstance Failed");
            m_server = (IOPCServerList2)o_Server;
            oPCServerList = (IOPCServerList)o_Server;
            try
            {
                // 目前只关心opc da2.0
                Guid catid = CATID_OPC_DA20;

                // get list of servers in the specified specification.
                IOPCEnumGUID? enumerator = null;

#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
                m_server.EnumClassesOfCategories(
                    1,
                    new Guid[] { catid },
                    0,
                    null,
                    out enumerator);
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。

                // read clsids.
                Guid[] clsids = Comn.ComInterop.ReadClasses(enumerator);

                // release enumerator object.	
                Comn.ComInterop.RealseComServer(enumerator);
                enumerator = null;

                // fetch class descriptions.
                ServerInfo[] servers = GetServerDetails(clsids.ToArray());

#if DEBUG
                System.Diagnostics.Debug.WriteLine("Find Servers:");
                for (int i = 0; i < servers.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"index={i},server={servers[i].ProgID}/{servers[i].VerIndProgID}");
                }
#endif
                for (int i = 0; i < servers.Length; i++)
                {
                    if (servers[i].ProgID.ToLower() == serverName.ToLower() || servers[i].VerIndProgID.ToLower() == serverName.ToLower())
                    {
                        result = servers[i];
                        break;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return result;
            }
            finally
            {
                // free the server.
                Comn.ComInterop.RealseComServer(m_server);
                m_server = null;
            }
        }
        private ServerInfo[] GetServerDetails(Guid[] clsids)
        {
            ArrayList servers = new ArrayList();
            for (int i = 0; i < clsids.Length; i++)
            {
                Guid clsid = clsids[i];
                try
                {
                    string? progID = null;
                    string? description = null;
                    string? verIndProgID = null;
                    ServerInfo server1 = new();

                    server1.Host = host;
                    server1.CLSID = clsid;

                    m_server?.GetClassDetails(
                        ref clsid,
                        out progID,
                        out description,
                        out verIndProgID);
                    if (verIndProgID != null)
                    {
                        server1.VerIndProgID = verIndProgID;
                    }
                    else if (progID != null)
                    {
                        server1.ProgID = progID;
                    }
                    if (description != null)
                    {
                        server1.Description = description;
                    }
                    servers.Add(server1);
                }
                catch (Exception)
                {
                    ;
                }
                finally
                {

                }
            }
            return (ServerInfo[])servers.ToArray(typeof(ServerInfo));
        }
    }
    internal class ServerInfo
    {
        public string Description { get; set; } = string.Empty;
        public string VerIndProgID { get; set; } = string.Empty;
        public string ProgID { get; set; } = string.Empty;
        public Guid CLSID { get; set; }
        public string Host { get; set; } = string.Empty;
    }
}
