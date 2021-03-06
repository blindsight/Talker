using System;
using System.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Data.SqlTypes;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Talker
{
	public class User
	{
		private string name;
		private ReadOnlyCollection<IUserConnection> connections;

		public enum LoginResult {
			ValidLogin,
			InvalidLogin,
			OpenUserName
		}

		[Flags]
		public enum Ignore : uint {
			None = 0, 
			Beeps = 1, //TODO: add beeps
			Greets = 2, //TODO: add greets
			Logons = 4,
			Pics = 8,
			Shout = 16,
			Tell = 32,
			User = 64,//TODO: add ignore user
			All = 4294967295 //max value for 32 bit int. The previous values need to be powers of two up to this value. which is 2^32
		}

		public User(List<IUserConnection> clientConnections, int clientIndex)
		{
			Connections = new ReadOnlyCollection<IUserConnection>(clientConnections);
			//client = clientConnection;
			name = "User_" + clientIndex;
			this.Desc = " is a newbie needing a description. ";
			this.TellBuffer = new List<UserCommuncationBuffer>();
			this.TotalLogins = 0;
			this.InMsg = "enters";
			this.OutMsg = "goes";
			this.ColorOption = Server.ColorOptions.On;
			this.Ignores = User.Ignore.None;
		}

		public void Write(string clientText)
		{
			foreach (IUserConnection userConnection in this.Connections) {
				clientText += "~RS"; //ALWAYS had RS to make sure color doesn't bleed
				//this is the first idea I've got for color parsing.
				//forevery color there willbe another look.. this could get processing heavy

				if (this.ColorOption != Server.ColorOptions.ViewCodes) {
					string replaceValue = "";

					foreach (KeyValuePair<string, string> colorCode in Server.ColorCodes) {

						if (this.ColorOption == Server.ColorOptions.On) {
							replaceValue = colorCode.Value;
						}

						clientText = clientText.Replace(colorCode.Key, replaceValue);
					}
				}

				if (userConnection.Type == UserConnectionTypes.WebSocket) {
					JsonObject JObject = new JsonObject();
					JsonValue jsonText = new JsonValue();

					JObject.Add("chat", clientText);
					userConnection.Write(JObject);
				} else {
					userConnection.Write(clientText);
				}
			}
		}

		public void WriteLine(string clientText)
		{
			this.Write(clientText + "\n");
		}

		public void ChangeRoom(Room newRoom)
		{
			//take the user out of the room so that write to room is faster. 
			this.Room.Users.Remove(this);
			this.Room = newRoom;
			this.Room.Users.Add(this);

			//TODO: find a better way to do this
			Server.CommandList.ForEach(delegate(ICommand command) {
				if(command.Name.Equals("look")) {
					UserInput newInput = new UserInput(this, "look");
					command.Run(newInput);
				}
			});
		}

		public LoginResult Login(string userName)
		{
			using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString)) {
				using(MySqlCommand cmd = new MySqlCommand("SELECT * FROM users WHERE name = ?username", conn)) {
					cmd.Parameters.AddWithValue("?username", userName);
					cmd.Connection.Open();

					MySqlDataReader userReader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleResult);

					while(userReader.Read()) { //TODO should the while be used if only one row?
						this.name = userReader["name"].ToString();
						this.TotalLogins = int.Parse(userReader["totalLogins"].ToString()); //todo: should try parse be used for safety?
						this.Age = short.Parse(userReader["age"].ToString());
						this.Email = userReader["email"].ToString();
						this.InMsg = userReader["inMsg"].ToString();
						this.OutMsg = userReader["outMsg"].ToString();
					}

					cmd.Connection.Close();
				}
			}

			if(this.name.Equals(userName)) {
				this.TotalLogins++;
				return LoginResult.ValidLogin;
			} else { //TODO: this is open until we have a password..  for invalid login
				return LoginResult.OpenUserName;
			}
		}

		public void ClearScreen()
		{
			//just doing it the same way amnuts 2.3.0 does it

			for(int i = 0 ; i < 5; ++i) {
				this.Write("\n\n\n\n\n\n\n\n\n\n");
			}
		}

		public bool Save()
		{
			int rowsAffected = 0;
			//TODO: save new record..
			using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString)) {
				using(MySqlCommand cmd = new MySqlCommand("UPDATE users SET totalLogins = ?totalLogins, age = ?age, email = ?email, inMsg = ?inMsg, outMsg = ?outMsg WHERE name = ?username", conn)) {
					cmd.Parameters.AddWithValue("?totalLogins", this.TotalLogins);
					cmd.Parameters.AddWithValue("?age", this.Age);
					cmd.Parameters.AddWithValue("?email", this.Email);
					cmd.Parameters.AddWithValue("?username", this.Name);
					cmd.Parameters.AddWithValue("?outMsg", this.OutMsg);
					cmd.Parameters.AddWithValue("?inMsg", this.InMsg);
					cmd.Connection.Open();

					rowsAffected = cmd.ExecuteNonQuery();

					cmd.Connection.Close();
				}
			}

			if(rowsAffected == 1) {
				return true;
			} else {
				return false;
			}
		}

		public User JoinUser(User joinUser)
		{
			this.Age = joinUser.Age;
			this.Desc = joinUser.Desc;
			this.Email = joinUser.Email;
			this.Gender = joinUser.Gender;
			this.InMsg = joinUser.InMsg;
			this.Ignores = joinUser.Ignores;
			this.LastCommand = joinUser.LastCommand;
			this.LastInput = joinUser.LastInput;
			this.Logon = joinUser.Logon;
			this.Name = joinUser.Name;
			this.OutMsg = joinUser.OutMsg;

			//TODO: notify room if different?

			if (this.Room != joinUser.Room) {
				this.ChangeRoom(joinUser.Room);
			}

			this.TellBuffer = joinUser.TellBuffer;
			this.TotalLogins = joinUser.TotalLogins;

			List<IUserConnection> userConnectionList = new List<IUserConnection>();
			userConnectionList.AddRange(this.Connections);
			userConnectionList.AddRange(joinUser.Connections);

			foreach (IUserConnection currentConnection in userConnectionList) {
				currentConnection.User = this;
			}

			this.Connections = new ReadOnlyCollection<IUserConnection>(userConnectionList);

			return joinUser;
		}

		public void Quit()
		{
			//can we just quit a single connection?
			foreach (IUserConnection userConnection in this.Connections) {
				userConnection.Close();
			}

			Connections = new ReadOnlyCollection<IUserConnection>(new List<IUserConnection>());
			//close but don't destory the object since there could still be references to the user still.
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Desc {
			get;
			set;
		}

		public string Email {
			get;
			set;
		}

		public string Gender {
			get;
			set;
		}

		public int TotalLogins {
			get;
			set;
		}

		public short Age {
			get;
			set;
		}

		public string OutMsg {
			get;
			set;
		}

		public string InMsg {
			get;
			set;
		}

		public DateTime Logon {
			get;
			set;
		}

		public Room Room {
			get;
			set;
		}

		public List<UserCommuncationBuffer> TellBuffer {
			get;
			protected set;
		}

		public ReadOnlyCollection<IUserConnection> Connections {
			get {
					return connections;
			}
			set {
					connections = value;
			}
		}

		public ICommand LastCommand {
			get;
			set;
		}

		public UserInput LastInput {
			get;
			set;
		}

		public Server.ColorOptions ColorOption {
			get;
			set;
		}

		public Ignore Ignores {
			get;
			set;
		}
	}
}

