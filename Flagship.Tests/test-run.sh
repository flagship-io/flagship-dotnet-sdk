#!/bin/bash

# Ensure the script stops if an error occurs
set -e

# Run tests with Coverlet to collect coverage data, outputting in Cobertura format
dotnet dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Use ReportGenerator to convert the coverage report to HTML
# Replace "coverage.cobertura.xml" with the actual path to your Cobertura coverage report if different
# The output will be placed in a folder named "coveragereport"
dotnet reportgenerator "-reports:coverage.cobertura.xml" "-targetdir:coveragereport" "-reporttypes:Html"

# Open the coverage report in the default web browser (Mac specific command)
# For Linux, you might use xdg-open, and for Windows, start
open coveragereport/index.html