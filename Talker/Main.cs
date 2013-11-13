using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using SuperWebSocket;
using SuperSocket;
using SuperSocket.SocketBase;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Talker
{
	class Talker
	{
		private static TcpListener tcpListener;
		private static Thread listenThread;
		static SuperWebSocket.WebSocketServer ws;

		public static void Main(string[] args)
		{
			// Check if the config file is correctly loaded
			if (System.Configuration.ConfigurationManager.AppSettings.Count == 0) {
				Console.WriteLine("ERROR: config file not loaded");
				return;
			}

			int AddressPort = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["port"]);
			int webSocketPort = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["webSocketPort"]);

			//TODO: error message port must be above 1024
			tcpListener = new TcpListener(IPAddress.Any, AddressPort);
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
			System.Reflection.Assembly CurrentServer = System.Reflection.Assembly.GetExecutingAssembly();
			string ProductVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(CurrentServer.Location).ProductVersion;
			DateTime CurrentDateTime = DateTime.Now;

			Console.WriteLine("------------------------------------------------------------------------------");
			Console.WriteLine("{0} {1} server booting on {2} at {3}", CurrentServer.GetName().Name, ProductVersion, CurrentDateTime.ToLongDateString(), CurrentDateTime.ToLongTimeString());
			Console.WriteLine("------------------------------------------------------------------------------");
			Console.WriteLine("Node Name : {0}", System.Environment.MachineName);

		
			//Console.WriteLine("Running On : {0} {1}", (Is64) ? "x86_64": "x86" ,System.Environment.OSVersion.ToString());
			Console.WriteLine("Started server at " + AddressPort);

			SuperSocket.SocketBase.Config.RootConfig r = new SuperSocket.SocketBase.Config.RootConfig();

			SuperSocket.SocketBase.Config.ServerConfig s = new SuperSocket.SocketBase.Config.ServerConfig();
			s.Name = "SuperWebSocket";
			s.Ip = "Any";
			s.Port = webSocketPort;
			s.Mode = SocketMode.Tcp;

			SuperSocket.SocketEngine.SocketServerFactory f = new SuperSocket.SocketEngine.SocketServerFactory();


			if (ws != null)
			{
				ws.Stop();
				ws = null;
			}

			ws = new WebSocketServer();
			ws.Setup(r, s, f);
			ws.SessionClosed += new SessionHandler<WebSocketSession, CloseReason>(ws_SessionClosed);
			ws.NewSessionConnected += new SessionHandler<WebSocketSession>(ws_NewSessionConnected);
			ws.NewMessageReceived += new SessionHandler<WebSocketSession, string>(ws_NewMessageReceived);
			ws.NewDataReceived += new SessionHandler<WebSocketSession, byte[]>(ws_NewDataReceived);
			ws.Start();

			Console.WriteLine("------------------------------------------------------------------------------");
			Console.WriteLine("Booted with PID {0}", System.Diagnostics.Process.GetCurrentProcess().Id);
			Console.WriteLine("------------------------------------------------------------------------------");
		}

		private static void ListenForClients()
		{
			tcpListener.Start();

			while (true) {
				//should talker move to async?
				TcpClient client = tcpListener.AcceptTcpClient();

				Random userNamer = new Random();
				int userNumber = userNamer.Next(0, 300);

				TcpUserCommunication tcpClient = new TcpUserCommunication(client);

				User UserObj = new User(new List<IUserConnection>() { tcpClient }, userNumber);
				tcpClient.User = UserObj;
				AsyncCallback callBack = new AsyncCallback(ProcessUserRead);

				client.GetStream().BeginRead(tcpClient.readingBytes, 0, 4096, callBack, tcpClient);

				ConnectUser(UserObj, tcpClient);
			}
		}

		static void ws_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason reason)
		{
			User userObj = Server.FindClientBySessionId(session.SessionID);

			foreach (IUserConnection userConnection in userObj.Connections) {
				if (userConnection.Type == UserConnectionTypes.WebSocket) {
					WebSocketUserCommunication connection = (WebSocketUserCommunication)userConnection;

					if (connection.SessionId() == session.SessionID) {
						userConnection.Close();

						List<IUserConnection> userConnectionList = new List<IUserConnection>();

						userConnectionList.AddRange(userObj.Connections);
						userConnectionList.Remove(userConnection);
						userObj.Connections = new ReadOnlyCollection<IUserConnection>(userConnectionList);
						//userObj.Connections.Remove(userConnection);
						break;
					}
				}
			}

			if (userObj.Connections.Count <= 0) {
				Server.ClientList.Remove(userObj);
				//TODO: just do quit command instead.
				userObj.Save();
			}

				//TODO: deal with multi user login?
		}

		static void ws_NewSessionConnected(WebSocketSession session)
		{
			WebSocketUserCommunication UserConnection = new WebSocketUserCommunication(session);

			Random userNamer = new Random();
			int userNumber = userNamer.Next(0, 999);

			User UserObj = new User(new List<IUserConnection>() { UserConnection }, userNumber);

			ConnectUser(UserObj, UserConnection);
		}

		private static void HandleClientCommunication(User userObj, string userInput)
		{
			UserInput CurrentInput = new UserInput(userObj, userInput);

			if (userInput.StartsWith(".")) {
				if (userInput.Trim().Length == 1) {
					userObj.LastCommand.Run(userObj.LastInput);
					return;
				}

				//TODO: need to check partial input as well
				ICommand CurrentCommand = Server.CommandList.Find(x => x.Name.Equals(CurrentInput.Args [0].ToLower()));

				if (CurrentCommand != null) {
					userObj.LastCommand = CurrentCommand;
					userObj.LastInput = CurrentInput;

					CurrentCommand.Run(CurrentInput);
				} else {
					userObj.WriteLine("Unknown command.");
				}
			} else {
				//TODO: setup default command thing.. 
				Server.DefaultCommand.Run(CurrentInput);
			}
		}

		static void ws_NewDataReceived(WebSocketSession session, byte[] e)
		{
			session.Send("Data?");
		}

		static void ws_NewMessageReceived(WebSocketSession session, string e)
		{
			User userObj = Server.FindClientBySessionId(session.SessionID);
			HandleClientCommunication(userObj, e);
		}

		private static void ProcessUserRead(IAsyncResult result)
		{
			TcpUserCommunication currentConnection = (TcpUserCommunication)result.AsyncState;
			User userObj = currentConnection.User;
			//NetworkStream stream = (NetworkStream)result;

			int bytesRead = currentConnection.Stream.EndRead(result);
			UTF8Encoding encoder = new UTF8Encoding();
			string userInput = encoder.GetString(currentConnection.readingBytes, 0, bytesRead).Trim();
			HandleClientCommunication(userObj, userInput);

			AsyncCallback callBack = new AsyncCallback(ProcessUserRead);
			currentConnection.Stream.BeginRead(currentConnection.readingBytes, 0, 4096, callBack, currentConnection);
		}

		private static void ConnectUser(User UserObj, IUserConnection userConnection) {
			UserObj.Logon = DateTime.UtcNow;
			UserObj.Room = Server.LoginRoom;
			UserObj.Room.Users.Add(UserObj);

			Server.ClientList.Add(UserObj);

			List<User> WriteAllButUsers = new List<User>();

			Server.ClientList.ForEach(delegate(User currentUser) {
				if(currentUser.Ignores.HasFlag(User.Ignore.Logons)
				   || currentUser.Ignores.HasFlag(User.Ignore.All)) {
					WriteAllButUsers.Add(currentUser);
				}
			});

			Server.CommandList.ForEach(delegate(ICommand command) {
				if(command.Name.Equals("look")) {
					UserInput newInput = new UserInput(UserObj, "look");
					command.Run(newInput);
				}
			});

			Server.WriteAllBut("[Entering is: " + UserObj.Name + " " + UserObj.Desc + " ] \n", WriteAllButUsers);
		}
	}
}
