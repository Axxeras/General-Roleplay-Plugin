using CommandSystem;
using Exiled.API.Features;
using GRPP;
using GRPP.API.Core.Webhooks;
using System;
using System.Linq;
using EasyTmp;
using UnityEngine;
using Exiled.API.Enums;

[CommandHandler(typeof(ClientCommandHandler))]
public class RPConsole : ICommand
{
	public string Command => "rp";
    public string[] Aliases => ["rpconsole"];
	public string Description => "Allows you to send a roleplay broadcast to all players in the room with you.";
    private string Usage = EasyArgs.Build().CmdArguments(".rp message").Done();
    
    public static void RoomBroadcast(ExPlayer? broadcastSender, ushort duration, string message)
    {
        if (broadcastSender == null)
            return;
        
        var currentRoom = broadcastSender.CurrentRoom;
        var position = broadcastSender.Position;
        
        if (currentRoom == null)
            return;
        
        foreach (var target in ExPlayer.List.Where(target => target != null))
        {
            if (target.CurrentRoom == null)
            {
                Log.Debug($"Skipping player {target.DisplayNickname} [{target.UserId}] because their current room is null.");
                continue;
            }
            
            if (target.CurrentRoom.Zone is ZoneType.Surface or ZoneType.Unspecified &&
                Vector3.Distance(position, target.Position) <= Plugin.Singleton.Config.RPCommandBroadcastRange || target.CurrentRoom == currentRoom)
                target.Broadcast(Plugin.Singleton.Config.RPBroadcastDuration, $"{broadcastSender.CustomName} says: " + message);
        }
        
        Log.Debug("Roleplay message sent by " + broadcastSender.DisplayNickname + " in " + currentRoom.Name + " with message: " + message);
    }
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var player = ExPlayer.Get(sender);
        
        if (!player.IsAlive)
        {
            response = "You cannot send a roleplay message while dead!";
            return false;
        }

        if (arguments.Count == 0)
        {
            response = Usage;
            return false;
        }
        
        var message = string.Join(" ", arguments);
        RoomBroadcast(player, Plugin.Singleton.Config.RPCommandBroadcastDuration, message);
        
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
 