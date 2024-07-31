@echo off

call test-run-ci.bat

dotnet reportgenerator "-reports:./coverage.cobertura.xml" "-targetdir:coveragereport" "-reporttypes:Html"
IF %ERRORLEVEL% NEQ 0 EXIT /B %ERRORLEVEL%