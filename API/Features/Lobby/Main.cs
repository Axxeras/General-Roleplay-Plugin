namespace GRPP.API.Features.Lobby;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Christmas.Scp2536;
using CommandSystem;
using EasyTmp;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Permissions.Extensions;
using GRPP.API.Attributes;
using GRPP.API.Core;
using GRPP.API.Core.Webhooks;
using GRPP.API.Features.CustomItems;
using GRPP.API.Features.Department;
using GRPP.API.Features.GRPPCommands;
using GRPP.API.Features.Items;
using GRPP.Extensions;
using JetBrains.Annotations;
using LabApi.Features.Permissions;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.ToolGun;
using UnityEngine;
using Broadcast = Broadcast;
using Door = Exiled.API.Features.Doors.Door;
using Logger = LabApi.Features.Console.Logger;
using Random = System.Random;
using Round = Exiled.API.Features.Round;
using Scp914 = Scp914;

public abstract class Main
{
    public static bool IsLobby;
    public static bool IsRoleplay;
    public static bool HasRoleplayStarted;
    public static SchematicObject? Schematic; // woo nullable!
    public static bool RestrictPermissions { get; set; } // false by default btw
    public static List<String> MainHosters { get; set; } = [];
    public static List<string> LoadedMaps { get; set; } = [];
    
    public static string Site = Plugin.Singleton.Config?.SiteName ?? Defaults.Site;
    
    [UsedImplicitly]
    [OnPluginEnabled]
    public static void InitEvents()
    {
        if (Plugin.Singleton == null) return;
        ServerHandlers.WaitingForPlayers += WaitingForPlayers;
        PlayerHandlers.Dying += OnLeaving;

        StaticUnityMethods.OnUpdate -= (Action)DeadmanSwitch.OnUpdate;
        // if (Plugin.Singleton.Config.MapsToLoadOnLobby == null) return;
        
        
    }

    private static void WaitingForPlayers() // reset methodd !
    {
        if (Plugin.Singleton == null) return;
        PlayerHandlers.Verified -= OnJoined;

        if (Scp2536Controller.Singleton)
            UObject.Destroy(Scp2536Controller.Singleton);
        Scp559Cake.PossibleSpawnpoints?.Clear();
        MainHosters = []; // wooo reset mainhosters list !!
        IsRoleplay = false;
        IsLobby = false;
        HasRoleplayStarted = false;
        RestrictPermissions = false;
        Shiv.IsEnabled = Plugin.Singleton.Config.ShivsNormalRounds;
        Mining.IsEnabled = Plugin.Singleton.Config.ShivsNormalRounds;
        RoleplayTime.StopClock();
    }
    private static void OnLeaving(DyingEventArgs ev)
    {
        if (ev.DamageHandler.Type == DamageType.Unknown)
            ev.IsAllowed = false;
    }

    public static void OnJoined(VerifiedEventArgs ev) => Timing.CallDelayed(2f, () => Action(ev.Player));

    public static void Action(ExPlayer player)
    {
        if (!IsLobby) return;

        player.ReferenceHub.nicknameSync._playerInfoToShow = (PlayerInfoArea)5;
        player.Role.Set(RoleTypeId.Tutorial, SpawnReason.None, RoleSpawnFlags.All);
        if (Plugin.Singleton != null && Plugin.Singleton.Config.LobbySchematic != "unset")
            Timing.CallDelayed(Timing.WaitForOneFrame,
                () => player.Position = new Vector3(Plugin.Singleton.Config.LobbySpawnLocationX,
                    Plugin.Singleton.Config.LobbySpawnLocationY, Plugin.Singleton.Config.LobbySpawnLocationZ));
        Timing.CallDelayed(0.2f,
            () => player.ShowHint(
                "<b>Welcome to the lobby!</b>\n<b>Pick a role in the Server-Specific tab in your Settings!</b>", 10f));
    }
}

[UsedImplicitly]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class UseLobbyCommand : ICommand
{
    public string Command => "UseLobby";
    public string[] Aliases => ["LobbyUse", "OpenLobby", "LobbyOpen", "lobopen", "openlob", "lobon", "l1"];
    public string Description => "Open the Lobby so no people can spawn, with teleporting everyone";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (Plugin.Singleton == null)
        {
            response = "Error. Plugin.Singleton is null. Please contact z5tm.";
            return false;
        }
        if (!sender.CheckPermission("grpp.lobby"))
        {
            response =
                "<color=orange>This command has</color> <color=red>failed</color><color=orange>.</color>\n<color=orange>You are missing the <color=blue>grpp.lobby</color><color=orange> permission.</color>";
            return false;
        }

        if (Main.IsLobby)
        {
            response =
                "<color=blue>Lobby</color> <color=orange>is already</color> <color=green>enabled</color><color=orange>!\nUse</color> <color=blue>l0</color> <color=orange>or </color><color=blue>endlobby</color><color=orange> to disable lobby!</color>";
            return false;
        }

        if (Main.Site.IsEmpty())
            Main.Site = new Random().Next(10, 99).ToString();
        var exUser = ExPlayer.Get(sender);
        if (exUser == null)
        {
            response = EasyArgs.Build().Red("Error:")
                .Space().Orange("user is null.").Done();
            return false;
        }
        // response = "<color=red>No Permission.</color>";
        // if (!sender.CheckPermission("scombat.lobby")) // socmbat.lobby > grpp.lobby has bene done btw
        // return false;

        // OHHH I see what it's doing now! it overrides the `response` var each time, then if an if check fails there it just sends the latest response. smart.
        var mapsToCheck = Plugin.Singleton.Config.MapsToLoadOnLobby;
        if (mapsToCheck != null && mapsToCheck.Any()) // if mapsToLoad is not null and the list isn't completely empty
        {
            var keys = MapUtils.LoadedMaps.Keys.ToHashSet(); // convert loaded maps (keys of them, the strings of the mapnames) to a hashset (for optimization) 
            var mapToload
                = mapsToCheck
                    .Where // where the condition is true
                    (map // n = the variable name we're usin, a string in this case
                        => // foreach map in the list,
                        !keys.Contains(map)) // if the map is IN the list of loaded maps, do not include it. If this had no "!", this would be only including ones where it is contained.
                    .ToList(); // then put it in a list for ezpz parsing !

            foreach (var map in mapToload.OfType<string>()) // for each map in maps to load (also a implicit null check  in oftype string lol
                try
                {
                    MapUtils.LoadMap(map); // if the map is not null, load it.
                    Main.LoadedMaps.Add(map); // add it to the list of loaded maps!
                }
                catch (Exception e)
                {
                    Logger.Debug($"Failure when attempting to load maps. \"{e.Message}\" -- Attempted map: {map}");
                }
        }

        exUser.Broadcast(5, "REMEMBER TO USE \"BEGINROLEPLAY\"", Broadcast.BroadcastFlags.AdminChat);

        if (Main.IsRoleplay)
        {
            foreach (var player in ExPlayer.List)
                if (player.Role.IsDead)
                    Main.Action(player);
        }
        else
        {
            var lobbySchematic = Plugin.Singleton.Config.LobbySchematic;
            if (lobbySchematic != null)
                Main.Schematic = ObjectSpawner.SpawnSchematic(lobbySchematic, Vector3.zero, Vector3.zero); // todo config
        }

        Round.Start();
        Round.IsLocked = true;

        Main.IsLobby = true;
        Main.IsRoleplay = true;

        Shiv.IsEnabled = false;
        Mining.IsEnabled = false;

        Height.IsEnabled = true;
        Name.IsEnabled = true;
        Scp914.IsEnabled = true;
        Info.IsEnabled = true;

        TeslaGate12.IsEnabled = false;
        SpawnWaves.IsEnabled = false;

        _ = AsyncWebhookHandler.AsyncWebhookTasks("lobby"/*sender*/);

        Door.LockAll(999999, DoorLockType.AdminCommand); // can also use NoPower // i see why the last devs did this. the fuckass ra panel can't - okay it's cuz of somethin else weird idfk. works now with admincommand.
        foreach (var player in ExPlayer.List) Main.Action(player);

        PlayerHandlers.Verified += Main.OnJoined;
        response = "<color=blue>Lobby</color> <color=orange>is now</color> <color=green>on</color>";
        return true;
    }
}

[UsedImplicitly]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class StopLobbyCommand : ICommand
{
    public string Command => "StopLobby";
    public string[] Aliases => ["CloseLobby", "EndLobby", "LobbyClose", "LobbyEnd", "loboff", "lobbyoff", "l0"];
    public string Description => "Closes the Lobby so no one can spawn";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckPermission("grpp.lobby"))
        {
            response =
                "<color=orange>You need the permission:</color> <color=blue>grpp.lobby</color><color=orange> to run this command.</color>";
            return false;
        }

        if (!Main.IsLobby)
        {
            response = "<color=blue>Lobby</color> <color=orange>is already</color> <color=red>off.</color>";
            return false;
        }

        if (Main.Schematic)
        {
            var a = Main.Schematic.GetComponent<MapEditorObject>();
            ToolGunHandler.DeleteObject(a);
        }

        Main.IsLobby = false;

        Name.IsEnabled = false;
        Height.IsEnabled = false;
        Info.IsEnabled = false;

        response = "<color=blue>Lobby</color> <color=orange>is now</color> <color=red>off.</color>";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class BeginRoleplay : ICommand
{
    public string Command => "StartRoleplay";
    public string[] Aliases => ["BeginRoleplay", "RoleplayStart", "startrp", "rp1", "rpstart"];
    public string Description => $"Usage: \n{Usage}";

    private string Usage =>
        "`rp1 <color=blue>[number]</color><color=orange>(Optional: Site number)</color> <color=blue>[1/yes 0/no]</color><color=orange>(Restrict Permissions of others)</color> `";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckPermission("grpp.lobby"))
        {
            response =
                "<color=orange>Sorry, but you do not have the</color> <color=blue>grpp.lobby</color> <color=orange>permission.</color>";
            return false;
        }

        if (Main.HasRoleplayStarted)
        {
            response =
                "<color=orange>Sorry, there is an ongoing</color> <color=blue>roleplay.</color>\n<color=orange>Note: There is no reason to run this command twice.</color>";
            return false;
        }
        if (arguments.Count >= 1 && uint.TryParse(arguments.At(0), out _)) // note: if we wanted to have no sitenums like 008 (the modification i just made keep it 008, old one made it 8) - just swap out _ to out var site
            Main.Site = arguments.At(0);

        if (arguments.Count >= 2 && (arguments.At(1) == "1" || arguments.At(1) == "yes") &&
            sender.CheckPermission("grpp.restrictpermissions"))
            if (Plugin.Singleton.Config.RestrictiveMode)
                Main.RestrictPermissions = true;
        
        TaserMod.TaserCardiac = true; // permits caridac !
        Main.HasRoleplayStarted = true;
        GiveInventories(); // DEPENDS ON HASROLEPLAYSTARTED
        sender.AddPermissions("grpp.bypassrestrict"); // per-round!
        Main.MainHosters.Add(ExPlayer.Get(sender).UserId);
        // if (Plugin.Singleton.Config.BypassID64 != null)
        //     Plugin.Singleton.Config.BypassID64.Add(Command);
        // else
        //     Logger.Info("There are currently no SteamIDs set up for use to bypass restrictive mode.");
        // if (Plugin.Singleton.Config.BypassRoles == null)
        // {
        //     
        // }
        
        _ = AsyncWebhookHandler.AsyncWebhookTasks("roleplay"/*sender*/);
        
        if (arguments.Count <= 0)
        {
            response = Main.RestrictPermissions
                ? $"<color=green>Roleplay has begun.</color> <color=orange>The site name is</color> <color=blue>{Main.Site}</color><color=orange>, and restrictive mode is</color> <color=green>enabled</color><color=orange>. Not sure how, but..</color>"
                : $"<color=green>Roleplay has begun.</color> <color=orange>The site name is</color> <color=blue>{Main.Site}</color><color=orange>, and restrictive mode is</color> <color=red>disabled</color>.\n<color=orange>The server has set the allowance of RestrictiveMode to:</color> <color=blue>{Plugin.Singleton.Config.RestrictiveMode}</color><color=orange>.</color>\n<color=orange>Note: To set RestrictiveMode, use</color> <color=blue>`rp1 sitenumber yes/no`</color> <color=orange>or</color> <color=blue>`rp1 sitenumber 1/0`</color>";
            // if (Plugin.Singleton?.Config?.RPWebHook !=null && Plugin.Singleton.Config.WebhookPlayerCountEnabled && Plugin.Singleton.Config.WebhookTpsEnabled) _ = new WebhookHandler().UseWebhook(Plugin.Singleton?.Config?.WebhookRPStartName, Plugin.Singleton?.Config?.RPWebHook, "", "", $"Players: {Player.ConnectionsCount}/{Server.MaxPlayers}\nTPS:{Server.Tps}", "A roleplay has been started!", "7419530", true, true);
            return true;
        }

        // Note: value = `conditon` ? valueiftrue : valueiffalse

        if (arguments.Count >= 1)
        {
            response = Main.RestrictPermissions
                ? $"<color=green>Roleplay has begun.</color> <color=orange>The site name is</color> <color=blue>{Main.Site}</color><color=orange>, and restrictive mode is</color> <color=green>enabled</color><color=orange>.</color>"
                : $"<color=green>Roleplay has begun.</color> <color=orange>The site name is</color> <color=blue>{Main.Site}</color><color=orange>, and restrictive mode is</color> <color=red>disabled</color>.\n<color=orange>Note that restrictive mode is, by default, disabled. Also, the server has set the allowance of RestrictiveMode to: <color=blue>{Plugin.Singleton.Config.RestrictiveMode}</color><color=orange>.</color>";
            // if (Plugin.Singleton?.Config?.RPWebHook !=null && Plugin.Singleton.Config.WebhookPlayerCountEnabled && Plugin.Singleton.Config.WebhookTpsEnabled) _ = new WebhookHandler().UseWebhook(Plugin.Singleton?.Config?.WebhookRPStartName, Plugin.Singleton?.Config?.RPWebHook, "", "", $"Players: {Player.ConnectionsCount}/{Server.MaxPlayers}\nTPS:{Server.Tps}", "A roleplay has been started!", "7419530", true, true);
            return true;
        }
        response = "<color=red>Error</color><color=orange>.</color>";
        return false;
    }

    

    private static void GiveInventories()
    {
        if (!Main.HasRoleplayStarted)
            return;
        try
        {
            foreach (var player in ExPlayer.List)
            {
                var scom = player.ScomPlayer();
                // if (scom?.CurrentRole?.RoleEntry == null)
                //     continue;
                // if (scom.CurrentRole.Rank == null)
                //     continue;
                // player.ClearInventory(); // ClearAmmo is redundant here, ClearInventory already clears ammo & items. ClearItems clears only items, ClearAmmo clears only ammo, but this clears everything // wait… why do we clear inventories anyway? anyways I'll just make this a toggle later :D
                if (scom.CurrentRole.Rank.LoadOut == null)
                {//what inthe world player.GetNearCameras exists LMAOO
                    Log.Warn($"User \"{player.Nickname}\" has no custom role. Aborting.");
                    continue; // note: return would have killed the entire loop. continue just skips this one !
                }
                foreach (var item in scom.CurrentRole.Rank.LoadOut)
                {
                    var itemGiver = GetItem(item, out var cost, player);

                    Features.Department.Department
                            .DepartmentsData[Features.Department.Department.GetDepartmentByRole(scom.CurrentRole.RoleEntry)]
                            .Balance -=
                        cost;
                    itemGiver?.GiveItem(player);
                }

                Features.Department.Department.UpdateDepartmentData(
                    Features.Department.Department.GetDepartmentByRole(scom.CurrentRole.RoleEntry));
            }
        }
        catch (NullReferenceException e)
        {
            Log.Debug($"There was an exception while setting someone's inventory. This is expected behavior, when a custom role is not set. Exception: {e}");
        }
    }

    private static readonly Dictionary<string, float> ItemCosts = new()
    {
        // Custom Items
        { "Taser", 400 },
        { "Baton", 25 },
        { "Smoke", 100 },
        { "Teargas", 100 },

        // -- Base Items -- \\

        // Utilities
        { "Radio", 100 },
        { "Flashlight", 40 },
        { "Lantern", 100 },

        // Medical Items
        { "Adrenaline", 50 },
        { "Medkit", 30 },
        { "Painkillers", 15 },

        // Armor -- Security Only
        { "ArmorHeavy", 845 },
        { "ArmorCombat", 560 },
        { "ArmorLight", 110 },

        // Ammunition -- GL + Security Only
        { "Ammo9x19", 5 },
        { "Ammo44cal", 60 },
        { "Ammo556x45", 30 },
        { "Ammo12gauge", 90 },

        // Weapons -- GL + Security Only
        { "GunCOM15", 400 },
        { "GunCOM18", 800 },
        { "GunRevolver", 800 },
        { "GunFSP9", 1250 },
        { "GunCrossvec", 1650 },
        { "GunE11SR", 1800 },
        { "GunShotgun", 3482 }
    };

    public static ItemGiver? GetItem(LoadOutItem item, out float cost, ExPlayer? player = null)
    {
        if (!ItemCosts.TryGetValue(item.ItemType ?? string.Empty, out cost))
            cost = 0;

        if (!Enum.TryParse(item.ItemType, out ItemType itemType))
            if (item.ItemType != null)
                return item.ItemType.ToLower() switch
                {
                    "taser" => CustomItemsManager.Get<TaserHandler>(),
                    "baton" => CustomItemsManager.Get<BatonHandler>(),
                    "smoke" => CustomItemsManager.Get<SmokeGrenadeHandler>(),
                    "teargas" => CustomItemsManager.Get<TearGasHandler>(),
                    _ => null
                };
                                                                    // if anything breaks here I'm coming back to this.

        // before this btw, var rolename was scp -- noting cuz this push is gonna be too big to look back precisely 
        
        // if (player != null) // commented out if statement, statement is always true according to Rider
        // {
            var scom = player?.ScomPlayer();
            var roleName = scom?.CurrentRole.RoleName;
        // }

        if (item.Level != null)
            if (roleName != null)
                return item.ItemType != null && item.ItemType.ToLower().StartsWith("keycard")
                    ? KeycardHandler.CreateInstance(itemType, roleName, item.Level.Value,
                        item.Permissions?.ConvertAll(x =>
                                (KeycardHandler.Levels)Enum.Parse(typeof(KeycardHandler.Levels), x))
                            .ToArray() ?? Array.Empty<KeycardHandler.Levels>())
                    : itemType;
        return null; // I must test this in game cuz what if. like. yeah.
    }
}

[UsedImplicitly]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class EndRoleplay : ICommand
{
    public string Command => "EndRoleplay";
    public string[] Aliases => ["EndRoleplay", "RoleplayEnd", "endrp", "rp0", "rpend"];
    public string Description => $"Command to end the roleplay.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckPermission("grpp.lobby"))
        {
            response = "<color=orange>Sorry, but you do not have the</color> <color=blue>grpp.lobby</color> <color=orange>permission.</color>";
            return false;
        }
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

        if (!Main.HasRoleplayStarted)
        {
            response = "<color=orange>Sorry, there is</color> <color=red>not</color> <color=orange>an ongoing</color> <color=blue>roleplay.</color>";
            return false;
        }
        
        Main.HasRoleplayStarted = false;
        Main.IsRoleplay = false;
        Main.IsLobby = false;
        Main.RestrictPermissions = false;

        TeslaGate12.IsEnabled = true;
        Shiv.IsEnabled = true;
        Mining.IsEnabled = true;
        SpawnWaves.IsEnabled = true;
        Height.IsEnabled = true;
        Name.IsEnabled = true;
        Info.IsEnabled = true;
        
        _ = AsyncWebhookHandler.AsyncWebhookTasks("end");

        if (arguments.Count == 0)
            LabApi.Features.Console.Logger.Debug("No arguments passed to rp0.");
        else
        {
            if (arguments.At(0) == "1" || 
                arguments.At(0) == "enable" || 
                arguments.At(0) == "on" ||
                arguments.At(0) == "ff") 
                Server.FriendlyFire = true;
            if (arguments.At(1) == "1" || 
                arguments.At(1) == "end" || 
                arguments.At(1) == "on" ||
                arguments.At(1) == "unlock")
                Round.IsLocked = false;
        }
        if (Plugin.Singleton?.Config.LobbyShouldUnloadMaps ?? false) // if lobby should unload on maps is enabled (with a default to false if null)
        {
            foreach (var map in Main.LoadedMaps) // for each map in this list,
                MapUtils.UnloadMap(map); // unload the map.
            Main.LoadedMaps.Clear();
        }
        

        response = "<color=blue>Roleplay</color> <color=orange>has</color> <color=red>ended</color><color=orange>.</color>";
        return true;
    }
}

[UsedImplicitly]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SetSite : ICommand
{
    public string Command => "SetSite";
    public string[] Aliases => ["SiteSet", "sitenum", "site", $"sitenumber"];
    public string Description => "Sets site number. This affects cards, and more in the future.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
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

        if (arguments.Count == 0)
        {
            response =
                $"<color=orange>>The</color> <color=blue>site's number</color> <color=orange>is currently:</color><color=blue>{Main.Site}</color><color=orange>.</color>";
            return true;
        }

        Main.Site = arguments.At(0);
        response =
            $"<color=orange>>The</color> <color=blue>site's number</color> <color=orange>has been set to</color> <color=blue>{arguments.At(0)}</color>";
        return true;
    }
}