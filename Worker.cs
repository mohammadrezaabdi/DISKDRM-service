namespace DISKDRM_service;
using static Disk;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using Process.NET;
using Process.NET.Memory;
using Process.NET.Native.Types;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private const int RUN_INTERVAL = 8000;
    public Worker(ILogger<Worker> logger) => _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            try
            {
                DisconnectUnAuthorizedDisks();
                SetMagicValueR8R12(); // the service execution is checked by setting magic value for registers R8-R15 (X64)
            }
            catch (Exception ex)
            {
                _logger.LogError("Error While Running Worker:\n" + ex.Message);
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
                    dismountedVolumes = dismountedVolumes.Concat(disk.Disable()).ToList();
                }
                catch (Exception)
                {
                    dismountedVolumes = dismountedVolumes.Concat(disk.Dismount()).ToList();
                }
            }
        }
        _logger.LogInformation("volumes [{mountVolume}] dismounted successfully.", new StringBuilder().AppendJoin(", ", dismountedVolumes));
    }

    [SuppressUnmanagedCodeSecurity] // disable security checks for better performance
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] // cdecl - let caller (.NET CLR) clean the stack
    private delegate int AssemblyAddFunction(int x, int y);
    private void SetMagicValueR8R12()
    {
        var currentProcess = new ProcessSharp(System.Diagnostics.Process.GetCurrentProcess(), MemoryType.Local);
        byte[] assembledCode =
        {
            0x48, 0x31, 0xc9,                                              //       xor    rcx, rcx 
                                                                           //  loop:
            0x48, 0x81, 0xf9, 0x00, 0x94, 0x35, 0x77,                      //       cmp    rcx, 2000000000
            0x7d, 0x38,                                                    //       jge    end_loop
            0x49, 0xb8, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61,    //       mov    r8, MAGIC_VAL
            0x49, 0xb9, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61,    //       mov    r9, MAGIC_VAL
            0x49, 0xba, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61,    //       mov    r10, MAGIC_VAL
            0x49, 0xbb, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61,    //       mov    r11, MAGIC_VAL
            0x49, 0xbc, 0x6e, 0x69, 0x6d, 0x61, 0x20, 0x69, 0x6d, 0x61,    //       mov    r12, MAGIC_VAL
            0x48, 0x83, 0xc1, 0x01,                                        //       add    rcx, 1
            0xeb, 0xbf,                                                    //       jmp    loop
                                                                           //  end_loop:
            0xc3,                                                          //       ret
        };
        var allocatedCodeMemory = currentProcess.MemoryFactory.Allocate(
            name: "set_magic_signal_asm",
            size: assembledCode.Length,
            protection: MemoryProtectionFlags.ExecuteReadWrite
        );
        allocatedCodeMemory.Write(0, assembledCode);
        var myAssemblyFunction = Marshal.GetDelegateForFunctionPointer<AssemblyAddFunction>(allocatedCodeMemory.BaseAddress);
        myAssemblyFunction(10, -15);
        allocatedCodeMemory.Dispose();
    }
}