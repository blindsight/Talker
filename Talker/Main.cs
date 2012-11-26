using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace Talker
{
	class Talker
	{
		private static TcpListener tcpListener;
		private static Thread listenThread;

		public static void Main(string[] args)
		{
			int AddressPort = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["port"]);
			//TODO: error message port must be above 1024
			tcpListener = new TcpListener(IPAddress.Any, AddressPort);
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
			Console.WriteLine("Started server at " + AddressPort);
		}

		private static void ListenForClients()
		{
			tcpListener.Start();
			//clientList = new List<User>();

			while (true) {
				TcpClient client = tcpListener.AcceptTcpClient();

				Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));

				
				Random userNamer = new Random();
				int userNumber = userNamer.Next(0, 300);

				User UserObj = new User(client, userNumber);

				Server.ClientList.Add(UserObj);
				clientThread.Start(UserObj);

				UserObj.WriteLine("User connected: " + userNumber);
			}
		}

		private static void HandleClientCommunication(object client)
		{
			User userObj = (User)client;
			NetworkStream clientStream = userObj.Stream;

			byte[] message = new byte[4096];
			int bytesRead;

			while(true) {
				bytesRead = 0;

				try {
					bytesRead = clientStream.Read(message, 0, 4096);
				} catch {
					break;
				}

				UTF8Encoding encoder = new UTF8Encoding();
				string userInput = encoder.GetString(message, 0, bytesRead).Trim();

				if(userInput.StartsWith(".")) {
					string userCommandText = userInput.Remove(0,1);
					string[] userCommands = userCommandText.Split(' ');
					string userMessage = userInput.Remove(0, userInput.IndexOf(' '));

					switch(userCommands[0].ToLower()) {
						case "quit":
							userObj.WriteLine("Thank you for coming and Goodbye!");
							Server.ClientList.Remove(userObj);
							userObj.Quit();
						break;
						case "shout":
							Server.WriteAll("Broadcast: " + userMessage + "\n");
						break;
						case "name":
							if(userCommands.Length < 2) {
								userObj.WriteLine(".name <new name>");
								break;
							}

							userObj.Name = userCommands[1];
							userObj.WriteLine("Your name has been changed to " + userObj.Name);
						break;
						case "tell":
							if(userCommands.Length < 2) {
								userObj.WriteLine(".tell <user> <message>");
								break;
							}

							string userTo = userMessage.Substring(0, userMessage.IndexOf(' '));
							string messageTo = userMessage.Substring(userMessage.IndexOf(' '));

							User userObjTo = Server.FindClientByName(userTo);	

							if(userObjTo == null) {
								userObj.WriteLine("No such user");
							} else {
								userObj.WriteLine("you tell " + userTo + ": " + messageTo);
								userObjTo.WriteLine(userObj.Name + " tells you:" + messageTo);
							}
						break;
						default:
							userObj.WriteLine("You say: " + userInput);
							Server.WriteAllBut( userObj.Name + " Says: " + userInput + "\n", new List<User>{ userObj});
						break;
					}
				} else {
					userObj.WriteLine("You say: " + userInput);
					Server.WriteAllBut( userObj.Name + " Says: " + userInput + "\n", new List<User>{ userObj});
				}
			}
		}
	}
}
