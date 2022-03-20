// See https://aka.ms/new-console-template for more information
ConsoleKeyInfo cki;
OpcDaClient.Db.Manager manager = new OpcDaClient.Db.Manager();
manager.Init();
manager.Run();
while (true)
{
    Console.WriteLine("Press R to Read Value");
    Console.WriteLine("Press X to Exit");
    Console.WriteLine("Press C to Clear");
    cki = Console.ReadKey();
    if (cki.Key == ConsoleKey.R)
    {
        Console.WriteLine("{0}-{1}", "Random.Real4",manager["Random.Real4"]);
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