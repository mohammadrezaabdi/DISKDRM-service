namespace DISKDRM_service;
using System.Runtime.InteropServices;

public struct SetDiskAttributes
{
    public static uint DISK_ATTRIBUTE_OFFLINE = 0x1;
    public static uint DISK_ATTRIBUTE_READ_ONLY = 0x2;

    public uint Version;
    [MarshalAs(UnmanagedType.I1)]
    public bool Persist;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Reserved1;
    public ulong Attributes;
    public ulong AttributesMask;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] Reserved2;
};
