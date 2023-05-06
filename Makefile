all :: build run

build ::
	dotnet restore
	dotnet build

run ::
	dotnet run

clean ::
	dotnet clean

publish ::
	dotnet publish -o publish -c Release -r win-x64 -p:PublishSingleFile=True

.PHONY: all clean run build publish