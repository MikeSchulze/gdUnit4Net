# !/usr/bin/env bash

rm -rf ./reports
$GODOT_BIN . --headless --build-solutions --quit-after 100 --verbose
echo "Compile exit: $?"


echo "Run tests"

dotnet test --settings .runsettings --results-directory ./reports
echo "Exit Code: $?"
