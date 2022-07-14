// See https://aka.ms/new-console-template for more information
ConsoleKeyInfo cki;
OpcDaClient.Manager.Manager manager = new OpcDaClient.Manager.Manager();
manager.Init();
manager.Run();
string [] tags=manager.GetTagName();
while (true)
{
    Console.Write("Press ");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write("R");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(" to Read Value");

    Console.Write("Press ");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write("C");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(" to Clear Screen");

    Console.Write("Press ");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("X");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(" to Exit");


    cki = Console.ReadKey();
    if (cki.Key == ConsoleKey.R)
    {
        Console.Clear();
        Console.WriteLine("=====================READ============================");
        foreach (string s in tags)
        {
            Console.WriteLine("{0}\t{1}", s, manager[s]);
        }
        Console.WriteLine("=====================READ END========================");
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