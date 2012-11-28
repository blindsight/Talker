using System;

namespace Talker.Commands
{
	public class Look : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			CurrentInput.User.WriteLine(String.Format("welcome to {0}", CurrentInput.User.Room.Name));
		}

		public string Name {
			get {
				return "look";
			}
		}
	}

	public class ChangeRoom : ICommand
	{
		public void Run(UserInput currentInput)
		{
			Room newRoom = Server.LoginRoom;

			if(currentInput.Args.Length > 1) {
				newRoom = Server.FindRoomByName(currentInput.Args[1]);

				if(newRoom == null) {
					newRoom = Server.LoginRoom;
				}
			}
			currentInput.User.ChangeRoom(newRoom);
		}

		public string Name {
			get {
				return "go";
			}
		}
	}
}

