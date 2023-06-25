namespace DISKDRM_service;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Runtime.ConstrainedExecution;

public class NativeMethods
{

    private const string setupapi = "setupapi.dll";
    private const string kernel32 = "kernel32.dll";

    private NativeMethods()
    {
    }

    [DllImport(setupapi, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetupDiCallClassInstaller(
        DiFunction installFunction,
        SafeDeviceInfoSetHandle deviceInfoSet,
        [In()] ref DeviceInfoData deviceInfoData
    );

    [DllImport(setupapi, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetupDiEnumDeviceInfo(
        SafeDeviceInfoSetHandle deviceInfoSet,
        int memberIndex,
        ref DeviceInfoData deviceInfoData
    );

    [DllImport(setupapi, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern SafeDeviceInfoSetHandle SetupDiGetClassDevs(
        [In()] ref Guid classGuid,
        [MarshalAs(UnmanagedType.LPWStr)]
        string enumerator,
        IntPtr hwndParent,
        SetupDiGetClassDevsFlags flags
    );

    [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetupDiGetDeviceInstanceId(
       IntPtr DeviceInfoSet,
       ref DeviceInfoData did,
       [MarshalAs(UnmanagedType.LPTStr)] StringBuilder DeviceInstanceId,
       int DeviceInstanceIdSize,
       out int RequiredSize
    );

    [SuppressUnmanagedCodeSecurity()]
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    [DllImport(setupapi, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetupDiDestroyDeviceInfoList(
        IntPtr deviceInfoSet
    );

    [DllImport(setupapi, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetupDiSetClassInstallParams(
        SafeDeviceInfoSetHandle deviceInfoSet,
        [In()] ref DeviceInfoData deviceInfoData,
        [In()] ref PropertyChangeParameters classInstallParams,
        int classInstallParamsSize
    );


    [DllImport(kernel32, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr CreateFile(
    string lpFileName,
    uint dwDesiredAccess,
    uint dwShareMode,
    IntPtr SecurityAttributes,
    uint dwCreationDisposition,
    uint dwFlagsAndAttributes,
    IntPtr hTemplateFile
    );

    [DllImport(kernel32, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        IntPtr lpInBuffer,
        uint nInBufferSize,
        IntPtr lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped
    );

    [DllImport(kernel32, ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        byte[] lpInBuffer,
        uint nInBufferSize,
        IntPtr lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped
    );

    [DllImport(kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetVolumeNameForVolumeMountPoint(
        string lpszVolumeMountPoint,
        [Out] StringBuilder lpszVolumeName,
        uint cchBufferLength
    );

    [DllImport(kernel32, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool DeleteVolumeMountPoint(
        string lpszVolumeMountPoint
    );

    [DllImport(kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(
        IntPtr hObject
    );

}
