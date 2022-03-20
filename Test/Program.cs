// See https://aka.ms/new-console-template for more information
ConsoleKeyInfo cki;
OpcDaClient.Db.Manager manager = new OpcDaClient.Db.Manager();
manager.Init();
manager.Run();
string [] tags=manager.GetTagName();
while (true)
{
    Console.Clear();
    Console.WriteLine("Press R to Read Value");
    Console.WriteLine("Press X to Exit");
    Console.WriteLine("Press C to Clear");
    cki = Console.ReadKey();
    if (cki.Key == ConsoleKey.R)
    {
        Console.Clear();
        foreach (string s in tags)
        {
            Console.WriteLine("{0}\t{1}", s, manager[s]);
        }
    }
    if (cki.Key == ConsoleKey.C)
    {
        Console.Clear();
    }
    if (cki.Key == ConsoleKey.X)
    {
        break;
    }
}
manager.Stop();