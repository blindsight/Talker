using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Data.SqlTypes;

namespace Talker
{
	public class User
	{
		private TcpClient client;
		private string name;

		public enum LoginResult {
			ValidLogin,
			InvalidLogin,
			OpenUserName
		}

		public User(TcpClient clientConnection, int clientIndex)
		{
			client = clientConnection;
			name = "User " + clientIndex;
			this.Desc = " is a newbie needing a description. ";
			this.TellBuffer = new List<UserCommuncationBuffer>();
			this.TotalLogins = 0;
		}

		public void Write(string clientText)
		{
			NetworkStream clientStream = client.GetStream();

			if(clientStream.CanWrite) {
				byte[] writeText = Encoding.UTF8.GetBytes(clientText);

				clientStream.Write(writeText, 0, writeText.Length);
			}
		}

		public void WriteLine(string clientText)
		{
			this.Write(clientText + "\n");
		}

		public NetworkStream Stream
		{
			get { return client.GetStream(); }
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

		public bool Save()
		{
			//TODO: save new record..
			using (MySqlConnection conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainDb"].ConnectionString)) {
				using(MySqlCommand cmd = new MySqlCommand("UPDATE users SET totalLogins = ?totalLogins, age = ?age, email = ?email WHERE name = ?username", conn)) {
					cmd.Parameters.AddWithValue("?totalLogins", this.TotalLogins);
					cmd.Parameters.AddWithValue("?age", this.Age);
					cmd.Parameters.AddWithValue("?email", this.Email);
					cmd.Parameters.AddWithValue("?username", this.Name);
					cmd.Connection.Open();

					int rowsAffected = cmd.ExecuteNonQuery();

					cmd.Connection.Close();

					if(rowsAffected == 1) {
						return true;
					} else {
						return false;
					}
				}
			}
		}

		public void Quit()
		{
			//close but don't destory the object since there could still be references to the user still.
			client.Close();
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
	}
}

