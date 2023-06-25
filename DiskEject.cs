namespace DISKDRM_service;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.ComponentModel;
using static NativeMethods;
using static FileConsts;
using static IOCTLValues;
using static FSCTLValues;
using static SetDiskAttributes;

//TODO: use library class in https://github.com/dotnet/pinvoke
//TODO: handle exceptions
public static class DiskEject
{
    private const int MAX_PATH = 260;
    private const int AQUIRE_LOCK_TIMEOUT = 10;
    public const string VOLUME_NOT_FOUND = "NOT EXISTS";
    public const string VOLUME_IS_WIN_PRIMARY = "WIN VOL";

    public static bool Dismount(string driveName)
    {
        string drivePath = @"" + driveName.Split(":")[0] + ":\\";
        bool result = false;
        if (!Directory.Exists(drivePath))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), VOLUME_NOT_FOUND);
        }

        string filename = @"\\.\" + driveName.Split(":")[0] + ":";
        IntPtr handle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
        if (LockVolume(handle) && DismountVolume(handle))
        {
            PreventRemovalOfVolume(handle, false);
            result = AutoEjectVolume(handle);
        }
        if (!result)
            result = OfflineDisk(handle);
        CloseVolume(handle);
        if (!result)
            result = RemoveVolume(drivePath);
        return result;
    }

    private static bool LockVolume(IntPtr handle)
    {
        uint byteReturned;

        for (int i = 0; i < AQUIRE_LOCK_TIMEOUT; i++)
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

    private static bool RemoveVolume(string drivePath)
    {
        StringBuilder volume = new StringBuilder(MAX_PATH);
        return GetVolumeNameForVolumeMountPoint(drivePath, volume, (uint)MAX_PATH) ? DeleteVolumeMountPoint(drivePath) : false;
    }

    private static bool OfflineDisk(IntPtr handle)
    {
        uint bytes_returned = 0;
        bool b_offline = false;
        var disk_attr = new SetDiskAttributes();
        disk_attr.Persist = true;
        disk_attr.AttributesMask = DISK_ATTRIBUTE_OFFLINE;
        disk_attr.Attributes = DISK_ATTRIBUTE_OFFLINE;
        disk_attr.Reserved1 = new byte[3] { 0, 0, 0 };
        disk_attr.Reserved2 = new uint[4] { 0, 0, 0, 0 };

        int nPtrQryBytes = Marshal.SizeOf(disk_attr);
        disk_attr.Version = (uint)nPtrQryBytes;

        IntPtr ptrQuery = Marshal.AllocHGlobal(nPtrQryBytes);
        Marshal.StructureToPtr(disk_attr, ptrQuery, false);

        b_offline = DeviceIoControl(handle, IOCTL_DISK_SET_DISK_ATTRIBUTES, ptrQuery, (uint)nPtrQryBytes, IntPtr.Zero, 0, out bytes_returned, IntPtr.Zero);
        // Invalidates the cached partition table and re-enumerates the device.
        b_offline = b_offline || DeviceIoControl(handle, IOCTL_DISK_UPDATE_PROPERTIES, IntPtr.Zero, 0, IntPtr.Zero, 0, out bytes_returned, IntPtr.Zero);
        return b_offline;
    }

    public static bool SetDeviceDisabled(Guid classGuid, string instanceId)
    {
        SafeDeviceInfoSetHandle diSetHandle = null;
        try
        {
            // Get the handle to a device information set for all devices matching classGuid that are present on the 
            // system.
            diSetHandle = NativeMethods.SetupDiGetClassDevs(ref classGuid, null, IntPtr.Zero, SetupDiGetClassDevsFlags.Present);
            // Get the device information data for each matching device.
            DeviceInfoData[] diData = GetDeviceInfoData(diSetHandle);
            // Find the index of our instance. i.e. the touchpad mouse - I have 3 mice attached...
            int index = GetIndexOfInstance(diSetHandle, diData, instanceId);
            if (index < 0) return false;
            // Disable...
            return DisableDevice(diSetHandle, diData[index]);
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            if (diSetHandle != null)
            {
                if (diSetHandle.IsClosed == false)
                {
                    diSetHandle.Close();
                }
                diSetHandle.Dispose();
            }
        }
    }

    private static DeviceInfoData[] GetDeviceInfoData(SafeDeviceInfoSetHandle handle)
    {
        List<DeviceInfoData> data = new List<DeviceInfoData>();
        DeviceInfoData did = new DeviceInfoData();
        int didSize = Marshal.SizeOf(did);
        did.Size = didSize;
        int index = 0;
        while (NativeMethods.SetupDiEnumDeviceInfo(handle, index, ref did))
        {
            data.Add(did);
            index += 1;
            did = new DeviceInfoData();
            did.Size = didSize;
        }
        return data.ToArray();
    }

    // Find the index of the particular DeviceInfoData for the instanceId.
    private static int GetIndexOfInstance(SafeDeviceInfoSetHandle handle, DeviceInfoData[] diData, string instanceId)
    {
        const int ERROR_INSUFFICIENT_BUFFER = 122;
        for (int index = 0; index <= diData.Length - 1; index++)
        {
            StringBuilder sb = new StringBuilder(1);
            int requiredSize = 0;
            bool result = NativeMethods.SetupDiGetDeviceInstanceId(handle.DangerousGetHandle(), ref diData[index], sb, sb.Capacity, out requiredSize);
            if (result == false && Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
            {
                sb.Capacity = requiredSize;
                result = NativeMethods.SetupDiGetDeviceInstanceId(handle.DangerousGetHandle(), ref diData[index], sb, sb.Capacity, out requiredSize);
            }
            if (result == false)
                throw new Win32Exception();
            if (instanceId.Equals(sb.ToString()))
            {
                return index;
            }
        }
        // not found
        return -1;
    }

    private static bool DisableDevice(SafeDeviceInfoSetHandle handle, DeviceInfoData diData)
    {
        PropertyChangeParameters @params = new PropertyChangeParameters();
        // The size is just the size of the header, but we've flattened the structure.
        // The header comprises the first two fields, both integer.
        @params.Size = 8;
        @params.DiFunction = DiFunction.PropertyChange;
        @params.Scope = Scopes.Global;
        @params.StateChange = StateChangeAction.Disable;

        bool result = SetupDiSetClassInstallParams(handle, ref diData, ref @params, Marshal.SizeOf(@params));
        if (result == false) throw new Win32Exception();
        result = SetupDiCallClassInstaller(DiFunction.PropertyChange, handle, ref diData);
        if (result == false)
        {
            int err = Marshal.GetLastWin32Error();
            if (err == (int)SetupApiError.NotDisableable)
                throw new ArgumentException("Device can't be disabled (programmatically or in Device Manager).");
            else if (err >= (int)SetupApiError.NoAssociatedClass && err <= (int)SetupApiError.OnlyValidateViaAuthenticode)
                throw new Win32Exception("SetupAPI error: " + ((SetupApiError)err).ToString());
            else
                throw new Win32Exception();
        }
        return result;
    }
}