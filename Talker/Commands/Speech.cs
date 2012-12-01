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
		{//TOOD: exclaims and other keywords?
			string output = String.Format("! {0} shouts: " + CurrentInput.Message + "\n", CurrentInput.User.Name);
			string userOutput = String.Format("! You shout: " + CurrentInput.Message + "\n");


			List<User> WriteAllButUsers = new List<User>();

			WriteAllButUsers.Add(CurrentInput.User);

			Server.ClientList.ForEach(delegate(User currentUser) {
				if(currentUser.Ignores.HasFlag(User.Ignore.Shout)
				   || currentUser.Ignores.HasFlag(User.Ignore.All)) {
					//The user has on an ignore, so don't write them
					WriteAllButUsers.Add(currentUser);
				}

			});


			Server.WriteAllBut(output, WriteAllButUsers );
			CurrentInput.User.WriteLine(userOutput);
			Server.ShoutConversation.Add(new UserCommuncationBuffer(DateTime.UtcNow, output, CurrentInput.User));
		}

		public string Name {
			get {
				return "shout";
			}
		}
	}

	public class RevShout : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			string output = "";
			//TODO: could there be one review function that calls all the others?
			foreach (UserCommuncationBuffer CurrentBuffer in Server.ShoutConversation) {
				output += String.Format("[{0}] {1}\n", CurrentBuffer.Send.ToShortTimeString(), CurrentBuffer.Message);
			}
			
			if (Server.ShoutConversation.Count <= 0) {
				output += "\nRevshout buffer is empty.\n\n";
			}
			
			CurrentInput.User.WriteLine("*** Shout review buffer ***\n");
			
			output += "*** End ***";
			CurrentInput.User.WriteLine(output);
		}
		
		public string Name {
			get { //TODO: how to support other languages?
				return "revshout";
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
				if(!userObjTo.Ignores.HasFlag(User.Ignore.Tell) && !userObjTo.Ignores.HasFlag(User.Ignore.All)) {
					string toMessage = String.Format("{0} tells you: {1}",CurrentInput.User.Name, messageTo);
					string fromMessage = String.Format("you tell {0}: {1}", userTo, messageTo);

					CurrentInput.User.TellBuffer.Add(new UserCommuncationBuffer(DateTime.UtcNow, fromMessage, userObjTo));
					CurrentInput.User.WriteLine(fromMessage);

					userObjTo.TellBuffer.Add(new UserCommuncationBuffer(DateTime.UtcNow, toMessage, CurrentInput.User));
					userObjTo.WriteLine(toMessage);
				} else {
					//TODO: with ranks wizs can over ride tell ignores??
					CurrentInput.User.WriteLine(String.Format("{0} is ignoring tells at the moment.", userTo));
				}
			}
		}

		public string Name
		{
			get {
				return "tell";
			}
		}
	}

	public class PrivateEmote : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			//TODO: usage for ICommand....
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine(".pemote <user> <text>");
				return;
			}
			
			string userTo = CurrentInput.Message.Substring(0, CurrentInput.Message.IndexOf(' '));
			string messageTo = CurrentInput.Message.Substring(CurrentInput.Message.IndexOf(' '));
			
			User userObjTo = Server.FindClientByName(userTo);	
			
			if(userObjTo == null) {
				CurrentInput.User.WriteLine("No such named \"" + userTo + "\"user.");
			} else {
				if(!userObjTo.Ignores.HasFlag(User.Ignore.Tell)) {
					string toMessage = String.Format("> {0} {1}",CurrentInput.User.Name, messageTo);
					string fromMessage = String.Format("> ({0}) {1}", userTo, messageTo);
					
					CurrentInput.User.TellBuffer.Add(new UserCommuncationBuffer(DateTime.UtcNow, fromMessage, userObjTo));
					CurrentInput.User.WriteLine(fromMessage);
					
					userObjTo.TellBuffer.Add(new UserCommuncationBuffer(DateTime.UtcNow, toMessage, CurrentInput.User));
					userObjTo.WriteLine(toMessage);
				} else {
					//TODO: with ranks wizs can over ride tell ignores??
					CurrentInput.User.WriteLine("{0} is ignoring tells at the moment.");
				}
			}
		}
		
		public string Name
		{
			get {
				return "pemote";
			}
		}
	}

	public class Mutter : ICommand
	{
		public void Run(UserInput CurrentInput)
		{
			//TODO: usage for ICommand....
			if(CurrentInput.Args.Length < 2) {
				CurrentInput.User.WriteLine(".mutter <user> <message>");
				return;
			}
			
			string userAbout = CurrentInput.Message.Substring(0, CurrentInput.Message.IndexOf(' '));
			string messageAbout = CurrentInput.Message.Substring(CurrentInput.Message.IndexOf(' '));
			
			User userObjAbout = Server.FindClientByName(userAbout);	
			
			if(userObjAbout == null) {
				CurrentInput.User.WriteLine("No such named \"" + userAbout + "\"user.");
			} else {
				string toMessage = String.Format("(NOT {0}) {1} mutters: {2}\n",userObjAbout.Name, CurrentInput.User.Name, messageAbout);
				string fromMessage = String.Format("(NOT {0}) you mutter: {1}\n", userAbout, messageAbout);

				CurrentInput.User.WriteLine(fromMessage);

				CurrentInput.User.Room.WriteAllBut(toMessage, new List<User> { userObjAbout, CurrentInput.User } );
			}
		}
		
		public string Name
		{
			get {
				return "mutter";
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

	public class PicturePreview : ICommand
	{
		public void Run(UserInput currentInput)
		{
			if(currentInput.Args.Length < 2) {
				string output = "+----------------------------------------------------------------------------+\n";
				output += "| Pictures available to view                                                 |\n";
				output += "+----------------------------------------------------------------------------+\n";

				string line = "";
				short lineLength = 0;

				List<TalkerFile> talkerFiles = TalkerFile.GetFiles("pictures");
				//TODO: should I just get the names first?!??
				talkerFiles.ForEach(delegate(TalkerFile currentFile) {
					line += String.Format("{0, -12}", currentFile.FileName.PadRight(10));

					if(lineLength == 5) {
						output += String.Format("| {0, -74} |\n", line);
						line = "";
						lineLength = 0;
					} else {
						lineLength++;
					}
				});


				if(!String.IsNullOrEmpty(line))  { //check for final line
					output += String.Format("| {0, -74} |\n", line);
				}
				output += "+----------------------------------------------------------------------------+\n";
				output += String.Format("| There are {0} pictures                                                      |\n", talkerFiles.Count);
				output += "+----------------------------------------------------------------------------+\n";

				currentInput.User.WriteLine(output);
			} else {
				//You preview the following picture...
				string pictureName = currentInput.Args[1];
				
				string picture = TalkerFile.GetFile(pictureName);
				
				if(!String.IsNullOrEmpty(picture)) {
					currentInput.User.WriteLine(String.Format("You preview the following picture..\n\n {0}", picture));
				} else {
					currentInput.User.WriteLine("Sorry, there is no picture with that name.");
				}
				

			}
		}
	
		public string Name {
			get {
				return "preview";
			}
		}
	}
	
	public class Picture : ICommand
	{
		/*You show the following picture to the room...
			Test shows the following picture...*/
		public void Run(UserInput currentInput)
		{
			if(currentInput.Args.Length < 2) {
				currentInput.User.WriteLine(".picture <picture>");
				return;
			}

			string pictureName = currentInput.Args[1];

			string picture = TalkerFile.GetFile(pictureName);
						
			if(!String.IsNullOrEmpty(picture)) {
				List<User> WriteAllButUsers = new List<User>();
				
				WriteAllButUsers.Add(currentInput.User);
				
				Server.ClientList.ForEach(delegate(User currentUser) {
					if(currentUser.Ignores.HasFlag(User.Ignore.Pics)
					   || currentUser.Ignores.HasFlag(User.Ignore.All)) {
						//The user has on an ignore, so don't write them
						WriteAllButUsers.Add(currentUser);
					}
					
				});

				currentInput.User.Room.WriteAllBut(String.Format("{0} shows the following picture..\n\n {1}\n\n",currentInput.User.Name, picture), WriteAllButUsers);
				currentInput.User.WriteLine(String.Format("You show the following picture..\n\n {0}\n",picture));
			} else {
				currentInput.User.WriteLine("Sorry, there is no picture with that name.");
			}
		}

		public string Name {
			get {
				return "picture";
			}
		}
	}
	
	public class PictureTell : ICommand
	{
		public void Run(UserInput currentInput)
		{
			//TODO: should I Cache the pictures or something for speed?
			if(currentInput.Args.Length < 3) {
				currentInput.User.WriteLine(".ptell <user> <picture>");
				return;
			}
			
			string userTo = currentInput.Message.Substring(0, currentInput.Message.IndexOf(' '));
			string pictureName = currentInput.Args[2];

			User userObjTo = Server.FindClientByName(userTo);	
			
			if(userObjTo == null) {
				currentInput.User.WriteLine("No such named \"" + userTo + "\"user.");
			} else {
				if(!userObjTo.Ignores.HasFlag(User.Ignore.Pics) && userObjTo.Ignores.HasFlag(User.Ignore.All)) {
					string picture = TalkerFile.GetFile(pictureName);

					string fromMessage = "";

					if(!String.IsNullOrEmpty(picture)) {
						userObjTo.WriteLine(String.Format("{0} shows you the following picture..\n\n {1}\n",currentInput.User.Name, picture));
						fromMessage = String.Format("You show the following picture to {0}\n\n {1}\n", userTo, picture);
					} else {
						fromMessage = "Sorry, there is no picture with that name.\n\n";
					}

					currentInput.User.WriteLine(fromMessage);
				} else {
					currentInput.User.WriteLine(String.Format("{0} is ignoring pictures at the moment", userTo));
				}
			}
		}

		public string Name {
			get {
				return "ptell";
			}
		}	
	}
}

