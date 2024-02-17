rm -rf ./bin
rm -rf ./obj
rm -rf ./api/bin
rm -rf ./api/obj
rm -rf ./test/.godot
rm -rf ./testadapter/.godot
rm -rf ./testadapter/bin
rm -rf ./testadapter/obj
rm -rf ./testadapter/obj

dotnet restore
dotnet build

echo "rebuild project cache"
#cd test
#$GODOT_BIN --path . -e --quit --headless --build-solutions

echo "run tests"
#./addons/gdUnit4/runtest.sh --add res://test/src

#dotnet test ./test/gdUnit4Test.csproj --no-build --settings ./test/.runsettings-ci
