namespace DISKDRM_service;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct DeviceInfoData
{
    public int Size;
    public Guid ClassGuid;
    public int DevInst;
    public IntPtr Reserved;
}
