
@ECHO OFF

SET Language=%~1
SET Full=%~2

IF ["%Language%"]==[""] SET Language=CS
IF ["%Full%"]==[""] SET Full=true

ECHO.
ECHO Generating All for %Language%...
ECHO.

ECHO|SET /p=Generating NOT.... 
node ByteArray.js NOT %Language% %Full%
ECHO Done.

ECHO|SET /p=Generating INC.... 
node ByteArray.js INC %Language% %Full%
ECHO Done.

ECHO|SET /p=Generating DEC.... 
node ByteArray.js DEC %Language% %Full%
ECHO Done.

ECHO|SET /p=Generating REV.... 
node ByteArray.js REV %Language% %Full%
ECHO Done.

ECHO|SET /p=Generating AND.... 
node ByteArray.js AND %Language% %Full%
ECHO Done.

ECHO|SET /p=Generating NAND... 
node ByteArray.js NAND %Language% %Full%
ECHO Done.

ECHO|SET /p=Generating OR..... 
node ByteArray.js OR %Language% %Full%
ECHO Done.

ECHO|SET /p=Generating NOR.... 
node ByteArray.js NOR %Language% %Full%
ECHO Done.

ECHO|SET /p=Generating XOR.... 
node ByteArray.js XOR %Language% %Full%
ECHO Done.

ECHO|SET /p=Generating XNOR... 
node ByteArray.js XNOR %Language% %Full%
ECHO Done.
