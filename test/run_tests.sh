#!/usr/bin/env bash
rm -rf ./reports
dotnet build

dotnet test --settings .runsettings --results-directory ./reports
echo "Exit Code: $?"
