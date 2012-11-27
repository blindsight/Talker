using System;

namespace Talker
{
	public interface ICommand
	{
		void Run(UserInput CurrentInput);

		string Name { get; }
	}
}

