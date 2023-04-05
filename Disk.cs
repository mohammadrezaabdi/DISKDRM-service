namespace SSDDRM_service;
using System.Management;

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
            list += String.Format("Serial: {0}\n", drive["SerialNumber"]);
            list += String.Format("Size: {0} bytes\n", drive["Size"]);
            // foreach (PropertyData property in drive.Properties)
            // {
            //     list += String.Format("Property: {0}, Value: {1}\n", property.Name, property.Value);
            // }
            //TODO: check hash in database & eject if needed
            foreach (ManagementObject diskPartition in drive.GetRelated("Win32_DiskPartition"))
            {
                foreach (var disk in diskPartition.GetRelated("Win32_LogicalDisk"))
                {
                    list += String.Format(disk.Properties["Name"].Value.ToString() + "\n");
                }
            }
            list += "\n";
        }
        return list;
    }
}