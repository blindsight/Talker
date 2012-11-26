using System;
using System.Collections.Generic;

namespace Talker
{
	public class Server
	{
		public Server()
		{
		}

		static Server()
		{
			ClientList = new List<User>();
		}

		public static List<User> ClientList
		{
			get;
			protected set;
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
	}
}

