using CommandSystem;
using Exiled.API.Features;
using GRPP;
using GRPP.API.Core.Webhooks;
using System;
using UnityEngine;
using Exiled.API.Enums;

[CommandHandler(typeof(ClientCommandHandler))]
public class RPConsole : ICommand
{
	public string Command { get; } = "rp";
	public string[] Aliases { get; } = { "rpconsole" };
	public string Description { get; } = "Allows you to send a roleplay broadcast to all players in the room with you.";

    public void RoomBroadcast(Player player, ushort duration, string message)
    {
        Room currentroom = player.CurrentRoom;
        Vector3 position = player.Position;

        foreach (Player target in Player.List)
        {
            if (target.CurrentRoom.Zone == ZoneType.Surface || target.CurrentRoom.Zone == ZoneType.Unspecified)
            {
                if (Vector3.Distance(position, target.Position) <= Plugin.Singleton.Config.RPCommandBroadcastRange)
                {
                    target.Broadcast(Plugin.Singleton.Config.RPBroadcastDuration, $"{player.CustomName} says: " + message);
                }
            }

            else if (target.CurrentRoom == currentroom)
            {
                target.Broadcast(Plugin.Singleton.Config.RPBroadcastDuration, $"{player.CustomName} says: " + message);
            }
        }
    }

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		//rp [duration] [message]

        var player = Player.Get(sender);

        if (arguments.Count == 0)
		{
			response = "Usage: .rp [message]";
			return false;
		}

        if (!player.IsAlive)
        {
            response = "You cannot send a roleplay message while dead!";
            return false;
        }

		string message = string.Join(" ", arguments);
		RoomBroadcast(Player.Get(sender), Plugin.Singleton.Config.RPCommandBroadcastDuration, message);

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
