using System;

namespace Talker.Commands
{
	public class Version : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			CurrentInput.User.WriteLine("|                           Your Talker's Name Here                          |");
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			CurrentInput.User.WriteLine(String.Format("| Total number of users    : {0}                             |", Server.ClientList.Count));
			/*| Logons this current boot :    1 new users,    1 old users                  |
				| Total number of users    : 3     Maximum online users     : 50             |
					| Total number of rooms    : 15    Swear ban currently on   : OFF            |
					| Smail auto-forwarding on : YES   Auto purge on            : NO             |
					| Maximum smail copies     : 6     Names can be recapped    : YES            |
					| Personal rooms active    : YES   Maximum user idle time   : 60  mins       |*/
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");
			CurrentInput.User.WriteLine("| 0.0.1                                        (C) Timothy Rhodes 2012       |");
			CurrentInput.User.WriteLine("+----------------------------------------------------------------------------+");

		}

		public string Name {
			get {
				return "version";
			}
		}
	}

	public class Time : ICommand
	{
		public void Run(UserInput CurrentInput)
		{

			DateTime currentTime = DateTime.UtcNow;
			TimeSpan upTime = currentTime - Server.TalkerBooted;

			string output="+----------------------------------------------------------------------------+\n";
			output +="| Talker times                                                               |\n";
			output +="+----------------------------------------------------------------------------+\n";
			output += String.Format("| The current system time is : {0,-45} |\n",currentTime);
			output += String.Format("| System booted              : {0,-45} |\n", Server.TalkerBooted);
			output += String.Format("| Uptime                     : {0,6} days, {1,2} hours, {2,2} minutes, {3,2} seconds |\n", upTime.Days, upTime.Hours, upTime.Minutes, upTime.Seconds);
			output +="+----------------------------------------------------------------------------+\n";

			CurrentInput.User.WriteLine(output);
		}

		public string Name {
			get {
				return "time";
			}
		}
	}
}

