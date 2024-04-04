rm -rf ./bin
rm -rf ./obj
rm -rf ./api/nupkg/*
rm -rf ./api/obj

rm -rf .godot

rm -rf ./test/.godot
rm -rf ./test/gdunit4_testadapter

rm -rf ./example/.godot
rm -rf ./example/gdunit4_testadapter


rm -rf ./testadapter/nupkg/*
rm -rf ./testadapter/obj


dotnet build ./api/gdUnit4Api.csproj
dotnet build ./testadapter/gdUnit4TestAdapter.csproj
dotnet restore
dotnet build


$GODOT_BIN --path ./example --headless --build-solutions --quit-after 20
# dotnet clean
# dotnet restore
# dotnet build
# "dotnet.unitTests.runSettingsPath": "./test/.runsettings"
