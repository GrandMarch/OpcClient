// See https://aka.ms/new-console-template for more information
//
//测试程序连接的是虚拟机的opc server
//vs运行在宿主机，通过远程调试的方式在192.168.56.136运行改测试程序
//测试程序连接到192.168.56.142的opc server
//
ConsoleKeyInfo cki;
OpcDaClient.Manager.Manager manager = new OpcDaClient.Manager.Manager();
manager.Init();
manager.Run();
string [] tags=manager.GetTagNames();
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