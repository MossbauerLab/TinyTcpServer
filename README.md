# TinyTcpServer

# 1. OVERVIEW
A small tcp server working under Mono or .NET (4.0) and provides hooks for handling data exchange with MULTIPLE clients (works under mono and .net) and BEHAVIOUR CUSTOMIZATION via C# SCRIPT.
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
`

    private const String LocalIpAddress = "127.0.0.1"; 
    private const UInt16 ServerPort = 8044;   
    private const String Script = @"..\..\TestScripts\SimpleScript.cs";
        
    private ITcpServer _server;

    public void Init()
    {
        CompilerOptions options = new CompilerOptions();
        CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<String, String>()
        {
            {"CompilerVersion", "v4.0"}
        });
        options.Parameters = new CompilerParameters(new string[] {"System.Web"});
        options.Parameters.GenerateExecutable = false;
        options.Parameters.GenerateInMemory = true;
        options.ScriptEntryType = "TestEchoScript.EchoScript";
        
        _server = new FlexibleTcpServer(Script, LocalIpAddress, ServerPort);    
    }
`

/*That is all ! all logics is inside you script
There are requirement to presence of initial class and entry method. Full examples could be found in
- MossbauerLab.TinyTcpServer.Core.FunctionalTests/Server/TestFlexibleTcpServer.cs
- In Console and GUI projects with Utils class for getting CompilerOptions and TcpServerConfig from very simple files (Key=Value)
*/

`

     public class ServerScript
     {
         public void Init(ref ITcpServer server)
         {
             if(server == null)
                 throw new NullReferenceException("server");
             _server = server;
             _connectHandlerId = Guid.NewGuid();
             _dataHandlerId = Guid.NewGuid();
             //Console.WriteLine("Init....");
             _server.AddConnectionHandler(_connectHandlerId, OnClientConnection);
             _server.AddHandler(new TcpClientHandlerInfo(_dataHandlerId), OnClientExchange);
         }
         // ...
     }
 
     // in this method we set up handlers
     // Handlers on Connect and Exchange looks like:
     public Byte[] OnClientExchange(Byte[] receivedData, TcpClientHandlerInfo info)
     {
         lock (receivedData)
         {
             Byte[] outputData = new Byte[receivedData.Length];
             Array.Copy(receivedData, outputData, receivedData.Length);
             return outputData;
         }
     }
     
     // connect true if client connected and false if disconnected 
     public void OnClientConnection(TcpClientContext context, Boolean connect)  
     
     {
            
     }
 `
 
 Full example present (in file SimpleScript inside MossbauerLab.TinyTcpServer.FunctionalTests
 
 # 5 Expanded setting
 There are additional settings for TcpServer -> see class TcpServerSettings.cs (MossbauerLab.TinyTcpServer.Core)
 In Console project there is a class that could parse config ftle (key=value) with that settings class is TcpServerConfigBuilder
 
 it handles file, examples of settings:
 `
 
     # number of clients processing the 'same time'
     ParallelTask = 256
     # buffer on receive for every client (in bytes)
     ClientBufferSize = 65535
     # chunk is a auant of size for read and write operations
     ChunkSize = 4096
     # number of times in a row that calls BeginAccept (in a sepatarate from IO processing thread)
     ClientConnectAttempts = 4
     # time while client stays inactive, after this time is off (in seconds) client will be disconneced by server
     ClientInactivityTime = 120
     # timeout for BeginAccept in milliseconds
     ClientConnectTimeout = 1000
     # number of attempts in a row to get data from client
     ClientReadAttempts = 8
     # timeout in milliseconds on every read attemp 
     ReadTimeout = 200
     # timeout in milliseconds for server to shutdown, close all opened resources
     ServerCloseTimeout = 2000
     # timeout in milliseconds to complete write operation
     WriteTimeout = 1000 
`
 # 6 CONTRIBUTORS
     EvilLord666 aka Ushakov Michael
     KatanaZZZ aka Anonymous
