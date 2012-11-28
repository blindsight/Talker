using System;
using System.Data;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace Talker
{
	public class Room
	{
		public Room()
		{
			Users = new List<User>();
			Review = new List<UserCommuncationBuffer>();
		}

		public Room(long roomIdToLoad) : this()
		{
			using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString)) {
				using(MySqlCommand cmd = new MySqlCommand("SELECT * FROM rooms WHERE roomId = ?roomId", conn)) {
					cmd.Parameters.AddWithValue("?roomId", roomIdToLoad);
					cmd.Connection.Open();
					
					MySqlDataReader roomReader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleResult);
					
					while(roomReader.Read()) { //TODO should the while be used if only one row?
						this.Id = long.Parse(roomReader["roomId"].ToString());
						this.Name = roomReader["name"].ToString();
						this.Desc = roomReader["desc"].ToString(); //todo: should try parse be used for safety?
						this.Topic = roomReader["topic"].ToString();
						
						//TODO: could add total users entered and left or something for stats?
					}
					
					cmd.Connection.Close();
				}
			}
		}

		public Room(string roomNameToLoad)
		{
			using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString)) {
				using(MySqlCommand cmd = new MySqlCommand("SELECT * FROM rooms WHERE name = ?roomname", conn)) {
					cmd.Parameters.AddWithValue("?roomname", roomNameToLoad);
					cmd.Connection.Open();

					MySqlDataReader roomReader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleResult);
					
					while(roomReader.Read()) { //TODO should the while be used if only one row?
						this.Id = long.Parse(roomReader["roomId"].ToString());
						this.Name = roomReader["name"].ToString();
						this.Desc = roomReader["desc"].ToString(); //todo: should try parse be used for safety?
						this.Topic = roomReader["topic"].ToString();

							//TODO: could add total users entered and left or something for stats?
					}
					
					cmd.Connection.Close();
				}
			}
		}

		public void Write(string message)
		{
			this.Users.ForEach(x => x.Write(message));
		}

		public void WriteLine(string message)
		{
			this.Write(message + "\n");
		}

		public void WriteAllBut(string message, List<User> ExceptUsers)
		{
			if(Users != null) {
				foreach(User currentUser in Users) {
					if(!ExceptUsers.Contains(currentUser)) {
						currentUser.Write(message);
					}
				}
			}
		}

		public static List<Room> GetAllRooms()
		{
			List<Room> rooms = new List<Room>();

			using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString)) {
				using(MySqlCommand cmd = new MySqlCommand("SELECT * FROM rooms", conn)) {
					cmd.Connection.Open();
					
					MySqlDataReader roomReader = cmd.ExecuteReader();
					
					while(roomReader.Read()) { //TODO should the while be used if only one row?
						Room newRoom = new Room();
						newRoom.Id = long.Parse(roomReader["roomId"].ToString());
						newRoom.Name = roomReader["name"].ToString();
						newRoom.Desc = roomReader["desc"].ToString(); //todo: should try parse be used for safety?
						newRoom.Topic = roomReader["topic"].ToString();
						
						//TODO: could add total users entered and left or something for stats?
						rooms.Add(newRoom);
					}
					
					cmd.Connection.Close();
				}
			}

			return rooms;
		}

		public long Id {
			get;
			protected set;
		}

		public List<User> Users {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public string Desc {
			get;
			set;
		}

		public string Topic {
			get;
			set;
		}

		public List<UserCommuncationBuffer> Review {
			get;
			protected set;
		}
	}
}

