namespace DISKDRM_service;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PropertyChangeParameters
{
    public int Size;
    // part of header. It's flattened out into 1 structure.
    public DiFunction DiFunction;
    public StateChangeAction StateChange;
    public Scopes Scope;
    public int HwProfile;
}