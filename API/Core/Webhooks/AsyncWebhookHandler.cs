namespace GRPP.API.Core.Webhooks;

using System;
using System.Threading.Tasks;
using Exiled.API.Features;
using Extensions;
using Features;
using Player = LabApi.Features.Wrappers.Player;
using Server = LabApi.Features.Wrappers.Server;

public static class AsyncWebhookHandler
{
    public static async Task
        AsyncWebhookTasks(
            string startType /*ICommandSender sender*/) // we can pass args here too but not necessary! // okay we can pass sender here but not necessary :D
    {
        if (Plugin.Singleton?.Config?.RPWebHook == null || !Plugin.Singleton.Config.RPWebHook.IsItAWebhook())
        {
            Log.Warn("No webhook has been set up, or the webhook has been set up improperly, so we are not sending a webhook.");
            return;
        }

        string whatToDescription =
            (Plugin.Singleton.Config.WebhookPlayerCountEnabled, Plugin.Singleton.Config.WebhookTpsEnabled) switch
            {
                (true, true) => $"Players: {Player.ConnectionsCount}/{Server.MaxPlayers}\nTPS:{Server.Tps}",
                (true, false) => $"Players: {Player.ConnectionsCount}/{Server.MaxPlayers}",
                (false, true) => $"TPS:{Server.Tps}",
                _ => ""
            };
        var startMsg = (startType) switch
        {
            "lobby" => Plugin.Singleton.Config.WebhookRPLobbyMsg,
            "roleplay" => Plugin.Singleton.Config.WebhookRPStartMsg,
            "end" => Plugin.Singleton.Config.WebhookRPEndMsg,
            _ => "Unset"
        };
        var color = (startType) switch
        {
            "lobby" => Plugin.Singleton.Config.WebhookLobbyColor.ToString(),
            "roleplay" => Plugin.Singleton.Config.WebhookRPColor.ToString(),
            "end" => Plugin.Singleton.Config.WebhookRPEndColor.ToString(),
            _ => "6769420"
        };

        var startName = (startType) switch
        {
            "lobby" => Plugin.Singleton.Config.WebhookRPLobbyName,
            "roleplay" => Plugin.Singleton.Config.WebhookRPStartName,
            "end" => Plugin.Singleton.Config.WebhookRPEndName,
            _ => "Unset"
        };

        try
        {
            await new WebhookHandler().UseWebhook(
                startName ?? Defaults.WebhookRPStartName,
                Plugin.Singleton.Config.RPWebHook,
                Plugin.Singleton.Config.WebhookRPExtraArgTitle 
                ?? Defaults.WebhookRPExtraArgTitle, // this is for like a secondary section - we can't nullify this, tested. try if you'd like tho
                
                Plugin.Singleton.Config.WebhookRPExtraArgDesc 
                ?? Defaults.WebhookRPExtraArgDesc, // this is for like a secondary section - we can't nullify this, tested. try if you'd like tho
                
                whatToDescription, 
                startMsg ?? Defaults.WebhookRPStartMsg,
                color,
                Plugin.Singleton.Config?.WebhookRPInLine ?? Defaults.WebhookRPInLine,
                Plugin.Singleton.Config?.WebhookRPTimeStamps ?? Defaults.WebhookRPTimeStamps
            ).ConfigureAwait(
                false); // configuredtaskawaitable, also the configureawait false is essentially just a minor performance gain because we're running on, well, a plugin -- anyways, 
        }
        catch (Exception e)
        {
            Log.Error(
                $"There has been an error whilst attempting to handle the webhooks. Error: {Environment.NewLine}\"{e}\"");
        }
    }

    public static async Task<Boolean> LogMessage(string webhookNameToUse, string webhookUrl, string title, string description, string color)
    {
        if (webhookUrl == null)
            throw new ArgumentNullException(nameof(webhookUrl));
        
        if (!webhookUrl.IsItAWebhook())
            return false;
        
        try
        {
            await new WebhookHandler().UseWebhook(webhookNameToUse, webhookUrl, string.Empty, string.Empty, description, title, color, true, true, true).ConfigureAwait(false); // configuredtaskawaitable, also the configureawait false is essentially just a minor performance gain because we're running on, well, a plugin -- anyways, 
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"There has been an error whilst attempting to handle the webhooks. Error: {Environment.NewLine}\"{e}\"");
            return false;
        }
    }
}