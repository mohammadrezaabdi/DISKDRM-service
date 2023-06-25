namespace DISKDRM_service;
using Microsoft.Win32.SafeHandles;

public class SafeDeviceInfoSetHandle : SafeHandleZeroOrMinusOneIsInvalid
{

    public SafeDeviceInfoSetHandle()
        : base(true)
    {
    }

    protected override bool ReleaseHandle()
    {
        return NativeMethods.SetupDiDestroyDeviceInfoList(this.handle);
    }

}