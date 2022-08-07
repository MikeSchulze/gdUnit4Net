#!/bin/sh

if [ -z "$GODOT_BIN" ]; then
    echo "'GODOT_BIN' is not set."
    echo "Please set the environment variable  'export GODOT_BIN=/Applications/Godot.app/Contents/MacOS/Godot'"
    exit 1
fi

$GODOT_BIN --no-window -d res://TestRunner.tscn --verbose $*
exit_code=$?
exit $exit_code
