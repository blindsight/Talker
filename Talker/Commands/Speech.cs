using System;
using System.Collections.Generic;

namespace Talker.Commands
{
	public class Say : ICommand
	{
		public void Run(UserInput currentInput)
		{
			string sayText = currentInput.Message;

			if(!currentInput.CommandInput.StartsWith(".")) {
				sayText = currentInput.CommandInput;
			}

			User userObj = currentInput.User;

			userObj.WriteLine("You say: " + sayText);
			string output = String.Format("{0} says: {1}\n", userObj.Name, sayText);
			
			userObj.Room.WriteAllBut(output, new List<User>{ userObj});
			userObj.Room.Review.Add(new UserCommuncationBuffer(DateTime.UtcNow, output, userObj));
		}

		public String Name {
			get {
				return "say";
			}
		}
	}

	public class SayTo : ICommand
	{
		public void Run(UserInput currentInput)
		{
			if(currentInput.Args.Length < 2) {
				currentInput.User.WriteLine(".sayto <user> <message>");
				return;
			}
		
			string userTo = currentInput.Message.Substring(0, currentInput.Message.IndexOf(' '));
			string messageTo = currentInput.Message.Substring(currentInput.Message.IndexOf(' '));
			
			User userObjTo = Server.FindClientByName(userTo);	
			
			if(userObjTo == null) {
				currentInput.User.WriteLine("No such named \"" + userTo + "\"user.");
			} else {
				User userObj = currentInput.User;
				
				userObj.WriteLine(String.Format("You (to {0}) {1} ",userObjTo.Name, messageTo));
				userObjTo.WriteLine(String.Format("{0} (to You) {1} ",userObj.Name, messageTo));
				string output = String.Format("{0} (to {1}) {2}\n", userObj.Name, userObjTo.Name, messageTo);
				
				userObj.Room.WriteAllBut(output, new List<User>{ userObj, userObjTo });
				userObj.Room.Review.Add(new UserCommuncationBuffer(DateTime.UtcNow, output, userObj));
			}
		}
		
		public String Name {
			get {
				return "sayto";
			}
		}
	}

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

	public class Think : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			string output = String.Format("{0} thinks . o O ( {1} )\n", CurrentInput.User.Name, CurrentInput.Message);
			CurrentInput.User.Room.Review.Add(new UserCommuncationBuffer(DateTime.UtcNow, output, CurrentInput.User));
			CurrentInput.User.Room.Write(output);
		}

		public string Name {
			get {
				return "think";
			}
		}
	}

	public class Sing : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			string output = String.Format("{0} sings o/~ {1} o/~\n", CurrentInput.User.Name, CurrentInput.Message);
			CurrentInput.User.Room.Review.Add(new UserCommuncationBuffer(DateTime.UtcNow, output, CurrentInput.User));
			CurrentInput.User.Room.Write(output);
		}
		
		public string Name {
			get {
				return "sing";
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
			string output = CurrentInput.User.Name + "" + CurrentInput.Message;

			CurrentInput.User.Room.Review.Add(new UserCommuncationBuffer(DateTime.UtcNow, output, CurrentInput.User));
			CurrentInput.User.Room.Write(output);
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
				string toMessage = String.Format("{0} tells you: {1}",CurrentInput.User.Name, messageTo);
				string fromMessage = String.Format("you tell {0}: {1}", userTo, messageTo);

				CurrentInput.User.TellBuffer.Add(new UserCommuncationBuffer(DateTime.UtcNow, fromMessage, userObjTo));
				CurrentInput.User.WriteLine(fromMessage);

				userObjTo.TellBuffer.Add(new UserCommuncationBuffer(DateTime.UtcNow, toMessage, CurrentInput.User));
				userObjTo.WriteLine(toMessage);
			}
		}

		public string Name
		{
			get {
				return "tell";
			}
		}
	}

	public class Echo : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine("Usage: echo <text>\n");
				return;
			}
			string output = CurrentInput.Message + "\n";
			
			CurrentInput.User.Room.Review.Add(new UserCommuncationBuffer(DateTime.UtcNow, output, CurrentInput.User));
			CurrentInput.User.Room.Write(output);
		}
		
		public string Name {
			get {
				return "echo";
			}
		}
	}

	public class RevTell : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			string output = "";

			foreach (UserCommuncationBuffer CurrentBuffer in CurrentInput.User.TellBuffer) {
				output += String.Format("[{0}] {1}\n", CurrentBuffer.Send.ToShortTimeString(), CurrentBuffer.Message);
			}

			if (CurrentInput.User.TellBuffer.Count <= 0) {
				output += "\nRevtell buffer is empty.\n\n";
			}

			CurrentInput.User.WriteLine("*** Your tell buffer ***");

			output += "*** End ***";
			CurrentInput.User.WriteLine(output);
		}

		public string Name {
			get { //TODO: how to support other languages?
				return "revtell";
			}
		}
	}

	public class ClearTell : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			CurrentInput.User.TellBuffer.Clear();
			CurrentInput.User.WriteLine("Your tells have now been cleared.");
		}

		public string Name {
			get { 
				return "ctell";
			}
		}
	}
}

