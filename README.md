# TinyTcpServer

# 1. OVERVIEW
A small tcp server working under Mono or .NET and provides hooks for handling data exchange with MULTIPLE clients (works under mono and .net) and BEHAVIOUR CUSTOMIZATION via C# SCRIPT.
It was fully tested with NUnit Tests on single and multi client (parallel) exchange.

Also we written 2 simple implementations (protocols) over ITcpServer in separate project:
Echo server (RFC 862)
Time server (RFC 868)

# 2. SOLUTION STRUCTURE
/
----/Console
----/GUI
----TinyTcpServer/
                  ----MossbauerLab.TinyTcpServer.Core
                  ----MossbauerLab.TinyTcpServer.Core.FunctionalTests
                  ----MossbauerLab.SimpleExtensions
                  ----MossbauerLab.SimpleExtensions.Tests
                  
/Console is a console project with management console (build for FlexibleTcpServer working with scripts)
/GUI is a Windows Forms Tool for management server (build for FlexibleTcpServer working with scripts)
/TinyTcpServer is a solution with server interface and it extensions (differnt implementation) including FlexibleTcpServer

# 3. NUGET PACKAGE
https://www.nuget.org/packages/MossbauerLab.TinyTcpServer.Core/

# 4. FULL EXAMPLE OF HOW TO USE

Contributors
EvilLord666 aka Ushakov Michael
KatanaZZZ aka Anonymous
