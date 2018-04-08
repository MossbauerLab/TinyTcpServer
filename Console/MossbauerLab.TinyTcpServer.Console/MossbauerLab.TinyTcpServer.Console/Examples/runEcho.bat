SET TinyTcpServerConsole="..\bin\Debug\MossbauerLab.TinyTcpServer.Console.exe"
SET IPAddress="127.0.0.1"
SET Port=6666
SET EchoScript="Echo.cs"
SET EchoOptions="echoOptions.txt"
%TinyTcpServerConsole% --start --ipaddr=%IPAddress% --port=%Port% --script=%EchoScript% --compilerOptions=%EchoOptions%
pause