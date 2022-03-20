// See https://aka.ms/new-console-template for more information
using OpcDaClient.Da;


OpcServer server = new OpcServer("localhost", "Matrikon.OPC.Simulation.1");
server.Connect();
OpcGroup g1 = server.AddGroup("g1", true, 1000, 0.0f);
OpcItem[] items = new OpcItem[2];
items[0] = new OpcItem("Random.Real4", OpcDaClient.Comn.OpcDataType.Float);
items[1] = new OpcItem("Bucket Brigade.Int4", OpcDaClient.Comn.OpcDataType.Int);
g1.AddOpcItem(items);
//g1.OnDataChanged += new OnDataChangedHandler(onDataChange);
g1.OnReadCompleted += new OnReadCompletedHandler(onDataChange);
g1.OnWriteCompleted += new OnWriteCompletedHandler(onWritecomplete);
//g1.ReadAsync();
int[] errors = new int[1];
g1.WriteAsync(new object[] { 9999 }, new int[] { items[1].ServerHandle }, out errors);

void onDataChange(ItemReadResult[] items)
{
    foreach (ItemReadResult item in items)
    {
        Console.WriteLine("{0},{1},{2},{3}", item.Name, item.Value, item.Quality, item.TimeStamp);
    }
}
void onWritecomplete(ItemWriteResult[] writeResults)
{
    foreach (ItemWriteResult item in writeResults)
    {
        Console.WriteLine("{0},{1}", item.Name, item.Error);
    }
}

Console.Read();
g1.Dispose();
server.Dispose();
