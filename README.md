Require
=======

javascript dependency management

Sample : https://github.com/wayjet/Require/tree/master/sample

查找包含 "jquery" 的包
..\bin\require.exe -cmd=list -repository="%cd%" -filter=jquery

显示 jquery 和 jquery.ui.core 所需要引用的包清单
..\bin\require.exe -cmd=detail -repository="%cd%" -require="jquery,jquery.ui.core"

项目需要使用 "jqModal" 和 "jquery.ui.droppable 1.10.4"，将所需要引用的包压缩到 "test.zip" 文件
..\bin\require.exe -cmd=zip -repository="%cd%" -output "%cd%/test.zip" -require="jqModal ; jquery.ui.droppable 1.10.4" 