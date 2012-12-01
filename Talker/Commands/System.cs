using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Talker.Commands
{
	public class Version : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			string copyright = "";
			object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

			if (attributes.Length > 0)
				copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

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
			CurrentInput.User.WriteLine(String.Format("| {0, -33} {1, 40} |", Assembly.GetExecutingAssembly().GetName().Version, copyright));
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

	public class Colors : ICommand
	{
		public void Run(UserInput currentInput)
		{
			string output = "\n";

			foreach(KeyValuePair<string, string> colorCode in Server.ColorCodes) {
				//TODO: an escape code to show the color code
				output += String.Format("{1} VIDEO TEST ~RS\n" ,colorCode.Key, colorCode.Value);
			}

			currentInput.User.WriteLine(output);
		}

		public string Name {
			get {
				return "colour";
			}
		}
	}

	public class DisplayFiles : ICommand
	{
		public void Run(UserInput currentInput)
		{

			string output = "+----- Files ----------------------------------------------------------------+\n\n";
			output += "Use these files to find out more about the talker.\n\n";

			TalkerFile.GetFiles("files").ForEach(delegate(TalkerFile currentFile) {
				output += string.Format("* {0, -10} - {1}", currentFile.FileName, currentFile.Description);
			});

			output += "\n\n+----------------------------------------------------------------------------+";

			currentInput.User.WriteLine(output);
		}

		public string Name {
			get {
				return "files";
			}
		}
	}
}

