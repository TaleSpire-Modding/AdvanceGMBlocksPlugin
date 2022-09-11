using Bounce.Singletons;
using HarmonyLib;
using LordAshes;
using Newtonsoft.Json;
using TaleSpire.GameMaster.Blocks;

namespace AdvanceGMBlocks.GMBlockPatches
{
    [HarmonyPatch(typeof(GMBlockButtonAtmosphere), "OnApply")] 
    internal class GMBlockMenuPatch
    {
        internal static bool Prefix(MapMenuItem arg1, object arg2, ref GMDataBlockBase ____base)
        {
            var original = AtmosphereManager.Instance.GetAtmosphere();

            var _base = ____base;

            var result = 
                AssetDataPlugin.ReadInfo(AdvanceGMBlocksPlugin.Guid, _base.AtmosphereBlock.Id.ToString());

            if (string.IsNullOrWhiteSpace(result))
                return true;

            var enabled = JsonConvert.DeserializeObject<GMBlockData>(result);

            if (!enabled.EnabledAudio.Ambient)
                _base.AtmosphereBlock.Data.ambientMusic = original.ambientMusic;
            if (!enabled.EnabledAudio.Music)
                _base.AtmosphereBlock.Data.music = original.music;

            if (!enabled.EnabledMixer.Ambient)
                _base.AtmosphereBlock.Data.ambientVolume = original.ambientVolume;
            if (!enabled.EnabledMixer.Music)
                _base.AtmosphereBlock.Data.musicVolume = original.musicVolume;

            if (!enabled.EnabledAtmosphere.DayCycle)
            {
                _base.AtmosphereBlock.Data.dayCycle = original.dayCycle;
                _base.AtmosphereBlock.Data.timeOfDay = original.timeOfDay;
                _base.AtmosphereBlock.Data.sunDirection = original.sunDirection;
            }

            if (!enabled.EnabledAtmosphere.Fog)
                _base.AtmosphereBlock.Data.fogMultiplier = original.fogMultiplier;
            
            if (!enabled.EnabledAtmosphere.Expose)
                _base.AtmosphereBlock.Data.exposure = original.exposure;

            if (!enabled.EnabledAtmosphere.PostEffects)
            {
                _base.AtmosphereBlock.Data.posteffect = original.posteffect;
                _base.AtmosphereBlock.Data.postEffectAmount = original.postEffectAmount;
            }

            SimpleSingletonBehaviour<AtmosphereManager>.Instance.SetAtmosphere(_base.AtmosphereBlock.Data, true);
            return false;
        }
    }
}