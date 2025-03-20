using AdvanceGMBlocksPlugin.SystemMessageExtension;
using BepInEx;
using DataModel;
using HarmonyLib;
using LordAshes;
using Newtonsoft.Json;
using PluginUtilities;
using RadialUI;
using UnityEngine;

namespace AdvanceGMBlocks
{
    [BepInPlugin(Guid, "Advance GM Blocks", Version)]
	[BepInDependency(RadialUIPlugin.Guid)]
	[BepInDependency(FileAccessPlugin.Guid)]
	[BepInDependency(AssetDataPlugin.Guid)]
	[BepInDependency(SetInjectionFlag.Guid)]
	public sealed class AdvanceGMBlocksPlugin : BaseUnityPlugin
	{
		// constants
		public const string Guid = "org.hollofox.plugins.AdvanceGMBlocksPlugin";
		public const string Version = "0.0.0.0";

        // Branch Icons
        private static readonly Sprite AudioSprite = FileAccessPlugin.Image.LoadSprite("file-audio.png");
        private static readonly Sprite MixerSprite = FileAccessPlugin.Image.LoadSprite("sliders.png");
        private static readonly Sprite AtmosphereSprite = FileAccessPlugin.Image.LoadSprite("cloud.png");

        // Toggle Icons
        private static readonly Sprite AmbientSprite = FileAccessPlugin.Image.LoadSprite("volume-low.png");
        private static readonly Sprite MusicSprite = FileAccessPlugin.Image.LoadSprite("music.png");
        private static readonly Sprite DayCycleSprite = FileAccessPlugin.Image.LoadSprite("sun.png");
        private static readonly Sprite FogSprite = FileAccessPlugin.Image.LoadSprite("smog.png");
        private static readonly Sprite ExposureSprite = FileAccessPlugin.Image.LoadSprite("cloud-sun.png");
        private static readonly Sprite ImageSprite = FileAccessPlugin.Image.LoadSprite("image.png");

		/// <summary>
		/// Awake plugin
		/// </summary>
		void Awake()
		{
			Logger.LogInfo("In Awake for Advance GM Blocks");
            Debug.Log("Advance GM Blocks Plug-in loaded");

            Harmony harmony = new Harmony(Guid);
            harmony.PatchAll();
            
            ModdingTales.ModdingUtils.AddPluginToMenuList(this, "HolloFoxes'");
            
            RadialUIPlugin.AddCustomButtonGMBlock(Guid,
                new MapMenu.ItemArgs
                {
					CloseMenuOnActivate = false,
					Title = "Filters",
					Action = OpenFilters,
                    // Icon = Icons.GetIconSprite("filter")
                });
        }

		public void OpenFilters(MapMenuItem mapmenuItem, object obj)
        {
            MapMenu mapMenu = MapMenuManager.OpenMenu(GMBlockInteractMenuBoardTool.block.WorldPosition,true);

            string result = AssetDataPlugin.ReadInfo(Guid, _currentKey);
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
            AssetDataPlugin.SetInfo(Guid,_currentKey,serialized);
        }
    }
}