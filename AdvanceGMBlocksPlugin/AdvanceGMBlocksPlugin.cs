using AdvanceGMBlocksPlugin.SystemMessageExtension;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using PluginUtilities;
using RadialUI;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AdvanceGMBlocks
{
    [BepInPlugin(Guid, "Advance GM Blocks", Version)]
	[BepInDependency(RadialUIPlugin.Guid)]
	[BepInDependency(SetInjectionFlag.Guid)]
	public sealed class AdvanceGMBlocksPlugin : DependencyUnityPlugin<AdvanceGMBlocksPlugin>
	{
		// constants
		public const string Guid = "org.hollofox.plugins.AdvanceGMBlocksPlugin";
		public const string Version = "0.0.0.0";

        internal static string LocalHidden;

        // Branch Icons
        private static readonly Sprite AudioSprite = LoadEmbeddedSprite("file-audio.png");
        private static readonly Sprite MixerSprite = LoadEmbeddedSprite("sliders.png");
        private static readonly Sprite AtmosphereSprite = LoadEmbeddedSprite("cloud.png");

        // Toggle Icons
        private static readonly Sprite AmbientSprite = LoadEmbeddedSprite("volume-low.png");
        private static readonly Sprite MusicSprite = LoadEmbeddedSprite("music.png");
        private static readonly Sprite DayCycleSprite = LoadEmbeddedSprite("sun.png");
        private static readonly Sprite FogSprite = LoadEmbeddedSprite("smog.png");
        private static readonly Sprite ExposureSprite = LoadEmbeddedSprite("cloud-sun.png");
        private static readonly Sprite ImageSprite = LoadEmbeddedSprite("image.png");


        internal static Sprite LoadEmbeddedSprite(string path)
        {
            path = $"AdvanceGMBlocksPlugin.CustomData.Images.Icons.{path}";
            var asm = Assembly.GetExecutingAssembly();
            Stream stream = asm.GetManifestResourceStream(path);

            if (stream == null)
            {
                Debug.LogError("Embedded resource not found: " + path);
                return null;
            }

            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                buffer = ms.ToArray();
            }
            stream.Dispose();

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(tex, buffer))
            {
                Debug.LogError("Failed to load embedded PNG: " + path);
                return null;
            }

            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        Harmony harmony;

        /// <summary>
        /// Awake plugin
        /// </summary>
        protected override void OnAwake()
        {
            Logger.LogDebug("In Awake for Advance GM Blocks");
            
            LocalHidden = Path.GetDirectoryName(Info.Location) + "/BoardData";
            Directory.CreateDirectory(LocalHidden);

            harmony = new Harmony(Guid);
            harmony.PatchAll();
            
            RadialUIPlugin.AddCustomButtonGMBlock(Guid,
                new MapMenu.ItemArgs
                {
					CloseMenuOnActivate = false,
					Title = "Filters",
					Action = OpenFilters,
                });
        }

        protected override void OnDestroyed()
        {
            // Remove Radial UI button
            RadialUIPlugin.RemoveCustomButtonGMBlock(Guid);

            LocalHidden = null;

            // Unpatch Harmony patches
            harmony?.UnpatchSelf();

            Logger.LogDebug("Advance GM Blocks unloaded");
        }

        public void OpenFilters(MapMenuItem mapmenuItem, object obj)
        {
            MapMenu mapMenu = MapMenuManager.OpenMenu(GMBlockInteractMenuBoardTool.block.WorldPosition,true);

            string filePath = Path.Join(AdvanceGMBlocksPlugin.LocalHidden, _currentKey);
            string result = string.Empty;
            if (File.Exists(filePath))
                result = File.ReadAllText(filePath);
            
            _currentData = string.IsNullOrEmpty(result) ? new GMBlockData() : JsonConvert.DeserializeObject<GMBlockData>(result);

            mapMenu.AddMenuItem(MapMenu.MenuType.BRANCH, AudioBranch,"Audio", icon: AudioSprite, obj:obj);
            mapMenu.AddMenuItem(MapMenu.MenuType.BRANCH, MixerBranch,"Mixer", icon: MixerSprite, obj: obj);
            mapMenu.AddMenuItem(MapMenu.MenuType.BRANCH, AtmosphereBranch,"Atmosphere", icon: AtmosphereSprite, obj: obj);

            mapMenu.AddToggleItem(!string.IsNullOrWhiteSpace(_currentData.Callback.Endpoint), (i, o) =>
            {
                SystemMessageCallbackExtension.AddCallbackInput(SystemMessage.Instance, "Set Callback Process", "This is for 3rd party integration/callback", 
                    _currentData.Callback, delegate (GMBlockData.CallbackData callbackData)
                    {
                        _currentData.Callback = callbackData;
                        SaveCurrentData();
                    });
            }, "Integrated Callback", obj: obj);
        }

        private static GMBlockData _currentData;
        internal static string _currentKey;

        private void AudioBranch(MapMenu map, object obj)
        {
            map.AddToggleItem(_currentData.EnabledAudio.Ambient,(i, o) =>
            {
                _currentData.EnabledAudio.Ambient = !_currentData.EnabledAudio.Ambient;
                SaveCurrentData();
            },"Ambient",icon: AmbientSprite, obj: obj);
            map.AddToggleItem(_currentData.EnabledAudio.Music, (i, o) =>
            {
                _currentData.EnabledAudio.Music = !_currentData.EnabledAudio.Music;
                SaveCurrentData();
            }, "Music",icon: MusicSprite, obj: obj);
        }

        private void MixerBranch(MapMenu map, object obj)
        {
            map.AddToggleItem(_currentData.EnabledMixer.Ambient, (i, o) =>
            {
                _currentData.EnabledMixer.Ambient = !_currentData.EnabledMixer.Ambient;
                SaveCurrentData();
            }, "Ambient Volume", icon: AmbientSprite, obj: obj);
            map.AddToggleItem(_currentData.EnabledMixer.Music, (i, o) =>
            {
                _currentData.EnabledMixer.Music = !_currentData.EnabledMixer.Music;
                SaveCurrentData();
            }, "Music Volume", icon: MusicSprite, obj: obj);
        }

        private void AtmosphereBranch(MapMenu map, object obj)
        {
            map.AddToggleItem(_currentData.EnabledAtmosphere.DayCycle, (i, o) =>
            {
                _currentData.EnabledAtmosphere.DayCycle = !_currentData.EnabledAtmosphere.DayCycle;
                SaveCurrentData();
            }, "Day Cycle", icon: DayCycleSprite, obj:obj);
            map.AddToggleItem(_currentData.EnabledAtmosphere.Fog, (i, o) =>
            {
                _currentData.EnabledAtmosphere.Fog = !_currentData.EnabledAtmosphere.Fog;
                SaveCurrentData();
            }, "Fog", icon: FogSprite, obj: obj);
            map.AddToggleItem(_currentData.EnabledAtmosphere.Expose, (i, o) =>
            {
                _currentData.EnabledAtmosphere.Expose = !_currentData.EnabledAtmosphere.Expose;
                SaveCurrentData();
            }, "Exposure", icon: ExposureSprite, obj: obj);
            map.AddToggleItem(_currentData.EnabledAtmosphere.PostEffects, (i, o) =>
            {
                _currentData.EnabledAtmosphere.PostEffects = !_currentData.EnabledAtmosphere.PostEffects;
                SaveCurrentData();
            }, "Post Effects", icon: ImageSprite, obj: obj);
        }

        private void SaveCurrentData()
        {
            string serialized = JsonConvert.SerializeObject(_currentData);
            File.WriteAllText(Path.Join(LocalHidden, _currentKey), serialized);
        }
    }
}