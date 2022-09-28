@ECHO OFF
CLS

IF NOT DEFINED GODOT_BIN (
	ECHO "GODOT_BIN is not set."
	ECHO "Please set the environment variable 'setx GODOT_BIN <path to godot.exe>'"
	EXIT /b -1
)

%GODOT_BIN% --no-window -d res://TestRunner.tscn --verbose %*
SET exit_code=%errorlevel%

ECHO %exit_code%

EXIT /B %exit_code%
