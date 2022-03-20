// See https://aka.ms/new-console-template for more information
using OpcDaClient.Da;


OpcServer server = new OpcServer("localhost", "Matrikon.OPC.Simulation.1");
server.Connect();
OpcGroup g1= server.AddGroup("g1", true, 1000, 0.0f);
OpcItem[] items = new OpcItem[2];
items[0] = new OpcItem("Random.Real4",OpcDaClient.Comn.OpcDataType.Float);
items[1] = new OpcItem("Random.Int4", OpcDaClient.Comn.OpcDataType.Int);
g1.AddOpcItem(items);
g1.OnDataChanged += new OpcGroup.OnDataChangedHandler(onDataChange); 
void onDataChange(OpcDaClient.Da.OpcItem[] items)
{
    foreach (OpcItem item in items)
    {
        Console.WriteLine("{0},{1},{2},{3}",item.Name,item.Value,item.Quality,item.TimeStamp);
    }
}

Console.Read();
g1.Dispose();
server.Dispose();
