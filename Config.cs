// TODO: Refactor this to sort by priority. Do this alongside the documentation updates.

namespace GRPP;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

public class Config : IConfig
{
    // [Description("Set this to true after configuration has been looked through.")]
    // public bool ConfigurationComplete { get; set; } = false; // obsoleted, as like... well. there's barely reason to make the user look into this config

    [Description("true = Plugin Enabled, false = Plugin Disabled.")]
    public bool IsEnabled { get; set; } = true;

    [Description("Which maps to load upon using lobby! Can be empty or only have one.")]
    public List<string>? MapsToLoadOnLobby { get; set; } = [];

    [Description("Whether to unload the maps on lobby usage! Use this if you want to have different maps for normal rounds and for roleplays! :D")]
    public bool? LobbyShouldUnloadMaps = false;

    [Description("Should debug logs be enabled? (This is required for webhook implementation, as it enables TLS 2 + 3.)")]
    public bool Debug { get; set; } = false;
    [Description("Where players spawn when lobby is enabled. `Xf`, `0f`/`2921f` format. Request data in-game through the RA to grab this.")]
    public float LobbySpawnLocationX { get; set; } = 0f;
    public float LobbySpawnLocationY { get; set; } = 0f;
    public float LobbySpawnLocationZ { get; set; } = 0f;

    [Description("The name of the lobby schematic. Use `unset` to just load into tower.")]
    public string? LobbySchematic { get; set; } = "unset";

    [Description("Spawn location when picking a role other than Class-D. Can be `x,y,z` i.e. `0,0,0`")] 
    // remind me to add the option to change spawn positions per-role and for classd-specific
    public float PlayerSpawnLocationX { get; set; } = 0f;
    public float PlayerSpawnLocationY { get; set; } = 0f;
    public float PlayerSpawnLocationZ { get; set; } = 0f;
    [Description("Maximum length for names, when changed using `.name`.")] public int? NameMaxLength = 75;
    [Description("Maximum length for customis, when changed using `.info`.")] public int? InfoMaxLength = 150;

    [Description("List of Departments, i.e. Security, Research")]
    public List<string> Departments { get; set; } = [
    "SetMeUp1",
    "SetMeUpNow"
    ]; // need to make this shit more user-friendly. I shall soon.

    [Description("Amount of time in seconds that the broadcast sent by the .rp command lasts.")]
    public ushort RPBroadcastDuration { get; set; } = 15; 

    [Description("Scom Word blocklist")]
    public List<string> Blocklist { get; set; } = ["horridword", "badwordbadd", "badword67"];

    [Description("Whether the WeightSystem should be on or off.")]
    public bool WeightSystem { get; set; } = false;

    [Description("The maximum height that is permitted by the `.height` client command.")] public float? MaxHeight { get; set; } = 1.1f;
    [Description("The minimum height that is permitted by the `.height` client command.")] public float? MinHeight { get; set; } = 0.9f;

    [Description("Experiemntal features toggle. (Unimplemented)")]
    public bool Experimental { get; set; } = false;

    // [Description("Discord Webhook link for Department Logs")]
    // public string URL { get; set; } = "";

    // [Description("Leave empty to allow all IPs on the webserver.")]
    // public List<string> AllowedIPs { get; set; } = [];
    [Description("Site number/name. Shows up on keycards as `Site-number`/`Site-Name`! Can be overriden by hoster, by using `rp1 sitenumber`. Can be negative!")] public string SiteName { get; set; } = "22";
    // Remind me to make this modular, where it detects a number and if there's more than a number it removes the Site, but if there's just numbers it appends Site-
    [Description("Should the main hoster be allowed to restrict permissions of other hosters? (BETA, currently being implemented!)")] public bool RestrictiveMode { get; set; } = false;
    [Description("This is optional. This is for a discord webhook URL, and it sends RP information into the channel, such as start and end time.")] public string? RPWebHook { get; set; } = null;

    [Description("Whether TPS is shown on the RP webhook.")]
    public bool? WebhookTpsEnabled { get; set; } = true;
    [Description("Whether player count is shown on the RP webhook.")]
    public bool? WebhookPlayerCountEnabled { get; set; } = true;
    [Description("The color to use for the lobby (RP, not game) webhook. Only applicable in this format.")]
    public UInt32? WebhookLobbyColor { get; set; } = 1752220;
    [Description("The color to use for the RP started (RP) webhook. Only applicable in this format.")]
    public UInt32? WebhookRPColor { get; set; } = 7419530;
    [Description("The color to use for the RP ended (RP) webhook. Only applicable in this format.")]
    public UInt32? WebhookRPEndColor { get; set; } = 7419530;
    [Description("The name of the webhook that'll be posting the lobby message.")] public string? WebhookRPLobbyName { get; set; } = "LobbyBot";
    [Description("The name of the webhook that'll be posting the roleplay has begun message.")] public string? WebhookRPStartName { get; set; } = "RoleplayBot";
    [Description("The name of the webhook that'll be posting the roleplay has begun message.")] public string? WebhookRPEndName { get; set; } = "EndRoleplayBot";
    [Description("The webhook message that is displayed when lobby is enabled and there is a linked webhook.")] public string? WebhookRPLobbyMsg { get; set; } = "Lobby has been enabled.";
    [Description("The webhook message that is displayed when the begin roleplay command is run and there is a linked webhook.")] public string? WebhookRPStartMsg { get; set; } = "The roleplay has begun!";
    [Description("The webhook message that is displayed when the begin roleplay command is run and there is a linked webhook.")] public string? WebhookRPEndMsg { get; set; } = "The roleplay has ended.";
    [Description("Whether to use inline form in the discord webhook, when there is a linked webhook.")] public bool? WebhookRPInLine { get; set; } = true;
    [Description("Whether to use timestamps in the discord webhook, when there is a linked webhook.")] public bool? WebhookRPTimeStamps { get; set; } = true;
    [Description("The extra title argument permitted in Discord webhooks.")] public string? WebhookRPExtraArgTitle { get; set; } = "";
    [Description("The extra description argument permitted in Discord webhooks.")] public string? WebhookRPExtraArgDesc { get; set; } = "";
    [Description("Allow anyone with RA to modify aesthetics of their rank.")]
    public bool RankModEnabled { get; set; } = false;
    [Description("Allow anyone with RA to modify the color of their rank.")]
    public bool RankColorEnabled { get; set; } = false;
    [Description("Allow anyone with RA to modify the words before the name of their rank.")]
    public bool RankNameEnabled { get; set; } = false;
    [Description("Enable shivs by default in normal rounds. Note, they get disabled on lobby usage.")] public bool ShivsNormalRounds {get; set;} = true;
    
    [Description("Name of Com-15, Com-18, Com-45 OGG file that plays when the gun is fired. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string ComSfx { get; set; } = "com.ogg";
    [Description("Name of Revolver OGG file that plays when the gun is fired. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string RevolverSfx { get; set; } = "revolver.ogg";
    [Description("Name of FSP-9 OGG file that plays when the gun is fired. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string Fsp9Sfx { get; set; } = "fsp9.ogg";
    [Description("Name of Crossvec OGG file that plays when the gun is fired. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string CrossVecSfx { get; set; } = "crossvec.ogg";
    [Description("Name of Shotgun OGG file that plays when the gun is fired. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string ShotgunSfx { get; set; } = "shotgun.ogg";
    
    [Description("Name of E11 OGG file that plays when the gun is fired. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string E11Sfx { get; set; } = "e11.ogg";
    [Description("Name of FRMG OGG file that plays when the gun is fired. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string FrmgSfx { get; set; } = "frmg.ogg";
    [Description("Name of Logicer OGG file. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string LogicerSfx { get; set; } = "logicer.ogg";
    [Description("Name of A7 OGG file. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string A7Sfx { get; set; } = "a7.ogg";
    [Description("Name of AK OGG file. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string AkSfx { get; set; } = "ak.ogg";
    [Description("Name of ParticleDisruptor OGG file. Please include the file extension, or since only .ogg 48khz is allowed, `.ogg` will do.")] public string ParticleSfx { get; set; } = "particle.ogg";

    // [Description("List of SteamID64s that are permitted to bypass restrictive mode.")] public List<String>? BypassID64 { get; set; } = [];
    // [Description("List of roles that are permitted to bypass restrictive modes. This is not a required field, and should only be used if permissions are not being set. If you need assistance, join our discord.")] public List<String>? BypassRoles { get; set; } = [];
    [Description("Whether players should be permitted to use the audio player, using `/au`. This is HIGHLY unrecommended.")] public bool ClientAu { get; set; } = false;
    [Description("Whether to utilize async while creating directories.")] public bool? CreateDirectoryAsync { get; set; }= true;

    [Description("Permits non-RA users to use `.print` to create janitoral keycards with information on them. Even if enabled in config, this must be enabled through RA with `printon`.")] public bool AllowClientCreateCommand { get; set; } = false;
    [Description("Maximum amount of characters allowed in the Print client command.")] public int? MaximumCreateDescription { get; set; } = 50;

    [Description("Client-sided print command webhook logging URL.")] public string PrintCommandWebhookUrl { get; set; } = string.Empty;
    [Description("Client-sided roleplay command webhook logging URL.")] public string RPCommandWebhookUrl { get; set; } = string.Empty;
    [Description("Client-sided modmail command webhook logging URL.")] public string ModmailCommandWebhookUrl { get; set; } = string.Empty;
}