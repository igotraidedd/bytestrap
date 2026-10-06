using Bloxstrap.Enums.FlagPresets;
using System.Collections.Concurrent;
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

        /// <summary>
        /// Order-independent comparison of current flags vs. last saved state.
        /// Values are compared as strings so JsonElement survivors from Load()
        /// compare equal to their string equivalents.
        /// </summary>
        public bool Changed
        {
            get
            {
                if (OriginalProp.Count != Prop.Count)
                    return true;

                foreach (var pair in Prop)
                {
                    if (!OriginalProp.TryGetValue(pair.Key, out object? original))
                        return true;

                    if (!String.Equals(original?.ToString(), pair.Value?.ToString(), StringComparison.Ordinal))
                        return true;
                }

                return false;
            }
        }

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

        // These used to allocate a brand new dictionary on every property access
        // (including per data-binding evaluation). They're immutable, so share one.
        public static IReadOnlyDictionary<RenderingMode, string> RenderingModes { get; } = new Dictionary<RenderingMode, string>
        {
            { RenderingMode.Default, "None" },
            { RenderingMode.Vulkan, "Vulkan" },
            { RenderingMode.OpenGL, "OpenGL" },
            { RenderingMode.D3D11, "D3D11" },
        };

        public static IReadOnlyDictionary<MSAAMode, string?> MSAAModes { get; } = new Dictionary<MSAAMode, string?>
        {
            { MSAAMode.Default, null },
            { MSAAMode.x1, "1" },
            { MSAAMode.x2, "2" },
            { MSAAMode.x4, "4" }
        };

        public static IReadOnlyDictionary<TextureQuality, string?> TextureQualityLevels { get; } = new Dictionary<TextureQuality, string?>
        {
            { TextureQuality.Default, null },
            { TextureQuality.Level0, "0" },
            { TextureQuality.Level1, "1" },
            { TextureQuality.Level2, "2" },
            { TextureQuality.Level3, "3" },
        };

        // Case-insensitive lookup of every preset's underlying FFlag name,
        // so IsPreset() is O(1) instead of an O(n) scan with string allocs.
        private static readonly HashSet<string> _presetFlagNames =
            new(PresetFlags.Values, StringComparer.OrdinalIgnoreCase);

        // Memoized prefix scans: SetPreset()/SetPresetEnum() are called in tight
        // loops by the settings UI, so each unique prefix is scanned exactly once.
        // (Same StartsWith semantics as before, just Ordinal + cached.)
        private static readonly ConcurrentDictionary<string, KeyValuePair<string, string>[]> _prefixCache =
            new(StringComparer.Ordinal);

        public static IReadOnlyList<KeyValuePair<string, string>> GetPairsByPrefix(string prefix) =>
            _prefixCache.GetOrAdd(prefix, p =>
                PresetFlags.Where(x => x.Key.StartsWith(p, StringComparison.Ordinal)).ToArray());

        public static IReadOnlyList<string> GetKeysByPrefix(string prefix) =>
            GetPairsByPrefix(prefix).Select(x => x.Key).ToArray();

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
                string newValue = value.ToString()!;

                if (Prop.TryGetValue(key, out object? existing))
                {
                    // skip no-op writes (previously compared the key against the
                    // old value, which could never match - always log + rewrite)
                    if (String.Equals(existing?.ToString(), newValue, StringComparison.Ordinal))
                        return;

                    App.Logger.WriteLine(LOG_IDENT, $"Changing of '{key}' from '{existing}' to '{newValue}' is pending");
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' to '{newValue}' is pending");
                }

                Prop[key] = newValue;
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
            foreach (var pair in GetPairsByPrefix(prefix))
                SetValue(pair.Value, value);
        }

        public void SetPresetEnum(string prefix, string target, object? value)
        {
            string matchPrefix = $"{prefix}.{target}";

            foreach (var pair in GetPairsByPrefix(prefix))
            {
                if (pair.Key.StartsWith(matchPrefix, StringComparison.Ordinal))
                    SetValue(pair.Value, value);
                else
                    SetValue(pair.Value, null);
            }
        }

        public string? GetPreset(string name)
        {
            if (!PresetFlags.TryGetValue(name, out string? flag))
            {
                App.Logger.WriteLine("FastFlagManager::GetPreset", $"Could not find preset {name}");
                Debug.Assert(false, $"Could not find preset {name}");
                return null;
            }

            return GetValue(flag);
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

        public bool IsPreset(string Flag) => _presetFlagNames.Contains(Flag);

        private static readonly JsonSerializerOptions _flagJsonOptions = new() { WriteIndented = true };

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
                _flagJsonOptions
            );

            // Collect every version directory first (deduped), then write in parallel -
            // each write is independent IO, so this scales with disk queue depth.
            var versionDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void CollectVersionDirs(string? basePath)
            {
                if (String.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
                    return;

                try
                {
                    foreach (string versionDir in Directory.GetDirectories(basePath))
                        versionDirs.Add(versionDir);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to enumerate version dirs in {basePath}");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }
            }

            CollectVersionDirs(Paths.Versions);
            CollectVersionDirs(Path.Combine(Paths.LocalAppData, "Roblox", "Versions"));

            Parallel.ForEach(versionDirs, versionDir =>
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
            });

            App.Logger.WriteLine(LOG_IDENT, "Bypass flag write complete.");
        }

        /// <summary>
        /// Clears read-only attributes on flag files to allow normal updates again.
        /// </summary>
        public void ClearBypassLocks()
        {
            const string LOG_IDENT = "FastFlagManager::ClearBypassLocks";

            var flagFiles = new List<string>();

            void CollectFlagFiles(string? basePath)
            {
                if (String.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
                    return;

                foreach (string versionDir in Directory.GetDirectories(basePath))
                {
                    string targetFile = Path.Combine(versionDir, "ClientSettings", "ClientAppSettings.json");
                    if (File.Exists(targetFile))
                        flagFiles.Add(targetFile);
                }
            }

            CollectFlagFiles(Paths.Versions);
            CollectFlagFiles(Path.Combine(Paths.LocalAppData, "Roblox", "Versions"));

            Parallel.ForEach(flagFiles, targetFile =>
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
            });
        }

        public override void Save()
        {
            // convert all flag values to strings before saving
            // (snapshot keys first - never mutate a collection mid-enumeration)
            foreach (string key in Prop.Keys.ToArray())
                Prop[key] = Prop[key]?.ToString() ?? String.Empty;

            base.Save();

            // clone the dictionary
            OriginalProp = new(Prop);
        }

        public override void Load(bool alertFailure = true)
        {
            base.Load(alertFailure);

            // normalize everything to strings up front so later comparisons
            // don't pay JsonElement overhead on every access
            foreach (string key in Prop.Keys.ToArray())
                Prop[key] = Prop[key]?.ToString() ?? String.Empty;

            // clone the dictionary
            OriginalProp = new(Prop);

            if (GetPreset("Rendering.ManualFullscreen") != "False")
                SetPreset("Rendering.ManualFullscreen", "False");
        }
    }
}
