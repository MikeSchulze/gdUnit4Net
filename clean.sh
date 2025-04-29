rm -rf ./bin
rm -rf ./obj
rm -rf ./Api/nupkg/*
rm -rf ./Api/obj

rm -rf .godot

rm -rf ./test/.godot
rm -rf ./test/gdunit4_testadapter

rm -rf ./example/.godot
rm -rf ./example/gdunit4_testadapter


rm -rf ./TestAdapter/nupkg/*
rm -rf ./TestAdapter/obj


dotnet build ./Api/GdUnit4Api.csproj
dotnet build ./TestAdapter/GdUnit4TestAdapter.csproj
dotnet restore
dotnet build


$GODOT_BIN --path ./example --headless --build-solutions --quit-after 20
# dotnet clean
# dotnet restore
# dotnet build
# "dotnet.unitTests.runSettingsPath": "./test/.runsettings"
