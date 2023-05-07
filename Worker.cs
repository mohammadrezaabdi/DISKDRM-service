namespace SSDDRM_service;
using static Disk;
using System;
using System.Text;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private const int RUN_INTERVAL = 10000;

    private Database? db;

    public Worker(ILogger<Worker> logger) => _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            DismountUnAuthorizedDisks(db);
            await Task.Delay(RUN_INTERVAL, stoppingToken);
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BackgroundService starting up ...");
        _logger.LogInformation("Initialize database ...");
        db = new Database();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BackgroundService stopping!");
        return base.StopAsync(cancellationToken);
    }

    public void DismountUnAuthorizedDisks(Database db)
    {
        List<Disk> listDisk = GetListDisks();
        //TODO: add C: primary disk to database
        foreach (Disk disk in listDisk)
        {
            if (disk.mountedVloumes.Contains(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))?.Substring(0, 2) ?? "C:"))
            {
                db.Add(disk.hashValue);
            }
        }

        // DEBUG: for filling database file
        // int i = 1;
        // foreach (Disk disk in listDisk)
        // {
        //     _logger.LogInformation(disk.ToString());
        //     if (i == 4) //skip disk 3
        //         continue;
        //     db.Add(disk.hashValue);
        //     i++;
        // }
        // db.SaveToFile();

        _logger.LogInformation("List Disks:");
        List<string> dismountedVolumes = new List<string>();
        foreach (Disk disk in listDisk)
        {
            _logger.LogInformation(disk.ToString());
            if (!db.Contains(disk.hashValue) && disk.mountedVloumes.Any())
            {
                try
                {
                    disk.Disable();
                    dismountedVolumes = dismountedVolumes.Concat(disk.mountedVloumes).ToList();
                }
                catch (Exception)
                {
                    dismountedVolumes = dismountedVolumes.Concat(disk.Dismount()).ToList();
                }
            }
        }
        _logger.LogInformation("volumes [{mountVolume}] dismounted successfully.", new StringBuilder().AppendJoin(", ", dismountedVolumes));
    }
}