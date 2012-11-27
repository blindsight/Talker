using System;
using System.Collections.Generic;

namespace Talker.Commands
{
	public class Shout : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			string output = String.Format("! {0} shouts: " + CurrentInput.Message + "\n", CurrentInput.User.Name);
			string userOutput = String.Format("! You shout: " + CurrentInput.Message + "\n");
			Server.WriteAllBut(output, new List<User>{ CurrentInput.User } );
			CurrentInput.User.WriteLine(userOutput);
		}

		public string Name {
			get {
				return "shout";
			}
		}
	}

	public class Emote : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine("Usage: emote <text>\n");
				return;
			}
			
			Server.WriteAll(CurrentInput.User.Name + "" + CurrentInput.Message);
		}

		public string Name {
			get {
				return "emote";
			}
		}
	}

	public class Tell : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			//TODO: usage for ICommand....
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine(".tell <user> <message>");
				return;
			}

			string userTo = CurrentInput.Message.Substring(0, CurrentInput.Message.IndexOf(' '));
			string messageTo = CurrentInput.Message.Substring(CurrentInput.Message.IndexOf(' '));
			
			User userObjTo = Server.FindClientByName(userTo);	
			
			if(userObjTo == null) {
				CurrentInput.User.WriteLine("No such named \"" + userTo + "\"user.");
			} else {
				CurrentInput.User.WriteLine("you tell " + userTo + ": " + messageTo);
				userObjTo.WriteLine(CurrentInput.User.Name + " tells you:" + messageTo);
			}
		}

		public string Name
		{
			get {
				return "tell";
			}
		}
	}
}

