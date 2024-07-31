sh ./test-run-ci.sh

dotnet reportgenerator "-reports:./coverage.cobertura.xml" "-targetdir:coveragereport" "-reporttypes:Html"