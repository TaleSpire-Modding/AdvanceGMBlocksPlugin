namespace AdvanceGMBlocks
{
    public sealed class GMBlockData
    {
        public enum CallbackType
        {
            Process,
            Get,
            Post,
            Put,
            Delete
        }

        public sealed class CallbackData {
            public string Endpoint = string.Empty;
            public CallbackType MethodType = CallbackType.Get;
            public string Payload = string.Empty;
        }

        public Audio EnabledAudio = new Audio();
        public Mixer EnabledMixer = new Mixer();
        public Atmosphere EnabledAtmosphere = new Atmosphere();
        public CallbackData Callback = new CallbackData();

        public sealed class Audio
        {
            public bool Ambient = true;
            public bool Music = true;
        }

        public sealed class Mixer
        {
            public bool Ambient = true;
            public bool Music = true;
        }

        public sealed class Atmosphere
        {
            public bool DayCycle = true;
            public bool Fog = true;
            public bool Expose = true;
            public bool PostEffects = true;
        }
    }
}
