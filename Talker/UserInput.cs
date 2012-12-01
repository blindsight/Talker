using System;

namespace Talker
{
	public class UserInput
	{
		public UserInput(User CurrentUser, string CommandInput)
		{
			//remove the "." before running command
			this.CommandInput = CommandInput;

			if(CommandInput.Length > 0) {
				this.Input = CommandInput.Remove(0,1);
			}
			//string userCommandText = CommandInput.Remove(0,1);
			this.Args = this.Input.Split(' ');
			this.InputStart = this.Input.IndexOf(' ');
			this.InputStart = ( this.InputStart>= 0) ? this.InputStart : 0; //make sure the index is above 0
			this.Message = this.Input.Remove(0, this.InputStart).Trim();
			this.User = CurrentUser;
		}

		public User User
		{
			get;
			protected set;
		}

		public string CommandInput
		{
			get;
			protected set;
		}

		//full string
		public string Input
		{
			get;
			protected set;
		}
		
		//int after first space
		public int InputStart
		{
			get;
			protected set;
		}
		
		//string after first space
		public string Message
		{
			get;
			protected set;
		}
		
		//string broken up by spaces
		public string[] Args
		{
			get;
			protected set;
		}
	}
}

