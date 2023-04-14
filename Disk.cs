namespace SSDDRM_service;
using System.Management;
using System.Security.Cryptography;
using System.Text;

//TODO: handle exceptions
public class Disk
{
    private string name;
    private string serialNumber;
    private string size;
    public List<string> mountedVloumes { get; private set; }
    public byte[] hashValue { get; private set; }

    public Disk(string name, string serialNumber, string size)
    {
        this.name = name;
        this.serialNumber = serialNumber;
        this.size = size;
        this.mountedVloumes = new List<string>();
        this.hashValue = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(name + serialNumber));
    }

    public static List<Disk> GetListDisks()
    {
        List<Disk> listDisk = new List<Disk>();
        ManagementClass driveClass = new ManagementClass("Win32_DiskDrive");
        ManagementObjectCollection drives = driveClass.GetInstances();
        foreach (ManagementObject drive in drives)
        {
            Disk disk = new Disk(drive.GetPropertyValue("Caption")?.ToString() ?? "", drive.GetPropertyValue("SerialNumber")?.ToString() ?? "", drive.GetPropertyValue("Size")?.ToString() ?? "");
            foreach (ManagementObject diskPartition in drive.GetRelated("Win32_DiskPartition"))
            {
                foreach (var diskPart in diskPartition.GetRelated("Win32_LogicalDisk"))
                {
                    disk.mountedVloumes.Add(diskPart.GetPropertyValue("Name")?.ToString() ?? "");
                }
            }
            listDisk.Add(disk);
        }
        return listDisk;
    }

    override public string ToString()
    {
        string str = String.Format("Name: {0}, Serial: {1}, Size: {2} bytes", this.name, this.serialNumber, this.size, this.mountedVloumes);
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
            if (DiskEject.Dismount(mountVloume))
            {
                dismountedVolums.Add(mountVloume);
            }
        }
        return dismountedVolums;
    }
}