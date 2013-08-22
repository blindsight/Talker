using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;

using MySql.Data.MySqlClient;

namespace Talker
{
	public static class Server
	{
		public enum ColorOptions
		{
			On,
			Off,
			ViewCodes
		}

		static Server()
		{
			TalkerBooted = DateTime.UtcNow;
			//TODO: needs to be thread safe
			ClientList = new List<User>();
			CommandList = new List<ICommand>();

			Assembly asm = Assembly.GetExecutingAssembly();

			foreach (Type type in asm.GetTypes()) {
				if(type.Namespace != null && type.Namespace.Equals("Talker.Commands") 
				   && type.IsClass
				   && type.GetInterface("ICommand") != null) {

					ICommand newCommand = (ICommand)Activator.CreateInstance(type);

					CommandList.Add(newCommand);

					if(newCommand.Name.Equals("say")) {
						Server.DefaultCommand = newCommand;
					}
				}
			}

			ShoutConversation = new List<UserCommuncationBuffer>();
			ColorCodes = new Dictionary<string,string>();
			//TODO: where should this be? what if they want to change how the color codes named?
			//TODO: move to an ansi class thats a IProtocol or something? html?
			//\033 doesn't work so \x1B is the hex version
			ColorCodes.Add("~RS", "\x1B[0m");  /* reset */
			ColorCodes.Add("~OL", "\x1B[1m");            /* bold */
			ColorCodes.Add("~UL", "\x1B[4m");            /* underline */
			ColorCodes.Add("~LI", "\x1B[5m");            /* blink */
			ColorCodes.Add("~RV", "\x1B[7m");            /* reverse */
			/* Foreground colour */
			ColorCodes.Add("~FK", "\x1B[30m");           /* black */
			ColorCodes.Add("~FR", "\x1B[31m");           /* red */
			ColorCodes.Add("~FG", "\x1B[32m");           /* green */
			ColorCodes.Add("~FY", "\x1B[33m");           /* yellow */
			ColorCodes.Add("~FB", "\x1B[34m");           /* blue */
			ColorCodes.Add("~FM", "\x1B[35m");           /* magenta */
			ColorCodes.Add("~FC", "\x1B[36m");           /* cyan */
			ColorCodes.Add("~FW", "\x1B[37m");           /* white */
			/* Background colour */
			ColorCodes.Add("~BK", "\x1B[40m");           /* black */
			ColorCodes.Add("~BR", "\x1B[41m");           /* red */
			ColorCodes.Add("~BG", "\x1B[42m");           /* green */
			ColorCodes.Add("~BY", "\x1B[43m");           /* yellow */
			ColorCodes.Add("~BB", "\x1B[44m");           /* blue */
			ColorCodes.Add("~BM", "\x1B[45m");           /* magenta */
			ColorCodes.Add("~BC", "\x1B[46m");           /* cyan */
			ColorCodes.Add("~BW", "\x1B[47m");           /* white */
			/* Some compatibility names */
			ColorCodes.Add("FT", "\x1B[36m");           /* cyan AKA turquoise */
			ColorCodes.Add("BT", "\x1B[46m");           /* cyan AKA turquoise */

			RoomList = Room.GetAllRooms();
			string roomName = "";

			using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString)) {
				using(MySqlCommand cmd = new MySqlCommand("SELECT * FROM config WHERE configText = 'main_room'", conn)) {
					cmd.Connection.Open();
					
					MySqlDataReader configReader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleResult);
					
					while(configReader.Read()) { //TODO should the while be used if only one row?
						roomName = configReader["configValue"].ToString(); //TODO: do a single value?
					}
					
					cmd.Connection.Close();
				}
			}

			if(!String.IsNullOrEmpty(roomName)) {
				//TODO: THIS could be null.
				Server.LoginRoom = Server.FindRoomByName(roomName);

				if(String.IsNullOrEmpty(Server.LoginRoom.Name)) {
					Console.WriteLine("ERROR: unable to load login room '{0}'", roomName);
					//TOODO: error loading room
				}
			} else {
				Console.WriteLine("ERROR: no main room is set");
				//TODO: error no main room! write somewhere!
			}
		}

		public static List<User> ClientList
		{
			get;
			set;
		}

		public static List<ICommand> CommandList
		{
			get;
			set;
		}

		public static List<Room> RoomList {
			get;
			set;
		}

		public static User FindClientByName(string userName)
		{
			foreach(User CurrentUser in ClientList) {
				if(CurrentUser.Name.ToLower().Equals(userName.ToLower())) {
					return CurrentUser;
				}
			}

			return null;
		}

		public static User FindClientBySessionId(string SessionId)
		{
			foreach(User CurrentUser in ClientList) {
				foreach (IUserConnection UserConnection in CurrentUser.Connections) {
					if (UserConnection.Type == UserConnectionTypes.WebSocket) {
						WebSocketUserCommunication UserCommunication = (WebSocketUserCommunication)UserConnection;

						if (UserCommunication.SessionId() == SessionId) {
							return CurrentUser;
						}
					}

				}
			}

			return null;
		}

		public static Room FindRoomByName(string roomName)
		{
			//I'm doing this as a method b/c in the future I'll have to look for rooms only in the db
			foreach(Room currentRoom in RoomList) {
				if(currentRoom.Name.ToLower().Equals(roomName.ToLower())) {
					return currentRoom;
				}
			}

			return null;
		}

		public static void WriteAll(String clientText)
		{
			foreach (User currentClient in ClientList) {
				currentClient.Write(clientText);
			}
		}

		public static void WriteAllBut(string clientText, List<User> ExceptUsers)
		{
			foreach (User currentClient in ClientList) {
				if(!ExceptUsers.Contains(currentClient)) {
					currentClient.Write(clientText);
				}
			}
		}

		public static DateTime TalkerBooted {
			get;
			set;
		}

		public static Room LoginRoom {
			get;
			set;
		}

		public static ICommand DefaultCommand
		{
			get;
			set;
		}

		public static Dictionary<string, string> ColorCodes {
			get;
			set;
		}

		public static List<UserCommuncationBuffer> ShoutConversation {
			get;
			set;
		}
	}
}

