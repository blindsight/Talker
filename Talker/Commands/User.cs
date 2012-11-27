using System;

namespace Talker.Commands
{
	public class Quit : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			CurrentInput.User.WriteLine("Thank you for coming and Goodbye!");
			Server.ClientList.Remove(CurrentInput.User);
			CurrentInput.User.Quit();
		}

		public string Name {
			get {
				return "quit";
			}
		}
	}

	public class ChangeName : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine(".name <new name>");
				return;
			}
			
			CurrentInput.User.Name = CurrentInput.Args[1];
			CurrentInput.User.WriteLine("Your name has been changed to \"" + CurrentInput.User.Name + "\"");
		}
		
		public string Name {
			get {
				return "name";
			}
		}
	}

	public class Desc : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine("Your current description is: " + CurrentInput.User.Desc);
				return;
			}
			
			CurrentInput.User.Desc = CurrentInput.Message;
			CurrentInput.User.WriteLine("Your description has been changed to \"" + CurrentInput.User.Desc + "\"");
		}
		
		public string Name {
			get {
				return "desc";
			}
		}
	}

	public class Who : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------+");
			CurrentInput.User.WriteLine("|  Name      Description                                      |  Mins  |");
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------+");
			
			string whoLines = "";
			
			DateTime commonTime = DateTime.UtcNow;
			
			foreach(User CurrentUser in Server.ClientList) {
				TimeSpan minsOnline = commonTime - CurrentUser.Logon;
				
				whoLines += String.Format("| {0,-59} | {1,-6} |\n", CurrentUser.Name + CurrentUser.Desc, minsOnline.Minutes);
			}
			
			CurrentInput.User.Write(whoLines);
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------+");
		}
		
		public string Name {
			get {
				return "who";
			}
		}
	}

	public class Help : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			string output = "";

			for(int i = 0; i < Server.CommandList.Count; i++) {
				output += String.Format(" {0} ", Server.CommandList[i].Name.PadRight(10));

				if (i % 4 == 0 && i != 0) {
					//TODO: might want to use string builder here
					output += "\n";
				}
			}

			CurrentInput.User.WriteLine(output);
			CurrentInput.User.WriteLine(String.Format("         {0} Total Commands        ", Server.CommandList.Count));
		}

		public string Name {
			get {
				return "help";
			}
		}
	}
}

