set current_dir=%~dp0
set VSTest="%current_dir%packages\Microsoft.TestPlatform.17.6.0\tools\net462\Common7\IDE\Extensions\TestPlatform\VSTest.Console.exe"
set NUGETFolder=%current_dir%packages
 
%VSTest% "%current_dir%Flagship.Tests\bin\Debug\Flagship.Tests.dll" /InIsolation /EnableCodeCoverage /logger:trx