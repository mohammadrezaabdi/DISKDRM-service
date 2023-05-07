using System.Runtime.InteropServices;
namespace SSDDRM_service;
using System.IO;

//TODO: handle exceptions
public sealed class Database
{

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int memcmp(byte[] b1, byte[] b2, long count);

    private static readonly Database database = new Database();
    public List<byte[]> db { get; private set; }
    public const int ENTITY_SIZE = 32;
    private string DATABASE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SSDDRM", "db.bin");

    static Database()
    {

    }
    private Database()
    {
        // Store File Path Info
        var fileinfo = new System.IO.FileInfo(DATABASE_PATH);
        if (!fileinfo.Exists)
        {
            Directory.CreateDirectory(fileinfo.Directory.FullName);
            File.Create(fileinfo.FullName).Close();
        }

        byte[] stream = File.ReadAllBytes(fileinfo.FullName);
        if (stream.Length % ENTITY_SIZE != 0)
        {
            throw new FileLoadException("Database file is corrupted!!!");
        }
        db = new List<byte[]>();
        for (int i = 0; i < stream.Length; i += ENTITY_SIZE)
        {
            byte[] diskHash = new byte[ENTITY_SIZE];
            Array.Copy(stream, i, diskHash, 0, ENTITY_SIZE);
            db.Add(diskHash);
        }
    }
    public static Database GetInstance
    {
        get
        {
            return database;
        }
    }

    public bool Contains(byte[] entity)
    {
        foreach (byte[] b in db)
        {
            if (ByteArrayCompare(b, entity))
            {
                return true;
            }
        }
        return false;
    }

    public int FindIndex(byte[] entity)
    {
        for (int i = 0; i < db.Count(); i++)
        {
            if (ByteArrayCompare(db[i], entity))
            {
                return i;
            }
        }
        return -1;
    }

    private bool ByteArrayCompare(byte[] b1, byte[] b2)
    {
        return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
    }

    public void Add(byte[] entity)
    {
        if (!Contains(entity))
        {
            db.Add(entity);
        }
    }

    public void Remove(byte[] entity)
    {
        int index = FindIndex(entity);
        if (index >= 0)
        {
            db.RemoveAt(index);
        }
    }

    public void SaveToFile()
    {
        byte[] stream = new byte[db.Count * ENTITY_SIZE];
        for (int i = 0; i < db.Count; i++)
        {
            Array.Copy(db[i], 0, stream, i * ENTITY_SIZE, ENTITY_SIZE);
        }
        File.WriteAllBytes(DATABASE_PATH, stream);
    }
}