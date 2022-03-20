using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcDaClient.Db
{
    /// <summary>
    /// 数据管理类
    /// 通过订阅的方式将数据缓存到管理类中，供其他的程序调用
    /// 管理类目前不考虑数据写入的问题，暂时没有写入需求。
    /// </summary>
    public class Manager
    {
        /// <summary>
        /// 所有的点名都不能重复
        /// </summary>
        public SortedList<string, OpcDaClient.Da.OpcItem> Database = new SortedList<string, OpcDaClient.Da.OpcItem>();
        /// <summary>
        /// server
        /// </summary>
        private OpcDaClient.Da.OpcServer? _server;
        private List<OpcDaClient.Da.OpcGroup> OpcGroups = new List<Da.OpcGroup> { };

        private Config _config=new Config();

        public object? this[string name]
        {
            get
            {
                try
                {
                    OpcDaClient.Da.OpcItem item = Database[name];
                    return item.Value;
                }
                catch (Exception)
                {

                    return null;
                }
            }
        }

        public bool Init()
        {
            string file=System.AppDomain.CurrentDomain.BaseDirectory+"opc.json";
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string content=sr.ReadToEnd();
                    try
                    {
                        Config?  x = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(content);
                        if (x != null)
                        {
                            _config = x;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                        throw;
                    }
                }
            }
        }
        public void Run()
        {
            try
            {
                _server = new OpcDaClient.Da.OpcServer(_config.Host, _config.OpcServer);
                _server?.Connect();
                foreach (Config.Group group in _config.Groups)
                {
                    Da.OpcGroup? g=_server?.AddGroup(group.Name, group.Active, group.UpdateRate, group.DeadBand);
                    if (null != g)
                    {
                        OpcGroups.Add(g);
                        foreach (Config.Item item in group.Items)
                        {
                            OpcDaClient.Da.OpcItem opcItem = new Da.OpcItem(item.Name, item.DataType);
                            g.AddOpcItem(new Da.OpcItem[] { opcItem });
                            Database.Add(opcItem.Name, opcItem);
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
        public void Stop()
        {
            OpcDaClient.Da.OpcGroup[]? groups = _server?.OpcGroups.ToArray();
            if (null != groups)
            {
                foreach (OpcDaClient.Da.OpcGroup g in groups)
                {
                    g.Dispose();
                }
            }
            _server?.Dispose();
        }
    }

    public class Config
    {
        public string OpcServer { get; set; } = "";
        public string Host { get; set; } = "127.0.0.1";
        public Group[] Groups { get; set; } =new Group[0];
        public class Item
        {
            public string Name { get; set; } = "";
            public Comn.OpcDataType DataType { get; set; } = Comn.OpcDataType.Int;
        }
        public class Group
        {
            public string Name { get; set; }= Guid.NewGuid().ToString();
            public bool Active { get; set; } = true;
            public float DeadBand { get; set; } = 0.0f;
            public int UpdateRate { get; set; } = 1000;
            public Item[] Items { get; set; }=new Item[0];
        }
    }
}
