set current_dir=%~dp0
set VSTest="%current_dir%packages\Microsoft.TestPlatform.16.8.3\tools\net451\Common7\IDE\Extensions\TestPlatform\VSTest.Console.exe"
set NUGETFolder=%current_dir%packages
 
"%NUGETFolder%\OpenCover.4.7.922\tools\OpenCover.Console.exe" -target:%VSTest% -targetargs:"%current_dir%Flagship.Tests\bin\Debug\Flagship.Tests.dll" -output:"%current_dir%CoverageResults.xml" -register:Path64