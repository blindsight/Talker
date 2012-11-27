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
				
				whoLines += String.Format("| {0,-59} | {1,-6} |\n", CurrentUser.Name + CurrentUser.Desc, Math.Round(minsOnline.TotalMinutes));
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

	public class UserStatus : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			User CurrentUser = CurrentInput.User;

			if (CurrentInput.Args.Length > 1) {
				User NewUser = Server.FindClientByName(CurrentInput.Args[1]);

				if(NewUser != null) {
					CurrentUser = NewUser;
				}
			}
			TimeSpan OnlineFor = DateTime.UtcNow - CurrentUser.Logon;

			string output = "+----- User Info ---------------------------------------------+\n";
			string NameDesc = String.Format("{0} - {1} ",CurrentUser.Name, CurrentUser.Desc);
			output += String.Format("Name   : {0,-30}                Level : \n", NameDesc);
			output += String.Format("Gender : {0,-15} Age : {1, -15}  Online for : {2,-15} mins\n",CurrentUser.Gender, CurrentUser.Age, Math.Round(OnlineFor.TotalMinutes));
			output += String.Format("Email Address : {0}\n", CurrentUser.Email);
			output += String.Format("Total Logins  : {0} \n", CurrentUser.TotalLogins);
			output += "+----------------------------------------------------------------------------+\n";
			CurrentInput.User.WriteLine(output);
			/*
+----- User Info -- (currently logged on) -----------------------------------+
Name   : Test - remove this character                  Level : GOD
Gender : Male          Age : Unknown              Online for : 900 mins
Email Address : Currently unset
Homepage URL  : Currently unset
ICQ Number    : Currently unset
Total Logins  : 8          Total login : 0 days, 16 hours, 8 minutes
+----- General Info ---------------------------------------------------------+
Enter Msg     : Test enters
Exit Msg      : Test goes to the...
Invited to    : <nowhere>      Muzzled : NO             Ignoring : NO           
In Area       : reception      At home : YES            New Mail : NO           
Killed 0 people, and died 0 times.  Energy : 10, Bullets : 6
+----- User Only Info -------------------------------------------------------+
Char echo     : NO             Wrap    : NO             Monitor  : NO           
Colours       : YES            Pager   : 23             Logon rm : NO           
Quick call to : <no one>       Autofwd : NO             Verified : NO           
On from site  : localhost                                   Port : 64638
+----- Wiz Only Info --------------------------------------------------------+
Unarrest Lev  : GOD            Arr lev : Unarrested     Muz Lev  : Unmuzzled    
Logon room    : reception                               Shackled : NO
Last site     : localhost
User Expires  : YES            On date : Sat 2013-01-05 18:49:59
+----------------------------------------------------------------------------+*/
		}

		public string Name {
			get {
				return "ustat";
			}
		}
	}
}

