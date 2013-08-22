using System;

namespace Talker
{
	public enum UserConnectionTypes
	{
		Tcp,
		WebSocket
	}

	public interface IUserConnection
	{
		void Write(String clientText);
		void Close();
		UserConnectionTypes Type { get; }
		User User { get; set; } //TODO: move to constructor..
	}
}

