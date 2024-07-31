@echo off

dotnet test -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura
IF ERRORLEVEL 1 EXIT /B 1