using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;

using MySql.Data.MySqlClient;

namespace Talker
{
	public static class Server
	{
		static Server()
		{
			TalkerBooted = DateTime.UtcNow;
			//TODO: needs to be thread safe
			ClientList = new List<User>();
			CommandList = new List<ICommand>();

			Assembly asm = Assembly.GetExecutingAssembly();

			foreach (Type type in asm.GetTypes()) {
				if(type.Namespace.Equals("Talker.Commands") 
				   && type.IsClass
				   && type.GetInterface("ICommand") != null) {

					ICommand newCommand = (ICommand)Activator.CreateInstance(type);

					CommandList.Add(newCommand);

					if(newCommand.Name.Equals("say")) {
						Server.DefaultCommand = newCommand;
					}
				}
			}


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
	}
}

