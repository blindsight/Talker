using System;

namespace Talker.Commands
{
	public class Look : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			//This assumes that the user has already changed rooms when the look is performed
			string output = String.Format("\nRoom: {0}\n\n{1}\n\n", CurrentInput.User.Room.Name, CurrentInput.User.Room.Desc);

			if(String.IsNullOrEmpty(CurrentInput.User.Room.Topic)) {
				output += "No topic has been set yet.";
			} else {
				output += String.Format("Current topic: {0}", CurrentInput.User.Room.Topic);
			}

			CurrentInput.User.WriteLine(output);
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

	public class RoomList : ICommand
	{
		public void Run(UserInput currentInput)
		{
			string output = "+----------------------------------------------------------------------------+\n";
			output += "| name                                                     - topic           |\n";
			output += "+----------------------------------------------------------------------------+\n";

			Server.RoomList.ForEach(delegate(Room currentRoom) {
				output += string.Format("  {0,-17} - {1}\n", currentRoom.Name, currentRoom.Topic);
			});

			output += "+----------------------------------------------------------------------------+\n";
			output += String.Format("|      There is a total of {0} rooms.  All are public    |\n", Server.RoomList.Count);
			output += "+----------------------------------------------------------------------------+\n";

			currentInput.User.WriteLine(output);
		}

		public string Name {
			get {
				return "rooms";
			}
		}
	}
}

