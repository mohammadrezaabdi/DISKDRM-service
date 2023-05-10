namespace DISKDRM_service;
using static Disk;
using System;
using System.Text;
using System.Diagnostics;
using SharpASM = SharpAssembly.SharpASM;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private const int RUN_INTERVAL = 7000;
    private const int AWAKE_SIGNAL_INTERVAL = 3;
    public Worker(ILogger<Worker> logger) => _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            try
            {
                DisconnectUnAuthorizedDisks();
                SetWorkerAwakeSignal();
            }
            catch (System.Exception e)
            {
                _logger.LogError("Error While Running Worker:\n", e.ToString());
            }
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

    public void DisconnectUnAuthorizedDisks()
    {
        List<Disk> listDisk = GetListDisks();

        //add C: primary disk to database
        foreach (Disk disk in listDisk)
        {
            if (disk.mountedVloumes.Contains(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))?.Substring(0, 2) ?? "C:"))
            {
                Database.GetInstance.Add(disk.hashValue);
            }
        }

        _logger.LogInformation("List Disks:");
        List<string> dismountedVolumes = new List<string>();
        foreach (Disk disk in listDisk)
        {
            _logger.LogInformation(disk.ToString());
            if (!Database.GetInstance.Contains(disk.hashValue) && disk.mountedVloumes.Any())
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

    public void SetWorkerAwakeSignal()
    {
        byte[] setRegistersMagicValASM =
            {
                                                                            //  %define MAGIC_VAL 0x616D6920616D696E
                0x49, 0xb8, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61, //	mov 	r8, MAGIC_VAL
                0x49, 0xb9, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61, //  mov 	r9, MAGIC_VAL
                0x49, 0xba, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61, // 	mov 	r10, MAGIC_VAL
                0x49, 0xbb, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61, // 	mov 	r11, MAGIC_VAL
                0x49, 0xbc, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61, // 	mov 	r12, MAGIC_VAL
                0x49, 0xbd, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61, // 	mov 	r13, MAGIC_VAL
                0x49, 0xbe, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61, // 	mov 	r14, MAGIC_VAL
                0x49, 0xbf, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61, //	mov 	r15, MAGIC_VAL
                0xc3,                                                       //  ret
            };

        Stopwatch s = new Stopwatch();
        s.Start();
        _logger.LogInformation("setting cpu registers magic values ...");
        while (s.Elapsed < TimeSpan.FromSeconds(3))
        {
            SharpASM.callASM(setRegistersMagicValASM);
        }
        s.Stop();
    }
}