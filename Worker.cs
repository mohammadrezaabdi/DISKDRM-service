namespace SSDDRM_service;
using static Disk;
using System;
using System.IO;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private const int RUN_INTERVAL = 10000;
    private const string DATABASE_PATH = @"./db.bin";

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            DismountUnAuthorizedDisks();
            await Task.Delay(RUN_INTERVAL, stoppingToken);
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BackgroundService starting up ...");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BackgroundService stopping!");
        return base.StopAsync(cancellationToken);
    }

    public void DismountUnAuthorizedDisks()
    {
        List<Disk> listDisk = GetListDisks();
        Database db = new Database(DATABASE_PATH);

        //for filling database file
        // int i = 1;
        // foreach (Disk disk in listDisk)
        // {
        //     _logger.LogInformation(disk.ToString());
        //     if (i == 4) //skip disk 3
        //         continue;
        //     db.Add(disk.hashValue);
        //     i++;
        // }
        // db.SaveToFile(DATABASE_PATH);

        _logger.LogInformation("List Disks:");
        foreach (Disk disk in listDisk)
        {
            _logger.LogInformation(disk.ToString());
            if (!db.Contains(disk.hashValue))
            {
                foreach (string mountVloume in disk.mountedVloumes)
                {
                    DiskEject de = new DiskEject(mountVloume[0]);
                    de.Dismount();
                }
            }
        }
    }
}