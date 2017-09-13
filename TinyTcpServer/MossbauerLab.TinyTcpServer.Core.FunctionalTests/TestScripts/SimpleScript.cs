using System;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.Flexibility
{
	public class ServerScript
	{
		public void Init(ref ITcpServer server)
		{
			if(server == null)
				throw new NullReferenceException("server");
			_server = server;
			Console.WriteLine("Init....");
		}
		
		private ITcpServer _server;
	}
}
