using System.Management;
namespace SSDDRM_service;

public static class Disk
{
    public static string ListDisks()
    {
        string list = "";
        ManagementClass driveClass = new ManagementClass("Win32_DiskDrive");
        ManagementObjectCollection drives = driveClass.GetInstances();
        foreach (ManagementObject drive in drives)
        {
            list += String.Format("Name: {0}\n", drive["Caption"]);
            list += String.Format("SerialNumber: {0}\n", drive["SerialNumber"]);
            list += String.Format("Size: {0} bytes\n", drive["Size"]);
            // foreach (PropertyData property in drive.Properties)
            // {
            //     list += String.Format("Property: {0}, Value: {1}\n", property.Name, property.Value);
            // }
            list += "\n";
        }
        return list;
    }
}