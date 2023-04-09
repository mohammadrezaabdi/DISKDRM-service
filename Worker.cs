namespace SSDDRM_service;
using static Disk;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private const int RUN_INTERVAL = 10000; 

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            _logger.LogInformation("List Disks:\n" + ListDisks());
            //example of removing device
            DiskEject de = new DiskEject('E');
            de.Dismount();
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
}