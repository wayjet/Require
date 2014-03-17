echo "显示依赖的信息"
..\issue\require.exe -cmd=zip -repository="%cd%" -output "%cd%/test.zip" -require="jqModal ; jquery.ui.droppable 1.10.4" 
pause