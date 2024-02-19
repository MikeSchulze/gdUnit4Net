rm -rf ./bin
rm -rf ./obj
rm -rf ./api/bin
rm -rf ./api/obj
rm -rf ./test/.godot
rm -rf ./testadapter/.godot
rm -rf ./testadapter/bin
rm -rf ./testadapter/obj
rm -rf ./testadapter/obj
rm -rf ./test/.godot
rm -rf ./test/gdunit4_testadapter
rm -rf ./example/.godot
rm -rf ./example/gdunit4_testadapter

dotnet clean
dotnet restore
dotnet build
