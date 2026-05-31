using CommandSystem;
using Exiled.API.Features;
using GRPP;
using GRPP.API.Core.Webhooks;
using System;
using System.Linq;

[CommandHandler(typeof(ClientCommandHandler))]
public class RPConsole : ICommand
{
	public string Command { get; } = "rp";
	public string[] Aliases { get; } = new string[] { "rpconsole" };
	public string Description { get; } = "Allows you to send a roleplay broadcast to all players in the room with you.";

    public void RoomBroadcast(Player player, ushort duration, string message)
    {
        Room currentroom = player.CurrentRoom;

        foreach (Player target in Player.List)
        {
            if (target.CurrentRoom == currentroom)
            {
                target.Broadcast(Plugin.Singleton.Config.RPBroadcastDuration, $"{player.CustomName} says: " + message);
            }
        }
    }

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		//rp [duration] [message]

        var player = Player.Get(sender);

        if (arguments.Count == 0 || arguments.Count == 1)
		{
			response = "Usage: .rp [message]";
			return false;
		}

		if (!ushort.TryParse(arguments.At(0), out ushort duration))
		{
			response = "Invalid duration. The first argument should be the duration of the broadcast in seconds.";
			return false;
		}

		string message = string.Join(" ", arguments.Skip(1).ToArray());
		RoomBroadcast(Player.Get(sender), duration, message);

        if (!Plugin.Singleton.Config.RPCommandWebhookUrl.IsEmpty())
            _ = AsyncWebhookHandler.LogMessage(
                webhookNameToUse: "RPLogger",
                webhookUrl: Plugin.Singleton.Config.RPCommandWebhookUrl,
                title: "Roleplay Message",
                description: $"A user has sent a roleplay message.\nName: \"{player.DisplayNickname}\"\nSteamID64: \"{player.UserId}\"\nMessage: \"{message}\"",
                color: "880808");


        response = "Roleplay message successfully sent!";
		return true;
    }
}
