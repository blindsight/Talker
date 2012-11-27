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
}

