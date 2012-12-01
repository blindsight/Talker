using System;
using System.Resources;

public static class BooleanExtensions
{
	public static string ToYesNoString(this bool value)
	{
		//TODO: move to resources
		return value ? "YES" : "NO";
	}
}
