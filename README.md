Require
=======

javascript dependency management

Sample : https://github.com/wayjet/Require/tree/master/sample

���Ұ��� "jquery" �İ�
..\bin\require.exe -cmd=list -repository="%cd%" -filter=jquery

��ʾ jquery �� jquery.ui.core ����Ҫ���õİ��嵥
..\bin\require.exe -cmd=detail -repository="%cd%" -require="jquery,jquery.ui.core"

��Ŀ��Ҫʹ�� "jqModal" �� "jquery.ui.droppable 1.10.4"��������Ҫ���õİ�ѹ���� "test.zip" �ļ�
..\bin\require.exe -cmd=zip -repository="%cd%" -output "%cd%/test.zip" -require="jqModal ; jquery.ui.droppable 1.10.4" 