# Build the project in Release configuration
MSBuild.exe ../Flagship/Flagship.csproj -property:Configuration=Release

# Copy the output libraries in nuget tree folder
New-Item -Path ".\lib\net40" -ItemType "directory" -force
Copy-Item -Path "..\Flagship\bin\Release\net40\Flagship*.dll" -Destination ".\lib\net40" -Recurse -force
Copy-Item -Path "..\Flagship\bin\Release\net40\Flagship*.pdb" -Destination ".\lib\net40" -Recurse -force
Copy-Item -Path "..\Flagship\bin\Release\net40\Flagship*.xml" -Destination ".\lib\net40" -Recurse -force

New-Item -Path ".\lib\net45" -ItemType "directory" -force
Copy-Item -Path "..\Flagship\bin\Release\net45\Flagship*.dll" -Destination ".\lib\net45" -Recurse -force
Copy-Item -Path "..\Flagship\bin\Release\net45\Flagship*.pdb" -Destination ".\lib\net45" -Recurse -force
Copy-Item -Path "..\Flagship\bin\Release\net45\Flagship*.xml" -Destination ".\lib\net45" -Recurse -force

New-Item -Path ".\lib\netstandard2.0" -ItemType "directory" -force
Copy-Item -Path "..\Flagship\bin\Release\netstandard2.0\Flagship*.dll" -Destination ".\lib\netstandard2.0" -Recurse -force
Copy-Item -Path "..\Flagship\bin\Release\netstandard2.0\Flagship*.pdb" -Destination ".\lib\netstandard2.0" -Recurse -force
Copy-Item -Path "..\Flagship\bin\Release\netstandard2.0\Flagship*.xml" -Destination ".\lib\netstandard2.0" -Recurse -force

New-Item -Path ".\lib\net5.0" -ItemType "directory" -force
Copy-Item -Path "..\Flagship\bin\Release\net5.0\Flagship*.dll" -Destination ".\lib\net5.0" -Recurse -force
Copy-Item -Path "..\Flagship\bin\Release\net5.0\Flagship*.pdb" -Destination ".\lib\net5.0" -Recurse -force
Copy-Item -Path "..\Flagship\bin\Release\net5.0\Flagship*.xml" -Destination ".\lib\net5.0" -Recurse -force

# Create the nuget package
nuget pack -OutputFileNamesWithoutVersion Flagship.nuspec 