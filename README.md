# TinyTcpServer
A small tcp server working under Mono or .NET and provides hooks for handling data exchange with clients (works under mono and .net) 
It was fully tested with NUnit Tests on single and multi client (parallel) exchange.

Also we written 2 simple protocols over this server in separate project:

Echo server (RFC 862)
Time server (RFC 868)

There is a GUI to run these types of servers and any other types in the near future.

We are planning (in near future) to build any protocol over this tcp server using script Engine. In this case server will server for transport purposes and all protocol logic will be in script (C# lamguage).

We are having nuget package: https://www.nuget.org/packages/MossbauerLab.TinyTcpServer.Core/

Contributors
EvilLord666 aka Ushakov Michael
