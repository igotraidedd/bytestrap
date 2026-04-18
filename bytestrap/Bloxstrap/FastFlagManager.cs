using Bloxstrap.Enums.FlagPresets;
using System.Security.Policy;
using System.Windows;

namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string ClassName => nameof(FastFlagManager);

        public override string LOG_IDENT_CLASS => ClassName;

        public override string ProfilesLocation => Path.Combine(Paths.Base, "Profiles");

        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings\\ClientAppSettings.json");

        public bool Changed => !OriginalProp.SequenceEqual(Prop);

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {

            // Presets and stuff
            { "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },
            { "Rendering.MSAA", "FIntDebugForceMSAASamples" },
            { "Rendering.FRMQualityOverride", "DFIntDebugFRMQualityLevelOverride" },

            // Rendering engines
            { "Rendering.Mode.DisableD3D11", "FFlagDebugGraphicsDisableDirect3D11" },
            { "Rendering.Mode.D3D11", "FFlagDebugGraphicsPreferD3D11" },
            { "Rendering.Mode.Vulkan", "FFlagDebugGraphicsPreferVulkan" },
            { "Rendering.Mode.OpenGL", "FFlagDebugGraphicsPreferOpenGL" },

            // Geometry
            { "Geometry.MeshLOD.Static", "DFIntCSGLevelOfDetailSwitchingDistanceStatic" }, // this isnt actually a flag, we use it to determine current value, not the best way of doing that :sob:
            { "Geometry.MeshLOD.L0", "DFIntCSGLevelOfDetailSwitchingDistance" },
            { "Geometry.MeshLOD.L12", "DFIntCSGLevelOfDetailSwitchingDistanceL12" },
            { "Geometry.MeshLOD.L23", "DFIntCSGLevelOfDetailSwitchingDistanceL23" },
            { "Geometry.MeshLOD.L34", "DFIntCSGLevelOfDetailSwitchingDistanceL34" },

            // Texture quality
            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },

            // === BOOTSTRAP PERFORMANCE FLAGS ===

            // FPS Unlock & Framerate
            { "Performance.UnlockFPS", "DFIntTaskSchedulerTargetFps" },
            { "Performance.RenderThrottlePercentage", "DFIntDebugRenderThrottlePercentage" },

            // Lighting / Rendering performance
            { "Performance.DisableShadows", "DFFlagDebugPauseVoxelizer" },
            { "Performance.LightingTechnology", "DFFlagDebugRenderForceTechnologyVoxel" },
            { "Performance.DisablePostFX", "FFlagDisablePostFx" },
            { "Performance.DisablePlayerShadows", "FIntRenderShadowIntensity" },
            { "Performance.ShadowMapSize", "FIntRenderShadowmapBias" },
            { "Performance.LowerQualityShadows", "DFIntCullFactorPixelThresholdShadowMapHighQuality" },
            { "Performance.LowerQualityShadowsMainView", "DFIntCullFactorPixelThresholdMainViewHighQuality" },

            // Physics / Heartbeat
            { "Performance.PhysicsThrottle", "DFIntS2PhysicsSenderRate" },
            { "Performance.HeartbeatInterval", "DFIntDefaultHeartbeatIntervalMs" },

            // Network / Ping optimization
            { "Performance.NetworkMTU", "DFIntConnectionMTUSize" },
            { "Performance.NetworkSendRate", "DFIntDataSenderRate" },
            { "Performance.NetworkSendBuffer", "DFIntDataSenderMaxBandwidthKBPS" },
            { "Performance.NetworkRecvBuffer", "DFIntDataReceiverMaxBandwidthKBPS" },
            { "Performance.NetworkPrediction", "DFFlagEnableNetworkPrediction" },

            // Terrain & grass
            { "Performance.DisableGrass", "FIntFRMMinGrassDistance" },
            { "Performance.TerrainQuality", "FIntTerrainOctreeMaxDepth" },
            { "Performance.DisableTerrain", "FFlagDebugRenderingSetDeterministic" },

            // Particles & effects
            { "Performance.ParticleCap", "FIntParticleMaxCount" },
            { "Performance.ReduceParticles", "FFlagDebugForceParticleLOD" },

            // Misc
            { "Performance.DisableTelemetry", "FFlagDebugDisableTelemetryEphemeralCounter" },
            { "Performance.DisableTelemetry2", "FFlagDebugDisableTelemetryEphemeralStat" },
            { "Performance.DisableTelemetry3", "FFlagDebugDisableTelemetryEventIngest" },
            { "Performance.DisableTelemetry4", "FFlagDebugDisableTelemetryPoint" },
            { "Performance.DisableTelemetry5", "FFlagDebugDisableTelemetryV2Counter" },
            { "Performance.DisableTelemetry6", "FFlagDebugDisableTelemetryV2Event" },
            { "Performance.DisableTelemetry7", "FFlagDebugDisableTelemetryV2Stat" },
            { "Performance.PreloadAssets", "DFFlagEnablePreloadAsync" },
            { "Performance.GarbageCollectionFreq", "DFIntGCFrequency" },
        };

        public static IReadOnlyDictionary<RenderingMode, string> RenderingModes => new Dictionary<RenderingMode, string>
        {
            { RenderingMode.Default, "None" },
            { RenderingMode.Vulkan, "Vulkan" },
            { RenderingMode.OpenGL, "OpenGL" },
            { RenderingMode.D3D11, "D3D11" },
        };

        public static IReadOnlyDictionary<MSAAMode, string?> MSAAModes => new Dictionary<MSAAMode, string?>
        {
            { MSAAMode.Default, null },
            { MSAAMode.x1, "1" },
            { MSAAMode.x2, "2" },
            { MSAAMode.x4, "4" }
        };

        public static IReadOnlyDictionary<TextureQuality, string?> TextureQualityLevels => new Dictionary<TextureQuality, string?>
        {
            { TextureQuality.Default, null },
            { TextureQuality.Level0, "0" },
            { TextureQuality.Level1, "1" },
            { TextureQuality.Level2, "2" },
            { TextureQuality.Level3, "3" },
        };

        // all fflags are stored as strings
        // to delete a flag, set the value as null
        public void SetValue(string key, object? value)
        {
            const string LOG_IDENT = "FastFlagManager::SetValue";

            if (value is null)
            {
                if (Prop.ContainsKey(key))
                    App.Logger.WriteLine(LOG_IDENT, $"Deletion of '{key}' is pending");

                Prop.Remove(key);
            }
            else
            {
                if (Prop.ContainsKey(key))
                {
                    if (key == Prop[key].ToString())
                        return;

                    App.Logger.WriteLine(LOG_IDENT, $"Changing of '{key}' from '{Prop[key]}' to '{value}' is pending");
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' to '{value}' is pending");
                }

                Prop[key] = value.ToString()!;
            }
        }

        // this returns null if the fflag doesn't exist
        public string? GetValue(string key)
        {
            // check if we have an updated change for it pushed first
            if (Prop.TryGetValue(key, out object? value) && value is not null)
                return value.ToString();

            return null;
        }

        public void SetPreset(string prefix, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
                SetValue(pair.Value, value);
        }

        public void SetPresetEnum(string prefix, string target, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
            {
                if (pair.Key.StartsWith($"{prefix}.{target}"))
                    SetValue(pair.Value, value);
                else
                    SetValue(pair.Value, null);
            }
        }

        public string? GetPreset(string name)
        {
            if (!PresetFlags.ContainsKey(name))
            {
                App.Logger.WriteLine("FastFlagManager::GetPreset", $"Could not find preset {name}");
                Debug.Assert(false, $"Could not find preset {name}");
                return null;
            }

            return GetValue(PresetFlags[name]);
        }

        public T GetPresetEnum<T>(IReadOnlyDictionary<T, string> mapping, string prefix, string value) where T : Enum
        {
            foreach (var pair in mapping)
            {
                if (pair.Value == "None")
                    continue;

                if (GetPreset($"{prefix}.{pair.Value}") == value)
                    return pair.Key;
            }

            return mapping.First().Key;
        }

        public bool IsPreset(string Flag) => PresetFlags.Values.Any(v => v.ToLower() == Flag.ToLower());

        /// <summary>
        /// Writes flags directly to all Roblox version directories, bypassing the normal
        /// modification pipeline. This ensures flags persist even if Roblox overwrites
        /// the ClientSettings folder on launch.
        /// </summary>
        public void BypassWriteFlags()
        {
            const string LOG_IDENT = "FastFlagManager::BypassWriteFlags";

            App.Logger.WriteLine(LOG_IDENT, "Starting bypass flag write...");

            // Write to the normal modifications location
            Save();

            string flagJson = JsonSerializer.Serialize(
                Prop.ToDictionary(k => k.Key, v => v.Value.ToString()!),
                new JsonSerializerOptions { WriteIndented = true }
            );

            // Write directly to every Roblox version directory
            if (Directory.Exists(Paths.Versions))
            {
                foreach (string versionDir in Directory.GetDirectories(Paths.Versions))
                {
                    string clientSettingsDir = Path.Combine(versionDir, "ClientSettings");
                    string targetFile = Path.Combine(clientSettingsDir, "ClientAppSettings.json");

                    try
                    {
                        Directory.CreateDirectory(clientSettingsDir);
                        File.WriteAllText(targetFile, flagJson);
                        // Make the file read-only to resist Roblox overwriting it
                        File.SetAttributes(targetFile, File.GetAttributes(targetFile) | FileAttributes.ReadOnly);
                        App.Logger.WriteLine(LOG_IDENT, $"Bypass wrote flags to {targetFile}");
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to bypass write to {targetFile}");
                        App.Logger.WriteException(LOG_IDENT, ex);
                    }
                }
            }

            // Also write to the Roblox LocalAppData directory directly
            string robloxVersionsPath = Path.Combine(Paths.LocalAppData, "Roblox", "Versions");
            if (Directory.Exists(robloxVersionsPath))
            {
                foreach (string versionDir in Directory.GetDirectories(robloxVersionsPath))
                {
                    string clientSettingsDir = Path.Combine(versionDir, "ClientSettings");
                    string targetFile = Path.Combine(clientSettingsDir, "ClientAppSettings.json");

                    try
                    {
                        Directory.CreateDirectory(clientSettingsDir);
                        File.WriteAllText(targetFile, flagJson);
                        File.SetAttributes(targetFile, File.GetAttributes(targetFile) | FileAttributes.ReadOnly);
                        App.Logger.WriteLine(LOG_IDENT, $"Bypass wrote flags to Roblox dir: {targetFile}");
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to bypass write to Roblox dir: {targetFile}");
                        App.Logger.WriteException(LOG_IDENT, ex);
                    }
                }
            }

            App.Logger.WriteLine(LOG_IDENT, "Bypass flag write complete.");
        }

        /// <summary>
        /// Clears read-only attributes on flag files to allow normal updates again.
        /// </summary>
        public void ClearBypassLocks()
        {
            const string LOG_IDENT = "FastFlagManager::ClearBypassLocks";

            void UnlockDir(string basePath)
            {
                if (!Directory.Exists(basePath))
                    return;

                foreach (string versionDir in Directory.GetDirectories(basePath))
                {
                    string targetFile = Path.Combine(versionDir, "ClientSettings", "ClientAppSettings.json");
                    if (File.Exists(targetFile))
                    {
                        try
                        {
                            FileAttributes attrs = File.GetAttributes(targetFile);
                            if (attrs.HasFlag(FileAttributes.ReadOnly))
                            {
                                File.SetAttributes(targetFile, attrs & ~FileAttributes.ReadOnly);
                                App.Logger.WriteLine(LOG_IDENT, $"Unlocked {targetFile}");
                            }
                        }
                        catch (Exception ex)
                        {
                            App.Logger.WriteException(LOG_IDENT, ex);
                        }
                    }
                }
            }

            UnlockDir(Paths.Versions);
            UnlockDir(Path.Combine(Paths.LocalAppData, "Roblox", "Versions"));
        }

        public override void Save()
        {
            // convert all flag values to strings before saving

            foreach (var pair in Prop)
                Prop[pair.Key] = pair.Value.ToString()!;

            base.Save();

            // clone the dictionary
            OriginalProp = new(Prop);
        }

        public override void Load(bool alertFailure = true)
        {
            base.Load(alertFailure);

            // clone the dictionary
            OriginalProp = new(Prop);

            if (GetPreset("Rendering.ManualFullscreen") != "False")
                SetPreset("Rendering.ManualFullscreen", "False");
        }
    }
}
