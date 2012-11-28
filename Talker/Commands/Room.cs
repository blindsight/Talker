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

	public class Review : ICommand
	{
		public void Run(UserInput currentInput)
		{
			if(currentInput.User.Room.Review.Count > 0 ) {
				currentInput.User.WriteLine("*** Room conversation buffer ***\n");

				string output = "";

				currentInput.User.Room.Review.ForEach(delegate(UserCommuncationBuffer currentMessage) {
					//TODO: what if the line doesn't have a new line at the end?
					output += String.Format("{0} {1}", currentMessage.Send.ToShortTimeString(), currentMessage.Message);
				});

				currentInput.User.WriteLine(output);
				currentInput.User.WriteLine("*** End ***");
			} else {
				currentInput.User.WriteLine("Review buffer is empty.");
			}
		}

		public string Name {
			get {
				return "review";
			}
		}
	}

	public class ClearBuffer : ICommand
	{
		public void Run(UserInput currentInput)
		{
			currentInput.User.Room.Review.Clear();
			currentInput.User.WriteLine("You clear the review buffer.");
			currentInput.User.Room.WriteAllBut(String.Format("{0} clears the review buffer.\n", currentInput.User.Name), new System.Collections.Generic.List<User> { currentInput.User });
		}

		public string Name {
			get {
				return "cbuff";
			}
		}
	}

	public class Topic : ICommand
	{
		public void Run(UserInput currentInput)
		{
			if(currentInput.Args.Length < 2) {
				currentInput.User.WriteLine(String.Format("The current topic is: {0}", currentInput.User.Room.Topic));
			} else {
				currentInput.User.Room.Topic = currentInput.Message;

				currentInput.User.WriteLine(String.Format("Topic set to: {0}", currentInput.Message));
				currentInput.User.Room.WriteAllBut(String.Format("{0} has set the topic to: {1}\n", currentInput.User.Name, currentInput.Message), new System.Collections.Generic.List<User> { currentInput.User });
			}
		}

		public string Name {
			get {
				return "topic";
			}
		}
	}
}

