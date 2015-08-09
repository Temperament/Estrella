namespace Zepheus.Zone
{
	public class CommandInfo
	{
		public string Command { get; private set; }
		public CommandHandler.Command Function { get; private set; }
		public string[] Parameters { get; private set; }
		public byte GmLevel { get; private set; }

		public CommandInfo(string command, CommandHandler.Command function, byte gmlevel, string[] param)
		{
			Command = command;
			Function = function;
			GmLevel = gmlevel;
			Parameters = param;
		}
	}
}