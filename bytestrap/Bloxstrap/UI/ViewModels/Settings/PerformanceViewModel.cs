using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using Bloxstrap.Enums.FlagPresets;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class PerformanceViewModel : NotifyPropertyChangedViewModel
    {
        public event EventHandler? RequestPageReloadEvent;

        // ═══════════════════════════════════
        //  Hardware Info (read-only)
        // ═══════════════════════════════════

        public string CPUName { get; } = Bloxstrap.Utility.HardwareInfo.GetCPUName();
        public string GPUName { get; } = Bloxstrap.Utility.HardwareInfo.GetGPUName();
        public string TotalRAM { get; } = Bloxstrap.Utility.HardwareInfo.GetTotalRAM();
        public string WindowsVersion { get; } = Bloxstrap.Utility.HardwareInfo.GetWindowsVersion();
        public string SystemUptime { get; } = Bloxstrap.Utility.HardwareInfo.GetUptime();
        public string DiskSpace { get; } = Bloxstrap.Utility.HardwareInfo.GetDiskSpace();
        public string GPUDriver { get; } = Bloxstrap.Utility.HardwareInfo.GetGPUDriverVersion();

        // ═══════════════════════════════════
        //  Rendering Mode
        // ═══════════════════════════════════

        public IReadOnlyDictionary<string, string> RenderingModeOptions { get; } = new Dictionary<string, string>
        {
            { "Default", "Automatic" },
            { "D3D11", "Direct3D 11" },
            { "Vulkan", "Vulkan" },
            { "OpenGL", "OpenGL" },
        };

        public string SelectedRenderingMode
        {
            get
            {
                if (App.FastFlags.GetPreset("Rendering.Mode.Vulkan") == "True") return "Vulkan";
                if (App.FastFlags.GetPreset("Rendering.Mode.OpenGL") == "True") return "OpenGL";
                if (App.FastFlags.GetPreset("Rendering.Mode.D3D11") == "True") return "D3D11";
                return "Default";
            }
            set
            {
                App.FastFlags.SetPreset("Rendering.Mode.DisableD3D11", null);
                App.FastFlags.SetPreset("Rendering.Mode.D3D11", null);
                App.FastFlags.SetPreset("Rendering.Mode.Vulkan", null);
                App.FastFlags.SetPreset("Rendering.Mode.OpenGL", null);

                switch (value)
                {
                    case "D3D11":
                        App.FastFlags.SetPreset("Rendering.Mode.D3D11", "True");
                        break;
                    case "Vulkan":
                        App.FastFlags.SetPreset("Rendering.Mode.Vulkan", "True");
                        break;
                    case "OpenGL":
                        App.FastFlags.SetPreset("Rendering.Mode.OpenGL", "True");
                        break;
                }

                OnPropertyChanged(nameof(SelectedRenderingMode));
            }
        }

        // ═══════════════════════════════════
        //  MSAA (Anti-Aliasing)
        // ═══════════════════════════════════

        public int SelectedMSAA
        {
            get => int.TryParse(App.FastFlags.GetPreset("Rendering.MSAA"), out var x) ? x : 0;
            set
            {
                App.FastFlags.SetPreset("Rendering.MSAA", value > 0 ? value.ToString() : null);
                OnPropertyChanged(nameof(SelectedMSAA));
            }
        }

        // ═══════════════════════════════════
        //  Texture Quality
        // ═══════════════════════════════════

        public bool TextureQualityOverrideEnabled
        {
            get => App.FastFlags.GetPreset("Rendering.TextureQuality.OverrideEnabled") == "True";
            set
            {
                App.FastFlags.SetPreset("Rendering.TextureQuality.OverrideEnabled", value ? "True" : null);
                if (!value)
                    App.FastFlags.SetPreset("Rendering.TextureQuality.Level", null);
                else if (TextureQualityLevel == 0)
                    TextureQualityLevel = 2;

                OnPropertyChanged(nameof(TextureQualityOverrideEnabled));
                OnPropertyChanged(nameof(TextureQualityLevel));
            }
        }

        public int TextureQualityLevel
        {
            get => int.TryParse(App.FastFlags.GetPreset("Rendering.TextureQuality.Level"), out var x) ? x : 2;
            set
            {
                App.FastFlags.SetPreset("Rendering.TextureQuality.Level", value);
                OnPropertyChanged(nameof(TextureQualityLevel));
            }
        }

        // ═══════════════════════════════════
        //  Mesh LOD (Render Distance)
        // ═══════════════════════════════════

        public bool MeshLODEnabled
        {
            get => App.FastFlags.GetPreset("Geometry.MeshLOD.L0") != null;
            set
            {
                if (value)
                {
                    MeshLODDistance = 200;
                }
                else
                {
                    App.FastFlags.SetPreset("Geometry.MeshLOD.L0", null);
                    App.FastFlags.SetPreset("Geometry.MeshLOD.L12", null);
                    App.FastFlags.SetPreset("Geometry.MeshLOD.L23", null);
                    App.FastFlags.SetPreset("Geometry.MeshLOD.L34", null);
                }

                OnPropertyChanged(nameof(MeshLODEnabled));
                OnPropertyChanged(nameof(MeshLODDistance));
            }
        }

        public int MeshLODDistance
        {
            get => int.TryParse(App.FastFlags.GetPreset("Geometry.MeshLOD.L0"), out var x) ? x : 200;
            set
            {
                App.FastFlags.SetPreset("Geometry.MeshLOD.L0", value);
                App.FastFlags.SetPreset("Geometry.MeshLOD.L12", (int)(value * 1.5));
                App.FastFlags.SetPreset("Geometry.MeshLOD.L23", value * 2);
                App.FastFlags.SetPreset("Geometry.MeshLOD.L34", (int)(value * 2.5));
                OnPropertyChanged(nameof(MeshLODDistance));
            }
        }

        // ═══════════════════════════════════
        //  FRM Quality Level Override
        // ═══════════════════════════════════

        public bool FRMOverrideEnabled
        {
            get => App.FastFlags.GetPreset("Rendering.FRMQualityOverride") != null;
            set
            {
                if (value)
                    FRMQualityLevel = 5;
                else
                    App.FastFlags.SetPreset("Rendering.FRMQualityOverride", null);

                OnPropertyChanged(nameof(FRMOverrideEnabled));
                OnPropertyChanged(nameof(FRMQualityLevel));
            }
        }

        public int FRMQualityLevel
        {
            get => int.TryParse(App.FastFlags.GetPreset("Rendering.FRMQualityOverride"), out var x) ? x : 5;
            set
            {
                App.FastFlags.SetPreset("Rendering.FRMQualityOverride", value);
                OnPropertyChanged(nameof(FRMQualityLevel));
            }
        }

        // ═══════════════════════════════════
        //  Disable Terrain
        // ═══════════════════════════════════

        public bool DisableTerrain
        {
            get => App.FastFlags.GetPreset("Performance.DisableTerrain") == "True";
            set => App.FastFlags.SetPreset("Performance.DisableTerrain", value ? "True" : null);
        }

        // ═══════════════════════════════════
        //  FPS
        // ═══════════════════════════════════

        public bool FPSUnlocked
        {
            get => App.FastFlags.GetPreset("Performance.UnlockFPS") != null;
            set
            {
                if (value)
                    TargetFPS = 9999;
                else
                    App.FastFlags.SetPreset("Performance.UnlockFPS", null);

                OnPropertyChanged(nameof(FPSUnlocked));
                OnPropertyChanged(nameof(TargetFPS));
            }
        }

        public int TargetFPS
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.UnlockFPS"), out var x) ? x : 60;
            set
            {
                App.FastFlags.SetPreset("Performance.UnlockFPS", value);
                OnPropertyChanged(nameof(TargetFPS));
                OnPropertyChanged(nameof(FPSUnlocked));
            }
        }

        public bool RenderThrottleEnabled
        {
            get => App.FastFlags.GetPreset("Performance.RenderThrottlePercentage") != null;
            set
            {
                if (value)
                    RenderThrottle = 100;
                else
                    App.FastFlags.SetPreset("Performance.RenderThrottlePercentage", null);

                OnPropertyChanged(nameof(RenderThrottleEnabled));
                OnPropertyChanged(nameof(RenderThrottle));
            }
        }

        public int RenderThrottle
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.RenderThrottlePercentage"), out var x) ? x : 100;
            set
            {
                App.FastFlags.SetPreset("Performance.RenderThrottlePercentage", value);
                OnPropertyChanged(nameof(RenderThrottle));
            }
        }

        // ═══════════════════════════════════
        //  Lighting & Shadows
        // ═══════════════════════════════════

        public bool DisableShadows
        {
            get => App.FastFlags.GetPreset("Performance.DisableShadows") == "True";
            set
            {
                App.FastFlags.SetPreset("Performance.DisableShadows", value ? "True" : null);
                App.FastFlags.SetPreset("Performance.DisablePlayerShadows", value ? "0" : null);
            }
        }

        public bool ShadowQualityEnabled
        {
            get => App.FastFlags.GetPreset("Performance.LowerQualityShadows") != null;
            set
            {
                if (value)
                    ShadowQuality = 200;
                else
                {
                    App.FastFlags.SetPreset("Performance.LowerQualityShadows", null);
                    App.FastFlags.SetPreset("Performance.LowerQualityShadowsMainView", null);
                }

                OnPropertyChanged(nameof(ShadowQualityEnabled));
                OnPropertyChanged(nameof(ShadowQuality));
            }
        }

        public int ShadowQuality
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.LowerQualityShadows"), out var x) ? x : 200;
            set
            {
                App.FastFlags.SetPreset("Performance.LowerQualityShadows", value);
                App.FastFlags.SetPreset("Performance.LowerQualityShadowsMainView", value);
                OnPropertyChanged(nameof(ShadowQuality));
            }
        }

        public bool ForceLowLighting
        {
            get => App.FastFlags.GetPreset("Performance.LightingTechnology") == "True";
            set => App.FastFlags.SetPreset("Performance.LightingTechnology", value ? "True" : null);
        }

        public bool DisablePostFX
        {
            get => App.FastFlags.GetPreset("Performance.DisablePostFX") == "True";
            set => App.FastFlags.SetPreset("Performance.DisablePostFX", value ? "True" : null);
        }

        // ═══════════════════════════════════
        //  Terrain & Environment
        // ═══════════════════════════════════

        public bool DisableGrass
        {
            get => App.FastFlags.GetPreset("Performance.DisableGrass") == "0";
            set => App.FastFlags.SetPreset("Performance.DisableGrass", value ? "0" : null);
        }

        public bool TerrainQualityEnabled
        {
            get => App.FastFlags.GetPreset("Performance.TerrainQuality") != null;
            set
            {
                if (value)
                    TerrainQuality = 4;
                else
                    App.FastFlags.SetPreset("Performance.TerrainQuality", null);

                OnPropertyChanged(nameof(TerrainQualityEnabled));
                OnPropertyChanged(nameof(TerrainQuality));
            }
        }

        public int TerrainQuality
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.TerrainQuality"), out var x) ? x : 4;
            set
            {
                App.FastFlags.SetPreset("Performance.TerrainQuality", value);
                OnPropertyChanged(nameof(TerrainQuality));
            }
        }

        // ═══════════════════════════════════
        //  Particles
        // ═══════════════════════════════════

        public bool ParticleCapEnabled
        {
            get => App.FastFlags.GetPreset("Performance.ParticleCap") != null;
            set
            {
                if (value)
                    ParticleCap = 250;
                else
                {
                    App.FastFlags.SetPreset("Performance.ParticleCap", null);
                    App.FastFlags.SetPreset("Performance.ReduceParticles", null);
                }

                OnPropertyChanged(nameof(ParticleCapEnabled));
                OnPropertyChanged(nameof(ParticleCap));
            }
        }

        public int ParticleCap
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.ParticleCap"), out var x) ? x : 250;
            set
            {
                App.FastFlags.SetPreset("Performance.ParticleCap", value);
                App.FastFlags.SetPreset("Performance.ReduceParticles", value < 200 ? "True" : null);
                OnPropertyChanged(nameof(ParticleCap));
            }
        }

        // ═══════════════════════════════════
        //  Network / Ping (all granular)
        // ═══════════════════════════════════

        public bool NetworkTuningEnabled
        {
            get => App.FastFlags.GetPreset("Performance.NetworkMTU") != null;
            set
            {
                if (value)
                {
                    MTUSize = 900;
                    SendRate = 4500;
                    SendBandwidth = 8192;
                    RecvBandwidth = 8192;
                }
                else
                {
                    App.FastFlags.SetPreset("Performance.NetworkMTU", null);
                    App.FastFlags.SetPreset("Performance.NetworkSendRate", null);
                    App.FastFlags.SetPreset("Performance.NetworkSendBuffer", null);
                    App.FastFlags.SetPreset("Performance.NetworkRecvBuffer", null);
                }

                OnPropertyChanged(nameof(NetworkTuningEnabled));
                OnPropertyChanged(nameof(MTUSize));
                OnPropertyChanged(nameof(SendRate));
                OnPropertyChanged(nameof(SendBandwidth));
                OnPropertyChanged(nameof(RecvBandwidth));
            }
        }

        public int MTUSize
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.NetworkMTU"), out var x) ? x : 1400;
            set { App.FastFlags.SetPreset("Performance.NetworkMTU", value); OnPropertyChanged(nameof(MTUSize)); }
        }

        public int SendRate
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.NetworkSendRate"), out var x) ? x : 1500;
            set { App.FastFlags.SetPreset("Performance.NetworkSendRate", value); OnPropertyChanged(nameof(SendRate)); }
        }

        public int SendBandwidth
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.NetworkSendBuffer"), out var x) ? x : 4096;
            set { App.FastFlags.SetPreset("Performance.NetworkSendBuffer", value); OnPropertyChanged(nameof(SendBandwidth)); }
        }

        public int RecvBandwidth
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.NetworkRecvBuffer"), out var x) ? x : 4096;
            set { App.FastFlags.SetPreset("Performance.NetworkRecvBuffer", value); OnPropertyChanged(nameof(RecvBandwidth)); }
        }

        // ═══════════════════════════════════
        //  Physics
        // ═══════════════════════════════════

        public bool PhysicsTuningEnabled
        {
            get => App.FastFlags.GetPreset("Performance.PhysicsThrottle") != null;
            set
            {
                if (value)
                {
                    PhysicsSendRate = 60;
                    HeartbeatMs = 0;
                }
                else
                {
                    App.FastFlags.SetPreset("Performance.PhysicsThrottle", null);
                    App.FastFlags.SetPreset("Performance.HeartbeatInterval", null);
                }

                OnPropertyChanged(nameof(PhysicsTuningEnabled));
                OnPropertyChanged(nameof(PhysicsSendRate));
                OnPropertyChanged(nameof(HeartbeatMs));
            }
        }

        public int PhysicsSendRate
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.PhysicsThrottle"), out var x) ? x : 60;
            set { App.FastFlags.SetPreset("Performance.PhysicsThrottle", value); OnPropertyChanged(nameof(PhysicsSendRate)); }
        }

        public int HeartbeatMs
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.HeartbeatInterval"), out var x) ? x : 33;
            set { App.FastFlags.SetPreset("Performance.HeartbeatInterval", value); OnPropertyChanged(nameof(HeartbeatMs)); }
        }

        public bool NetworkPrediction
        {
            get => App.FastFlags.GetPreset("Performance.NetworkPrediction") == "True";
            set => App.FastFlags.SetPreset("Performance.NetworkPrediction", value ? "True" : null);
        }

        // ═══════════════════════════════════
        //  Telemetry & Privacy
        // ═══════════════════════════════════

        public bool DisableTelemetry
        {
            get => App.FastFlags.GetPreset("Performance.DisableTelemetry") == "True";
            set
            {
                string? val = value ? "True" : null;
                App.FastFlags.SetPreset("Performance.DisableTelemetry", val);
                App.FastFlags.SetPreset("Performance.DisableTelemetry2", val);
                App.FastFlags.SetPreset("Performance.DisableTelemetry3", val);
                App.FastFlags.SetPreset("Performance.DisableTelemetry4", val);
                App.FastFlags.SetPreset("Performance.DisableTelemetry5", val);
                App.FastFlags.SetPreset("Performance.DisableTelemetry6", val);
                App.FastFlags.SetPreset("Performance.DisableTelemetry7", val);
            }
        }

        // ═══════════════════════════════════
        //  Memory & Assets
        // ═══════════════════════════════════

        public bool PreloadAssets
        {
            get => App.FastFlags.GetPreset("Performance.PreloadAssets") == "True";
            set => App.FastFlags.SetPreset("Performance.PreloadAssets", value ? "True" : null);
        }

        public bool GCTuningEnabled
        {
            get => App.FastFlags.GetPreset("Performance.GarbageCollectionFreq") != null;
            set
            {
                if (value)
                    GCFrequency = 600;
                else
                    App.FastFlags.SetPreset("Performance.GarbageCollectionFreq", null);

                OnPropertyChanged(nameof(GCTuningEnabled));
                OnPropertyChanged(nameof(GCFrequency));
            }
        }

        public int GCFrequency
        {
            get => int.TryParse(App.FastFlags.GetPreset("Performance.GarbageCollectionFreq"), out var x) ? x : 600;
            set { App.FastFlags.SetPreset("Performance.GarbageCollectionFreq", value); OnPropertyChanged(nameof(GCFrequency)); }
        }

        // ═══════════════════════════════════
        //  Bypass Mode
        // ═══════════════════════════════════

        public bool BypassMode
        {
            get => App.Settings.Prop.UseBypassFlagMethod;
            set => App.Settings.Prop.UseBypassFlagMethod = value;
        }

        // ═══════════════════════════════════
        //  Reset
        // ═══════════════════════════════════

        public ICommand ResetAllCommand => new RelayCommand(() =>
        {
            foreach (var pair in FastFlagManager.PresetFlags.Where(x => x.Key.StartsWith("Performance.")))
                App.FastFlags.SetValue(pair.Value, null);

            RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
        });

        // ═══════════════════════════════════
        //  Export / Import Performance Config
        // ═══════════════════════════════════

        public ICommand ExportConfigCommand => new RelayCommand(() =>
        {
            try
            {
                var perfFlags = new Dictionary<string, string>();
                foreach (var pair in FastFlagManager.PresetFlags.Where(x => x.Key.StartsWith("Performance.")))
                {
                    string? val = App.FastFlags.GetValue(pair.Value);
                    if (val != null)
                        perfFlags[pair.Key] = val;
                }

                if (perfFlags.Count == 0)
                {
                    MessageBox.Show("No performance settings configured to export.", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    FileName = "bytestrap-performance.json",
                    Title = "Export Performance Config"
                };

                if (dialog.ShowDialog() == true)
                {
                    string json = JsonSerializer.Serialize(perfFlags, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(dialog.FileName, json);
                    MessageBox.Show($"Exported {perfFlags.Count} settings.", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        public ICommand ImportConfigCommand => new RelayCommand(() =>
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    Title = "Import Performance Config"
                };

                if (dialog.ShowDialog() == true)
                {
                    string json = File.ReadAllText(dialog.FileName);
                    var imported = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    if (imported == null || imported.Count == 0)
                    {
                        MessageBox.Show("No valid settings found in file.", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int count = 0;
                    foreach (var pair in imported)
                    {
                        if (FastFlagManager.PresetFlags.ContainsKey(pair.Key))
                        {
                            App.FastFlags.SetValue(FastFlagManager.PresetFlags[pair.Key], pair.Value);
                            count++;
                        }
                    }

                    MessageBox.Show($"Imported {count} settings.", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestPageReloadEvent?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        // ═══════════════════════════════════
        //  Copy Flags to Clipboard
        // ═══════════════════════════════════

        public ICommand CopyFlagsCommand => new RelayCommand(() =>
        {
            try
            {
                var allFlags = new Dictionary<string, object>();
                foreach (var pair in FastFlagManager.PresetFlags)
                {
                    string? val = App.FastFlags.GetValue(pair.Value);
                    if (val != null)
                        allFlags[pair.Value] = val;
                }

                if (allFlags.Count == 0)
                {
                    MessageBox.Show("No flags configured.", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string json = JsonSerializer.Serialize(allFlags, new JsonSerializerOptions { WriteIndented = true });
                Clipboard.SetText(json);
                MessageBox.Show($"Copied {allFlags.Count} flags to clipboard.", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Copy failed: {ex.Message}", "Bytestrap", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });

        // ═══════════════════════════════════
        //  Ping Tester
        // ═══════════════════════════════════

        private string _pingResult = "Not tested";
        public string PingResult
        {
            get => _pingResult;
            set { _pingResult = value; OnPropertyChanged(nameof(PingResult)); }
        }

        private bool _pingTesting = false;
        public bool PingTesting
        {
            get => _pingTesting;
            set { _pingTesting = value; OnPropertyChanged(nameof(PingTesting)); }
        }

        public ICommand TestPingCommand => new AsyncRelayCommand(async () =>
        {
            PingTesting = true;
            PingResult = "Testing...";

            try
            {
                var ping = new Ping();
                var targets = new[] { "roblox.com", "rbxcdn.com", "rbxtrk.com" };
                var results = new List<string>();

                foreach (var target in targets)
                {
                    try
                    {
                        var reply = await ping.SendPingAsync(target, 3000);
                        results.Add(reply.Status == IPStatus.Success
                            ? $"{target}: {reply.RoundtripTime}ms"
                            : $"{target}: timeout");
                    }
                    catch
                    {
                        results.Add($"{target}: failed");
                    }
                }

                PingResult = string.Join("  |  ", results);
            }
            catch (Exception ex)
            {
                PingResult = $"Error: {ex.Message}";
            }
            finally
            {
                PingTesting = false;
            }
        });

        // ═══════════════════════════════════
        //  Active Flags Counter
        // ═══════════════════════════════════

        public int ActiveFlagCount
        {
            get
            {
                int count = 0;
                foreach (var pair in FastFlagManager.PresetFlags.Where(x => x.Key.StartsWith("Performance.")))
                {
                    if (App.FastFlags.GetValue(pair.Value) != null)
                        count++;
                }
                return count;
            }
        }

        public int TotalFlagCount => FastFlagManager.PresetFlags.Count(x => x.Key.StartsWith("Performance."));
    }
}
