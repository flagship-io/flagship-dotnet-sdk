#!/bin/bash

set -e

dotnet test -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura