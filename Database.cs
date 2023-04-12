using System.Runtime.InteropServices;
namespace SSDDRM_service;
using System.IO;

public class Database
{

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int memcmp(byte[] b1, byte[] b2, long count);
    public List<byte[]> db { get; private set; }
    public static int ENTITY_SIZE = 32;

    public Database(string database_path)
    {
        byte[] stream = File.ReadAllBytes(database_path);
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

    public void SaveToFile(string database_path)
    {
        byte[] stream = new byte[db.Count * ENTITY_SIZE];
        for (int i = 0; i < db.Count; i++)
        {
            Array.Copy(db[i], 0, stream, i * ENTITY_SIZE, ENTITY_SIZE);
        }
        File.WriteAllBytes(database_path, stream);
    }
}