namespace SSDDRM_service;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

//TODO: use library class in https://github.com/dotnet/pinvoke
//TODO: handle exceptions
public static class DiskEject
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr SecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile
        );

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        IntPtr lpInBuffer,
        uint nInBufferSize,
        IntPtr lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped
    );

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        byte[] lpInBuffer,
        uint nInBufferSize,
        IntPtr lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GetVolumeNameForVolumeMountPoint(
        string lpszVolumeMountPoint,
        [Out] StringBuilder lpszVolumeName,
        uint cchBufferLength
    );

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool DeleteVolumeMountPoint(string lpszVolumeMountPoint);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);
    private const int MAX_PATH = 260;

    const uint GENERIC_READ = 0x80000000;
    const uint GENERIC_WRITE = 0x40000000;
    const uint FILE_SHARE_READ = 0x1;
    const uint FILE_SHARE_WRITE = 0x2;
    const uint FSCTL_LOCK_VOLUME = 0x00090018;
    const uint FSCTL_DISMOUNT_VOLUME = 0x00090020;
    const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
    const uint IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;
    const uint IOCTL_MOUNTMGR_DELETE_POINTS = 0x6dc004;

    public static bool Dismount(string driveName)
    {
        string drivePath = @"" + driveName.Split(":")[0] + ":\\";
        bool result = false;
        if (!Directory.Exists(drivePath))
        {
            Console.WriteLine($"Drive letter {drivePath} does not exist!");
            return result;
        }
        if (drivePath.Equals(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))))
        {
            Console.WriteLine($"Drive letter {drivePath} cannot be ejected!");
            return result;
        }

        string filename = @"\\.\" + driveName.Split(":")[0] + ":";
        IntPtr handle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
        if (LockVolume(handle) && DismountVolume(handle))
        {
            PreventRemovalOfVolume(handle, false);
            result = AutoEjectVolume(handle);
        }
        CloseVolume(handle);
        if (!result)
            result = SafeRemoveVolume(drivePath);
        return result;
    }

    private static bool LockVolume(IntPtr handle)
    {
        uint byteReturned;

        for (int i = 0; i < 10; i++)
        {
            if (DeviceIoControl(handle, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero))
            {
                return true;
            }
            Thread.Sleep(500);
        }
        return false;
    }

    private static bool PreventRemovalOfVolume(IntPtr handle, bool prevent)
    {
        byte[] buf = new byte[1];
        uint retVal;

        buf[0] = (prevent) ? (byte)1 : (byte)0;
        return DeviceIoControl(handle, IOCTL_STORAGE_MEDIA_REMOVAL, buf, 1, IntPtr.Zero, 0, out retVal, IntPtr.Zero);
    }

    private static bool DismountVolume(IntPtr handle)
    {
        uint byteReturned;
        return DeviceIoControl(handle, FSCTL_DISMOUNT_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
    }

    private static bool AutoEjectVolume(IntPtr handle)
    {
        uint byteReturned;
        return DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
    }

    private static bool CloseVolume(IntPtr handle)
    {
        return CloseHandle(handle);
    }

    //TODO: remove letter with IOCTL_MOUNTMGR_DELETE_POINTS
    private static bool SafeRemoveVolume(string drivePath)
    {
        StringBuilder volume = new StringBuilder(MAX_PATH);
        return GetVolumeNameForVolumeMountPoint(drivePath, volume, (uint)MAX_PATH) ? DeleteVolumeMountPoint(drivePath) : false;
    }
}