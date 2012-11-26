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
				UserObj.Logon = DateTime.UtcNow;

				Server.ClientList.Add(UserObj);
				clientThread.Start(UserObj);
				UserObj.WriteLine("[Entering is: " + UserObj.Name + " " + UserObj.Desc + " ] ");
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
					int userInputStart = userInput.IndexOf(' ');
					userInputStart = ( userInputStart>= 0) ? userInputStart : 0; //make sure the index is above 0
					string userMessage = userInput.Remove(0, userInputStart);

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
							userObj.WriteLine("Your name has been changed to \"" + userObj.Name + "\"");
						break;
						case "desc":
							if(userCommands.Length < 2) {
								userObj.WriteLine("Your current description is: " + userObj.Desc);
								break;
							}
						
							userObj.Desc = userMessage;
							userObj.WriteLine("Your description has been changed to \"" + userObj.Desc + "\"");
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
								userObj.WriteLine("No such named \"" + userTo + "\"user.");
							} else {
								userObj.WriteLine("you tell " + userTo + ": " + messageTo);
								userObjTo.WriteLine(userObj.Name + " tells you:" + messageTo);
							}
						break;
						case "emote":
							if(userCommands.Length < 2) {
								userObj.WriteLine("Usage: emote <text>\n");
								break;
							}
						
							Server.WriteAll( userObj.Name + "" + userMessage);
						break;
						case "who":
							
							userObj.WriteLine("+----------------------------------------------------------------------+");
							userObj.WriteLine("|  Name      Description                                      |  Mins  |");
							userObj.WriteLine("+----------------------------------------------------------------------+");
						
							string whoLines = "";
						
							DateTime commonTime = DateTime.UtcNow;
						
							foreach(User CurrentUser in Server.ClientList) {
								TimeSpan minsOnline = commonTime - CurrentUser.Logon;
							
								whoLines += String.Format("| {0,-59} | {1,-6} |\n", CurrentUser.Name + CurrentUser.Desc, minsOnline.Minutes);
							}

							userObj.Write(whoLines);
							userObj.WriteLine("+----------------------------------------------------------------------+");

						break;
						case "version":
							userObj.WriteLine("+----------------------------------------------------------------------------+");
							userObj.WriteLine("|                           Your Talker's Name Here                          |");
							userObj.WriteLine("+----------------------------------------------------------------------------+");
							userObj.WriteLine(String.Format("| Total number of users    : {0}                             |", Server.ClientList.Count));
	/*| Logons this current boot :    1 new users,    1 old users                  |
	| Total number of users    : 3     Maximum online users     : 50             |
	| Total number of rooms    : 15    Swear ban currently on   : OFF            |
	| Smail auto-forwarding on : YES   Auto purge on            : NO             |
	| Maximum smail copies     : 6     Names can be recapped    : YES            |
	| Personal rooms active    : YES   Maximum user idle time   : 60  mins       |*/
							userObj.WriteLine("+----------------------------------------------------------------------------+");
							userObj.WriteLine("| 0.0.1                                        (C) Timothy Rhodes 2012       |");
							userObj.WriteLine("+----------------------------------------------------------------------------+");
						break;
						default:
							userObj.WriteLine("Unknown command.");
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
