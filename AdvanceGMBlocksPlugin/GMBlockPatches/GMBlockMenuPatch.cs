using Bounce.Singletons;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using TaleSpire.GameMaster.Blocks;
using static AdvanceGMBlocks.GMBlockData;

namespace AdvanceGMBlocks.GMBlockPatches
{

    [HarmonyPatch(typeof(GMBlockButtonAtmosphere), "OnOpenMenu")]
    internal sealed class GMBlockMenuPatchOpen
    {
        internal static void Prefix(ref GMDataBlockBase ____base)
        {
            AdvanceGMBlocksPlugin._currentKey = ____base.AtmosphereBlock.Id.ToString();
        }
    }

    [HarmonyPatch(typeof(GMBlockButtonAtmosphere), "OnApply")] 
    internal sealed class GMBlockMenuPatch
    {
        internal static bool Prefix(MapMenuItem arg1, object arg2, ref GMDataBlockBase ____base)
        {
            DataModel.AtmosphereData original = AtmosphereManager.Instance.GetAtmosphere();

            GMDataBlockBase _base = ____base;

            string result = 
                AssetDataPlugin.ReadInfo(AdvanceGMBlocksPlugin.Guid, _base.AtmosphereBlock.Id.ToString());

            if (string.IsNullOrWhiteSpace(result))
                return true;

            GMBlockData enabled = JsonConvert.DeserializeObject<GMBlockData>(result);

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
            if (!string.IsNullOrWhiteSpace(enabled.Callback.Endpoint))
            {
                Uri.TryCreate(enabled.Callback.Endpoint, UriKind.Absolute, out Uri uri);
                HttpClient client = new HttpClient();
                switch (enabled.Callback.MethodType)
                {
                    case CallbackType.Get:
                        if (Uri.TryCreate(enabled.Callback.Endpoint, UriKind.Absolute, out Uri getUri))
                            client.GetAsync(getUri);
                        break;
                    case CallbackType.Post:
                        if (Uri.TryCreate(enabled.Callback.Endpoint, UriKind.Absolute, out Uri postUri))
                            client.PostAsync(uri, new StringContent(enabled.Callback.Payload, Encoding.UTF8, "application/json"));
                        break;
                    case CallbackType.Put:
                        if (Uri.TryCreate(enabled.Callback.Endpoint, UriKind.Absolute, out Uri putUri))
                            client.PutAsync(uri, new StringContent(enabled.Callback.Payload, Encoding.UTF8, "application/json"));
                        break;
                    case CallbackType.Delete:
                        if (Uri.TryCreate(enabled.Callback.Endpoint, UriKind.Absolute, out Uri deleteUri))
                            client.DeleteAsync(uri);
                        break;
                    case CallbackType.Process:
                        // Desktop call
                        try
                        {
                            Process.Start(enabled.Callback.Endpoint);
                        }
                        catch (Exception)
                        {

                        }
                        break;
                    default:
                        break;
                }
            }
            return false;
        }
    }
}