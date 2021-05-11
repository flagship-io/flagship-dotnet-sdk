call coverage-ci.bat

"%NUGETFolder%\ReportGenerator.4.8.4\tools\net47\ReportGenerator.exe" -reports:"%current_dir%CoverageResults.xml" -targetdir:"%current_dir%CoverageReport"

start "" "%current_dir%CoverageReport\index.html"