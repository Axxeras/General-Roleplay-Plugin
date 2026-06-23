namespace GRPP.API.Features.GRPPCommands;

using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using CommandSystem;
using EasyTmp;
using GRPP;
using Core.Webhooks;

public static class ModmailHandler
{
    [OnPluginEnabled]
    public static void InitEvents() => ServerHandlers.WaitingForPlayers += WaitingForPlayers;
    
    public static int Index { get; set; }
    public static Dictionary<int, ExPlayer> ModmailInstances => [];
    private static void WaitingForPlayers() => Index = 0;
}

[CommandHandler(typeof(ClientCommandHandler))]
public class Modmail : ICommand
{
    public string Command { get; } = "modmail";
    public string[] Aliases { get; } = ["mm"];
    public string Description { get; } = "Sends a message to all moderators currently in the server.";
    private static readonly string Usage = EasyArgs.Build().CmdArguments(".mm message").Done();
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var player = ExPlayer.Get(sender);
        
        if (arguments.Count == 0)
        {
            response = Usage;
            return false;
        }
        
        ModmailHandler.Index += 1;
        var currentIndex = ModmailHandler.Index;
        ModmailHandler.ModmailInstances.Add(currentIndex, player);
        
        var message = string.Join(" ", arguments.ToArray());
        
        foreach (var target in ExPlayer.List)
        {
            if (!target.RemoteAdminAccess)
                continue;
            
            target.SendConsoleMessage($"[ModMail] {player.DisplayNickname} ({player.UserId}) (Modmail Index: {currentIndex}): {message}", "yellow");
            target.Broadcast(10, $"New modmail from {player.DisplayNickname} ({player.UserId}). Check the client console!");
        }
        if (!Plugin.Singleton.Config.ModmailCommandWebhookUrl.IsEmpty())
            _ = AsyncWebhookHandler.LogMessage(
                webhookNameToUse: "ModmailLogger",
                webhookUrl: Plugin.Singleton.Config.ModmailCommandWebhookUrl,
                title: "New ModMail Message",
                description: $"A user has sent a modmail message. Index: {currentIndex}.\nName: \"{player.DisplayNickname}\"\nSteamID64: \"{player.UserId}\"\nMessage: \"{message}\"",
                color: "088808");
        response = "Your message has been sent to the moderators!";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Reply : ICommand
{
    public string Command { get; } = "modMailReply";
    public string[] Aliases { get; } = ["mmr", "modmail"];
    public string Description { get; } = "Replies to a modmail message using the index provided in the message.";
    private static readonly string Usage = EasyArgs.Build().CmdArguments("mmr index message").Done();
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var player = ExPlayer.Get(sender);
        if (arguments.Count < 2)
        {
            response = Usage;
            return false;
        }
        if (!int.TryParse(arguments.At(0), out var index))
        {
            response = "The index must be a valid integer.";
            return false;
        }
        if (!ModmailHandler.ModmailInstances.ContainsKey(index))
        {
            response = "No modmail message found with the provided index.";
            return false;
        }
        
        var target = ModmailHandler.ModmailInstances[index];
        var message = string.Join(" ", arguments.Skip(1).ToArray());
        
        target.SendConsoleMessage($"[ModMail Reply] {player.DisplayNickname}[Staff]: {message}", "yellow");
        target.Broadcast(10, "You have received a reply to your modmail message. Check the client console!");
        
        response = $"Reply sent to {target.DisplayNickname}!";
        return true;
    }
}