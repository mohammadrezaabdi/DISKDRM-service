# Disk Disconnect Service
A simple program that ejects and removes Any unauthorized storage disk in Windows. it can removes both USB disks and Firewire disks regardless of its interface. it runs as a Windows background service and removes any unauthorized disk. the disk is authorized when its hashkey exists in the database.
The service developed using .NET 6.0 and WIN32 APIs and tested on Windows 10 and Windows Server 2016 onwards.

## How Does it work?
There are 4 strategies that are utilized for Disconnecting a disk:
1. Disconnecting a disk by disabling its disk driver, in `Windows Device Manager` utility you can disabling the disk driver manualy. if the disk is not held by any process, the service would disconnects the disk by this way.
2. Disconnecting a disk by make it Offline, in `Windows Disk Management` utility you can set a storage disk status to Offline mode. the Offline mode only disconnects the disk from Windows filesystem which makes any read or write operations impossible. some disks cannot 
3. Disconnecting a disk by ejecting it (removable devices only), in Windows `Devices and Printers` section you can Eject removable devices in a safe way. if the disk is held by a process, it cannot be ejected by this way.
4. Disconnecting a disk by removing its mount point (Dismounting). if Windows `Diskpart` utility, you can remove disk's mounting letter. if the disk doesn't have any mounting point, the Windows filesystem cannot perform any read or write operations from it.

When the service executes, it uses all of following methods in order to remove a disk. for example if we are editing a file in a disk, methods 1-3 may not work for disconnecting it from Windows, so the Service follows method 4 in order to force disconnecting it.

## How to use
simply run `install.bat` as administrator. it will register the service and start it. it will be accessible via `Windows Service Manager` utility.
if you want to stop and uninstall the service, just run `uninstall.bat`.

### update the database
in order to modify the database file, you have to run the service executable file with `--add-disk` and `--remove-disk` flags, it will shows the list of disks to add/remove from database file. the default database path is *C:\ProgramData\DISKDRM\db.bin*

## Build instructions
first you have to install requirements:
1. [.NET SDK 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2. .NET Framework along with [MSBuild Tools](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022)
3. Chocolatey, gnuwin32, Cygwin, MinGW or any tool for [running Makefiles](https://stackoverflow.com/questions/32127524/how-to-install-and-use-make-in-windows) (Optional)

for building the project just open powershell in projects root and run `make build` or following commands:
    dotnet restore      :: for first time
	dotnet build
for running the service just run `make run` or `dotnet run`.

if you want to install the service, you have to publish the program via `make publish` or following command:

	dotnet publish -o publish -c Release -r win-x64 -p:PublishSingleFile=True

the binary executable service will be placed in */publish* folder.
