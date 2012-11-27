using System;

namespace Talker
{
	public class UserCommuncationBuffer
	{
		public UserCommuncationBuffer(DateTime send, string message, User from)
		{
			this.Send = send;
			this.Message = message;
			this.From = from;
		}

		public DateTime Send {
			get;
			protected set;
		}

		public string Message {
			get;
			protected set;
		}

		public User From {
			get;
			protected set;
		}
	}
}

