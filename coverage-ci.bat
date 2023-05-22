set current_dir=%~dp0
set VSTest="%current_dir%packages\Microsoft.TestPlatform.17.6.0\tools\net462\Common7\IDE\Extensions\TestPlatform\VSTest.Console.exe"
set NUGETFolder=%current_dir%packages
 
"%NUGETFolder%\OpenCover.4.7.1221\tools\OpenCover.Console.exe" -target:%VSTest% -targetargs:"%current_dir%Flagship.Tests\bin\Debug\Flagship.Tests.dll" -output:"%current_dir%CoverageResults.xml" -register:path32