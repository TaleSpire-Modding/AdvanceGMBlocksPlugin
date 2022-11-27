namespace AdvanceGMBlocks
{
    public sealed class GMBlockData
    {
        public Audio EnabledAudio = new Audio();
        public Mixer EnabledMixer = new Mixer();
        public Atmosphere EnabledAtmosphere = new Atmosphere();

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
