using System;
using System.Reflection;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Talker
{
	public class Server
	{
		public Server()
		{
		}

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
				}
			}
		}

		public static List<User> ClientList
		{
			get;
			protected set;
		}

		public static List<ICommand> CommandList
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

		public static DateTime TalkerBooted {
			get;
			protected set;
		}
	}
}

