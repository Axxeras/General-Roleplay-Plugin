// General Roleplay Plugin
// Copyright (C) 2025 Site-12, VisLuke, SticksDeveloper
// Copyright (C) 2026 z5tm
//
// This file is part of General Roleplay Plugin.
//
// General Roleplay Plugin is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// General Roleplay Plugin is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with Foobar. If not, see <https://www.gnu.org/licenses/>.
//

namespace GRPP;

using System;
using System.Linq;
using System.Net;
using System.Reflection;
using API.Attributes;
using API.Core;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using ProjectMER.Events.Arguments;
using Log = Exiled.API.Features.Log;

public sealed class Plugin : Plugin<Config>
{
    public override string Name => "grpp"; // General Roleplay Plugin
    public override string Author => "z5tm & Site-12 Development Team"; // Thank you Stick and VisLuke [i have now been informed that there were many more. thanks y'all :fire:]
    public override Version Version => new(1, 4, 4);
    public static Plugin Singleton => _singleton ?? throw new InvalidOperationException("Plugin is not initialized!");
    private Harmony? _harmony;
    public const string HarmonyInstanceName = "com.grpp.main";
    
    private bool _wasEverEnabled;
    private static Plugin? _singleton;
    public InternalConfiguration? GlobalConfig { get; private set; }
    
    public override void OnEnabled()
    {
        base.OnEnabled();
        _singleton = this;
        
        GlobalConfig = new InternalConfiguration();
        if (_wasEverEnabled || _harmony != null)
            return;
        
        _wasEverEnabled = true;
        _harmony = new Harmony(HarmonyInstanceName);
        _harmony.PatchAll();
        Log.Info($"GRPP enabled.");
        ProjectMER.Events.Handlers.Schematic.SchematicSpawned += SpawningSchematic;
        
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        Log.Debug("Debug mode is enabled. Not enabling SSS as it's currently broken, and enabling TLS 2 + 3 for webhook sending.");
        ListResourceNames();
        InvokeOnEnabledAttributes();
    }

    public override void OnDisabled()
    {
        _harmony?.UnpatchAll();
        _harmony = null;
        _singleton = null;
        base.OnDisabled();
        GlobalConfig = null;
    }

    private static void SpawningSchematic(SchematicSpawnedEventArgs ev)
    {
        foreach (var gameObject in ev.Schematic.AttachedBlocks)
        {
            var objPos = gameObject.transform.position;
            var objRot = gameObject.transform.rotation;

            var resultObj = gameObject.name switch
            {
                "HCZDoor" => PrefabHelper.Spawn(PrefabType.HCZBreakableDoor, objPos, objRot),
                "EZDoor" => PrefabHelper.Spawn(PrefabType.EZBreakableDoor, objPos, objRot),
                "LCZDoor" => PrefabHelper.Spawn(PrefabType.LCZBreakableDoor, objPos, objRot),
                "BreachDoor" => PrefabHelper.Spawn(PrefabType.HCZBulkDoor, objPos, objRot),
                "Clutter_1" => PrefabHelper.Spawn(PrefabType.HCZOpenHallway_Clutter_A, objPos, objRot),
                "Clutter_2" => PrefabHelper.Spawn(PrefabType.HCZOpenHallway_Clutter_B, objPos, objRot),
                "Clutter_3" => PrefabHelper.Spawn(PrefabType.HCZOpenHallway_Clutter_C, objPos, objRot),
                "Clutter_4" => PrefabHelper.Spawn(PrefabType.HCZOpenHallway_Clutter_D, objPos, objRot),
                "Clutter_5" => PrefabHelper.Spawn(PrefabType.HCZOpenHallway_Clutter_E, objPos, objRot),
                "Clutter_6" => PrefabHelper.Spawn(PrefabType.HCZOpenHallway_Clutter_F, objPos, objRot),
                "Clutter_7" => PrefabHelper.Spawn(PrefabType.HCZOpenHallway_Clutter_G, objPos, objRot),
                "Clutter_8" => PrefabHelper.Spawn(PrefabType.HCZOpenHallway_Construct_A, objPos, objRot),
                _ => null
            };

            if (!resultObj) continue;

            resultObj.transform.localScale = gameObject.transform.localScale;
            resultObj.transform.parent = ev.Schematic.transform;
        }
    }

    /// <summary>
    /// Lists all resource names within this assembly to the server console.
    /// </summary>
    private static void ListResourceNames()
    {
        foreach (var resourceName in typeof(Plugin).Assembly.GetManifestResourceNames())
            Log.Info($"RESOURCE: '{resourceName}'");
    }

    /// <summary>
    /// Invokes all static void methods within the assembly which have the <see cref="OnPluginEnabledAttribute"/> attribute.
    /// </summary>
    private void InvokeOnEnabledAttributes()
    {
        foreach (var method in AccessTools.GetTypesFromAssembly(Assembly).SelectMany(AccessTools.GetDeclaredMethods))
        {
            if (method.GetCustomAttribute<OnPluginEnabledAttribute>() is null)
                continue;

            if (!method.IsStatic)
            {
                Log.Warn($"Failed to invoke method {method.DeclaringType}::{method.Name} on plugin enabled, as it is not static.");
                continue;
            }

            if (method.GetParameters().Length != 0)
            {
                Log.Warn($"Failed to invoke method {method.DeclaringType}::{method.Name} on plugin enabled, as it contains parameters.");
                continue;
            }

            try
            {
                method.Invoke(null, null);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}