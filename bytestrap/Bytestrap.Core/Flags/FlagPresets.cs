namespace Bytestrap.Core.Flags;

/// <summary>
/// Maps friendly preset keys (e.g. "Performance.UnlockFPS") to real Roblox
/// FastFlag names (e.g. "DFIntTaskSchedulerTargetFps").
///
/// This is the cross-platform twin of the Windows app's FastFlagManager
/// preset table - keep the two in sync when flags change.
/// </summary>
public static class FlagPresets
{
    public static readonly IReadOnlyDictionary<string, string> PresetFlags =
        new Dictionary<string, string>(StringComparer.Ordinal)
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
            { "Geometry.MeshLOD.Static", "DFIntCSGLevelOfDetailSwitchingDistanceStatic" },
            { "Geometry.MeshLOD.L0", "DFIntCSGLevelOfDetailSwitchingDistance" },
            { "Geometry.MeshLOD.L12", "DFIntCSGLevelOfDetailSwitchingDistanceL12" },
            { "Geometry.MeshLOD.L23", "DFIntCSGLevelOfDetailSwitchingDistanceL23" },
            { "Geometry.MeshLOD.L34", "DFIntCSGLevelOfDetailSwitchingDistanceL34" },

            // Texture quality
            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },

            // === PERFORMANCE FLAGS ===

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

    private static readonly HashSet<string> _flagNames =
        new(PresetFlags.Values, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, KeyValuePair<string, string>[]> _prefixCache =
        new(StringComparer.Ordinal);

    private static readonly object _prefixCacheLock = new();

    /// <summary>All preset pairs whose key starts with <paramref name="prefix"/> (cached).</summary>
    public static IReadOnlyList<KeyValuePair<string, string>> GetPairsByPrefix(string prefix)
    {
        lock (_prefixCacheLock)
        {
            if (_prefixCache.TryGetValue(prefix, out var cached))
                return cached;

            var matches = PresetFlags
                .Where(x => x.Key.StartsWith(prefix, StringComparison.Ordinal))
                .ToArray();

            _prefixCache[prefix] = matches;
            return matches;
        }
    }

    /// <summary>Resolves a friendly preset key OR a raw FFlag name to a raw FFlag name.</summary>
    public static string ResolveFlagName(string presetOrFlag)
    {
        if (PresetFlags.TryGetValue(presetOrFlag, out string? flag))
            return flag;

        return presetOrFlag;
    }

    /// <summary>True if <paramref name="flag"/> is one of the known preset FFlags.</summary>
    public static bool IsPresetFlag(string flag) => _flagNames.Contains(flag);

    /// <summary>True if <paramref name="name"/> is a known friendly preset key.</summary>
    public static bool IsPresetKey(string name) => PresetFlags.ContainsKey(name);
}
