using CommandSystem;
using Exiled.API.Features;
using GRPP;
using GRPP.API.Core.Webhooks;
using System;
using System.Linq;

[CommandHandler(typeof(ClientCommandHandler))]
public class Modmail : ICommand
{
    public string Command { get; } = "modmail";
    public string[] Aliases { get; } = new string[] { "mm" };
    public string Description { get; } = "Sends a message to all moderators currently in the server.";
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var player = Player.Get(sender);
        if (arguments.Count == 0)
        {
            response = "Usage: .mm [message]";
            return false;
        }
        string message = string.Join(" ", arguments.ToArray());
        foreach (Player target in Player.List)
        {
            if (target.RemoteAdminAccess)
            {
                target.SendConsoleMessage($"[ModMail] {player.DisplayNickname} ({player.UserId}): {message}", "yellow");
                target.Broadcast(10, $"New modmail from {player.DisplayNickname} ({player.UserId}). Check the client console!");
            }
        }
        if (!Plugin.Singleton.Config.ModmailCommandWebhookUrl.IsEmpty())
            _ = AsyncWebhookHandler.LogMessage(
                webhookNameToUse: "ModmailLogger",
                webhookUrl: Plugin.Singleton.Config.ModmailCommandWebhookUrl,
                title: "New ModMail Message",
                description: $"A user has sent a modmail message.\nName: \"{player.DisplayNickname}\"\nSteamID64: \"{player.UserId}\"\nMessage: \"{message}\"",
                color: "088808");
        response = "Your message has been sent to the moderators!";
        return true;
    }
}
