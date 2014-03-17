set nuget=..\..\tools\nuget
set outputDir=..\..\packages\jquery\

if not exist "%outputDir%" mkdir "%outputDir%"


call %nuget% pack jquery.nuspec -version 2.1.0 -OutputDirectory %outputDir% -properties "releaseNotes=http://blog.jquery.com/2014/01/24/jquery-1-11-and-2-1-released/"
call %nuget% pack jquery.nuspec -version 1.11.0 -OutputDirectory %outputDir% -properties "releaseNotes=http://blog.jquery.com/2014/01/24/jquery-1-11-and-2-1-released/"