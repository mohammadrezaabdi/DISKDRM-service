namespace DISKDRM_service;

using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

//TODO: handle exceptions
public class Disk
{
    private string name;
    private string serialNumber;
    private string size;
    private string path;
    private Guid guid;
    public List<string> mountedVloumes { get; private set; }
    public byte[] hashValue { get; private set; }

    public Disk(string name, string serialNumber, string size, string path, string guid)
    {
        this.name = name;
        this.serialNumber = serialNumber;
        this.size = size;
        this.path = path;
        this.guid = new Guid(guid);
        this.mountedVloumes = new List<string>();
        this.hashValue = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(name + ' ' + serialNumber));
    }

    public static List<Disk> GetListDisks()
    {
        List<Disk> listDisk = new List<Disk>();
        ManagementObjectSearcher driveClasses = new ManagementObjectSearcher(
            "SELECT * FROM Win32_PnPEntity WHERE PNPClass='DiskDrive'");
        foreach (ManagementObject driveClass in driveClasses.Get())
        {
            foreach (ManagementObject drive in driveClass.GetRelated("Win32_DiskDrive"))
            {
                Disk disk = new Disk(
                    drive.GetPropertyValue("Caption")?.ToString() ?? "",
                    drive.GetPropertyValue("SerialNumber")?.ToString() ?? "",
                    drive.GetPropertyValue("Size")?.ToString() ?? "",
                    drive.GetPropertyValue("PNPDeviceID")?.ToString() ?? "",
                    driveClass.GetPropertyValue("ClassGuid")?.ToString() ?? ""
                    );
                foreach (ManagementObject diskPartition in drive.GetRelated("Win32_DiskPartition"))
                {
                    foreach (ManagementObject diskPart in diskPartition.GetRelated("Win32_LogicalDisk"))
                    {
                        disk.mountedVloumes.Add(diskPart.GetPropertyValue("Name")?.ToString() ?? "");
                    }
                }
                listDisk.Add(disk);
            }
        }
        return listDisk;
    }

    override public string ToString()
    {
        string str = String.Format("Name: {0}, Serial: {1}, Size: {2} bytes, Path: {3}, classGuid: {4}", this.name, this.serialNumber, this.size, this.path, this.guid);
        var volumestr = new StringBuilder().AppendJoin(" | ", this.mountedVloumes);
        var hashstr = new StringBuilder();
        foreach (byte theByte in this.hashValue)
        {
            hashstr.Append(theByte.ToString("x2"));
        }
        return String.Format("{0}, Volume: {1}, Hash: {2}", str, volumestr.ToString(), hashstr.ToString());
    }

    public List<string> Dismount()
    {
        List<string> dismountedVolums = new List<string>();
        foreach (string mountVloume in mountedVloumes)
        {
            try
            {
                if (DiskEject.Dismount(mountVloume))
                {
                    dismountedVolums.Add(mountVloume);
                }
            }
            catch (Win32Exception e)
            {
                if (e.Message.Equals(DiskEject.VOLUME_IS_WIN_PRIMARY))
                {
                    break;
                }
            }
        }
        return dismountedVolums;
    }

    public List<string> Disable()
    {
        if (DiskEject.SetDeviceDisabled(this.guid, this.path))
            return this.mountedVloumes;
        return new List<string>();
    }
}