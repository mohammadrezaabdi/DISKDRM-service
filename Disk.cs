namespace SSDDRM_service;

public static class Disk
{
    public static string ListDisks()
    {
        string listDisk = "";

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady){
                continue;
            }
            double freeSpace = drive.TotalFreeSpace;
            double totalSpace = drive.TotalSize;
            double percentFree = (freeSpace / totalSpace) * 100;
            float num = (float)percentFree;

            listDisk += String.Format("Drive:{0} With {1} % free\n", drive.Name, num);
            listDisk += String.Format("Space Remaining:{0}\n", drive.AvailableFreeSpace);
            listDisk += String.Format("Percent Free Space:{0}\n", percentFree);
            listDisk += String.Format("Space used:{0}\n", drive.TotalSize);
            listDisk += String.Format("Type: {0}\n", drive.DriveType);
        }
        return listDisk;
    }
}