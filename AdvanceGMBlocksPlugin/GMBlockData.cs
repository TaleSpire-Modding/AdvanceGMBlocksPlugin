namespace AdvanceGMBlocks
{
    public class GMBlockData
    {
        public Audio EnabledAudio = new Audio();
        public Mixer EnabledMixer = new Mixer();
        public Atmosphere EnabledAtmosphere = new Atmosphere();

        public class Audio
        {
            public bool Ambient = true;
            public bool Music = true;
        }

        public class Mixer
        {
            public bool Ambient = true;
            public bool Music = true;
        }

        public class Atmosphere
        {
            public bool DayCycle = true;
            public bool Fog = true;
            public bool Expose = true;
            public bool PostEffects = true;
        }
    }
}
