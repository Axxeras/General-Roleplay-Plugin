namespace GRPP.API.Features.GRPPCommands;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using CommandSystem;
using Attributes;
using EasyTmp;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Extensions;
using Lobby;

public abstract class RankMod
{
    public static bool IsEnabled;
    
    [OnPluginEnabled]
    public static void InitEvents() => ServerHandlers.WaitingForPlayers += WaitingForPlayers;

    private static void WaitingForPlayers() => IsEnabled = false;
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class RankModCommand : ICommand
{
    public string Command => "modrank";
    public string[] Aliases => ["rankmod"];
    public string Description => "Sets your rank color and a tag before it!";
    public string Usage => "rankmod color infinitearguments";
    enum ValidColors {pink, red, brown, silver, light_green, crimson, cyan, aqua, deep_pink, tomato, yellow, magenta, blue_green, orange, lime, green, emerald, carmine, nickel, mint, army_green, pumpkin, gold}
    enum ValidColorsForColorTag {blue, red, brown}
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckRemoteAdmin(out response))
            return false;
        
        if (Main.RestrictPermissions && (!sender.CheckPermission("grpp.bypassrestrict") || !Main.MainHosters.Contains(ExPlayer.Get(sender).UserId)))
        {
            response = EasyArgs.Build()
                .Blue("Restrictive permissions")
                .Space().Orange("mode is currently")
                .Space().Green("enabled").Orange(". You also do not have the ")
                .Space().Blue("\"grpp.bypassrestrict\"")
                .Space().Orange("permission, nor are you the").Space().Blue("main hoster").Space()
                .Orange("of the roleplay. \nThis command has been")
                .Space().Red("ignored").Orange(".").Done();
            return false;
        }
        if (Plugin.Singleton.Config.RankModEnabled)
        {
            RankMod.IsEnabled = true;
            Log.Debug("[GRPP-RankMod] Managed to enable RankMod.");
        }

        if (!RankMod.IsEnabled || !Plugin.Singleton.Config.RankModEnabled)
        {
            Log.Debug("[GRPP-RankMod] RankMod is disabled.");
            response = "<color=orange>This feature is currently disabled via config.\nThe command has</color> <color=red>not</color> <color=orange>been run.</color>";
            return false;
        }

        var player = ExPlayer.Get((CommandSender)sender);

        if (arguments.Count == 0)
        {
            response = $"<color=orange>This command must be run with arguments! I have zero idea how to reset rank color. List of options:</color><color=blue> (coming soon, for now ask z5)</color>";
            return false;
        }
        if (Regex.IsMatch(arguments.At(0), @"\d+"))
        {
            response = "<color=red>Fail</color><color=orange>. The first argument has numbers in it.</color>";
            return false;
        }
        bool colorOk = Enum.TryParse<ValidColors>(arguments.At(0), ignoreCase: true, out ValidColors color);
        bool colorOkToColorTag =
            Enum.TryParse<ValidColorsForColorTag>(arguments.At(0), ignoreCase: true, out ValidColorsForColorTag color1);

        if (!colorOk)
        {
            Log.Debug("[GRPP-RankMod] Cancelling. Color not found in enum.");
            response = $"<color=orange>The color was not found in the list.\nList:</color> <color=blue>(coming soon, for now ask z5)</color>";
            return false;
        }
        if (arguments.Count == 1 && colorOkToColorTag && Plugin.Singleton.Config.RankColorEnabled)
        {
            Log.Debug("[GRPP-RankMod] Got one argument. Perfect. Color was okay to tag, too.");
            player.RankColor = arguments.At(0).ToLower();
            response = $"<color=orange>Rank color has been set to</color> <color={arguments.At(0)}>{arguments.At(0)}</color><color=orange>.</color>";
            return true;
        }
        if (arguments.Count == 1 && !colorOkToColorTag && Plugin.Singleton.Config.RankColorEnabled)
        {
            Log.Debug("[GRPP-RankMod] Got one argument. Perfect. Color was not okay to tag, by the way.");
            player.RankColor = arguments.At(0).ToLower();
            response = $"<color=orange>Rank color has been set to</color> <color=blue>{arguments.At(0)}</color><color=orange>.</color>";
            return true;
        }
        if (arguments.Count >= 2 && Plugin.Singleton.Config.RankNameEnabled)
        {
            Log.Debug(($"[GRPP-RankMod] Got {arguments.Count} arguments."));
            player.RankColor = arguments.At(0).ToLower();
            
            response = "<color=orange>Your <color=blue>rank color</color> <color=orange>AND</color> <color=blue>rank name</color> <color=orange>have now been set!!!.</color>";
            player.RankName = $"{string.Join(" ", arguments.Skip(1))} ({player.Group?.BadgeText})";
            return true;
        }
        if (arguments.Count >= 2 && Plugin.Singleton.Config.Debug && Plugin.Singleton.Config.RankNameEnabled)
        {
            Log.Debug(($"[GRPP-RankMod] Got {arguments.Count} arguments."));
            player.RankColor = arguments.At(0).ToLower();
            
            response = $"<color=orange>Your <color=blue>rank color</color> <color=orange>AND</color> <color=blue>rank name</color> <color=orange>have now been set!!!.\nArguments: {arguments}";
            player.RankName = $"{string.Join(" ", arguments.Skip(1))} ({player.Group?.BadgeText})";
            return true;
        }
        // if (arguments.Count == 3)
        // {
        //     Log.Debug(("[GRPP-RankMod] Got three arguments."));
        //     player.RankColor = arguments.At(0).ToLower();
        //     response = "<color=orange>Sorry, the other features in this command have not been implemented. Soon, there will be a rankname set.</color>";
        //     player.RankName = string.Join(" ", arguments.Skip(1), "(", player.Group?.BadgeText, ")");
        //     // player.RankName = string.Join(" ", arguments.At(1), arguments.At(2), " (", player.Group?.BadgeText, ")");
        //     return true;
        // }
        // if (arguments.Count == 4)
        // {
        //     Log.Debug(("[GRPP-RankMod] Got four arguments."));
        //     player.RankColor = arguments.At(0).ToLower();
        //     response = "<color=orange>Sorry, the other features in this command have not been implemented. Soon, there will be a rankname set.</color>";
        //     // player.RankName = string.Join(arguments.At(1), arguments.At(2), arguments.At(3), " (", player.Group?.BadgeText, ")");
        //     player.RankName = string.Join(" ", arguments.Skip(1), "(", player.Group?.BadgeText, ")");
        //     return true;
        // }

        // response = null;
        // GC.Collect(); // - i really wanna do this but idk the implications

        
        // player.DisplayNickname = string.Join(" ", arguments);

        // foreach (var item in player.Items)
        //     if (CustomItemsManager.Get<KeycardHandler>().Container.HasItem(item.Base, out var idCard))
        //     {
        //         idCard.Name = player.DisplayNickname;
        //         break;
        // }

        if (!Plugin.Singleton.Config.RankColorEnabled)
        {
            response = $"<color=orange>In the config file, RankColor's allowance is set to</color> <color=blue>{Plugin.Singleton.Config.RankColorEnabled}</color><color=orange>.\nRankName's allowance is set to</color> <color=blue>{Plugin.Singleton.Config.RankNameEnabled}</color>.";
            return false;
        }

        if (!Plugin.Singleton.Config.RankNameEnabled)
        {
            response = $"<color=orange>In the config file, RankColor's allowance is set to</color> <color=blue>{Plugin.Singleton.Config.RankColorEnabled}</color><color=orange>.\nRankName's allowance is set to</color> <color=blue>{Plugin.Singleton.Config.RankNameEnabled}</color>.";
            return false;
        }
        if (Plugin.Singleton.Config.Debug)
        {
            response =
                $"<color=orange>There was an issue with the command.\nArguments:</color><color=blue>{arguments.ToString().ToLower()}</color>";
            return false;
        }

        response =
            "<color=orange>The command has</color> <color=red>failed</color> <color=orange>to run in some way. Enable debug mode for more information.</color>";

        return false;
    }
}
