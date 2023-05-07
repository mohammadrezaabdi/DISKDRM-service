using SSDDRM_service;
using Serilog;

internal class Program
{
    protected static string UPDATE_DB_FLAG = "--add-disk";
    protected static string QUIT_FLAG = "q";
    private static async Task Main(string[] args)
    {

        foreach (var arg in args)
        {
            if (arg.ToLower().Equals(UPDATE_DB_FLAG))
            {
                UpdateDB();
                System.Environment.Exit(0);
            }
        }

        var progData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(progData, "SSDDRM", "servicelog.txt"))
            .CreateLogger();

        IHost host = Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .UseSerilog()
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
            })
            .Build();

        await host.RunAsync();
    }

    private static void UpdateDB()
    {
        List<Disk> listDisk = Disk.GetListDisks();
        Console.WriteLine("- List of Disks:");
        for (int i = 0; i < listDisk.Count(); i++)
        {
            Console.WriteLine($"+ Disk {i + 1}: " + listDisk[i]);
        }

        while (true)
        {
            Console.Write("-- Enter Disk Number: (or 'Q' for Quit) ");
            string option = Console.ReadLine()?.ToString() ?? "Q";
            if (option.ToLower().Equals(QUIT_FLAG))
            {
                break;
            }
            try
            {
                int index = Int32.Parse(option);
                if (index <= 0 || index > listDisk.Count())
                {
                    throw new FormatException();
                }
                Database.GetInstance.Add(listDisk[index - 1].hashValue);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid Option");
            }
        }

        Database.GetInstance.SaveToFile();
    }
}