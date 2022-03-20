// See https://aka.ms/new-console-template for more information
using OpcDaAsync.Da;


OpcServer server = new OpcServer("localhost", "Matrikon.OPC.Simulation.1");
server.Connect();
OpcGroup g1= server.AddGroup("g1", true, 1000, 0.0f);
OpcItem[] items = new OpcItem[2];
items[0] = new OpcItem("Random.Real4",OpcDaAsync.Comn.OpcDataType.Float);
items[1] = new OpcItem("Random.Int4", OpcDaAsync.Comn.OpcDataType.Int);
g1.AddOpcItem(items);
g1.OnDataChanged += new EventHandler<OpcEventArgs>(onDataChange);

void onDataChange(object? sender,OpcEventArgs e)
{
    Console.WriteLine("ddddddddddddd");
}

Console.Read();
g1.Dispose();
server.Dispose();
