namespace Bytestrap.Core.Flags;

/// <summary>
/// One-click performance profiles. Keys are friendly preset keys (see
/// <see cref="FlagPresets"/>); values are the flag values to set.
///
/// These mirror the Windows app's quick presets so configs behave the same
/// on both platforms. `null` values are not stored - profiles only set.
/// </summary>
public static class PerformanceProfiles
{
    public sealed record Profile(string Name, string Description, IReadOnlyDictionary<string, string> Flags);

    public static readonly IReadOnlyDictionary<string, Profile> All =
        new Dictionary<string, Profile>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "max-fps",
                new Profile(
                    "max-fps",
                    "Unlock FPS, lower visuals, disable shadows & effects for maximum framerate.",
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        { "Performance.UnlockFPS", "9999" },
                        { "Rendering.TextureQuality.OverrideEnabled", "True" },
                        { "Rendering.TextureQuality.Level", "0" },
                        { "Performance.DisableShadows", "True" },
                        { "Performance.DisablePlayerShadows", "0" },
                        { "Performance.LightingTechnology", "True" },
                        { "Performance.DisablePostFX", "True" },
                        { "Performance.DisableGrass", "0" },
                        { "Performance.ParticleCap", "50" },
                        { "Performance.ReduceParticles", "True" },
                        { "Rendering.FRMQualityOverride", "1" },
                        { "Performance.RenderThrottlePercentage", "50" },
                        { "Performance.DisableTelemetry", "True" },
                        { "Performance.DisableTelemetry2", "True" },
                        { "Performance.DisableTelemetry3", "True" },
                        { "Performance.DisableTelemetry4", "True" },
                        { "Performance.DisableTelemetry5", "True" },
                        { "Performance.DisableTelemetry6", "True" },
                        { "Performance.DisableTelemetry7", "True" },
                    })
            },
            {
                "low-ping",
                new Profile(
                    "low-ping",
                    "Tune network, physics rate and prediction for the lowest latency.",
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        { "Performance.NetworkMTU", "900" },
                        { "Performance.NetworkSendRate", "4500" },
                        { "Performance.NetworkSendBuffer", "8192" },
                        { "Performance.NetworkRecvBuffer", "8192" },
                        { "Performance.PhysicsThrottle", "120" },
                        { "Performance.HeartbeatInterval", "0" },
                        { "Performance.NetworkPrediction", "True" },
                    })
            },
            {
                "ultra-low-latency",
                new Profile(
                    "ultra-low-latency",
                    "Aggressive low-ping tuning plus uncapped FPS and minimal background work.",
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        { "Performance.UnlockFPS", "9999" },
                        { "Performance.NetworkMTU", "576" },
                        { "Performance.NetworkSendRate", "6000" },
                        { "Performance.NetworkSendBuffer", "8192" },
                        { "Performance.NetworkRecvBuffer", "8192" },
                        { "Performance.PhysicsThrottle", "240" },
                        { "Performance.HeartbeatInterval", "0" },
                        { "Performance.NetworkPrediction", "True" },
                        { "Performance.DisablePostFX", "True" },
                        { "Performance.DisableTelemetry", "True" },
                        { "Performance.DisableTelemetry2", "True" },
                        { "Performance.DisableTelemetry3", "True" },
                        { "Performance.DisableTelemetry4", "True" },
                        { "Performance.DisableTelemetry5", "True" },
                        { "Performance.DisableTelemetry6", "True" },
                        { "Performance.DisableTelemetry7", "True" },
                        { "Performance.GarbageCollectionFreq", "1200" },
                    })
            },
            {
                "balanced",
                new Profile(
                    "balanced",
                    "Sensible middle ground: high FPS cap, medium visuals, mild network tuning.",
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        { "Performance.UnlockFPS", "240" },
                        { "Rendering.TextureQuality.OverrideEnabled", "True" },
                        { "Rendering.TextureQuality.Level", "2" },
                        { "Performance.LowerQualityShadows", "200" },
                        { "Performance.LowerQualityShadowsMainView", "200" },
                        { "Performance.ParticleCap", "250" },
                        { "Performance.TerrainQuality", "4" },
                        { "Rendering.FRMQualityOverride", "5" },
                        { "Performance.NetworkMTU", "1400" },
                        { "Performance.NetworkSendRate", "1500" },
                        { "Performance.NetworkSendBuffer", "4096" },
                        { "Performance.NetworkRecvBuffer", "4096" },
                        { "Performance.NetworkPrediction", "True" },
                        { "Performance.GarbageCollectionFreq", "600" },
                    })
            },
            {
                "max-graphics",
                new Profile(
                    "max-graphics",
                    "Maximum visual quality: high textures, long render distance, MSAA x4.",
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        { "Rendering.TextureQuality.OverrideEnabled", "True" },
                        { "Rendering.TextureQuality.Level", "3" },
                        { "Rendering.FRMQualityOverride", "10" },
                        { "Geometry.MeshLOD.L0", "1000" },
                        { "Geometry.MeshLOD.L12", "1500" },
                        { "Geometry.MeshLOD.L23", "2000" },
                        { "Geometry.MeshLOD.L34", "2500" },
                        { "Performance.LowerQualityShadows", "10" },
                        { "Performance.LowerQualityShadowsMainView", "10" },
                        { "Performance.ParticleCap", "1000" },
                        { "Rendering.MSAA", "4" },
                        { "Rendering.Mode.D3D11", "True" },
                    })
            },
        };

    /// <summary>
    /// Renders a profile to raw FFlag name/value pairs for ClientAppSettings.json.
    /// </summary>
    public static Dictionary<string, string> Render(Profile profile)
    {
        var rendered = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var pair in profile.Flags)
            rendered[FlagPresets.ResolveFlagName(pair.Key)] = pair.Value;

        return rendered;
    }
}
