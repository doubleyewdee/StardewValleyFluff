namespace doubleyewdee
{
    using System;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;

    /// <summary>The mod entry point.</summary>
    public sealed class ModEntry : Mod
    {
        private ModConfig config;
        private int idleTime = 0;
        private bool inIdle = false;

        private struct AudioLevels
        {
            public float Ambient;
            public float Footstep;
            public float Music;
            public float Sound;

            public AudioLevels(Options currentOptions)
            {
                this.Ambient = currentOptions.ambientVolumeLevel;
                this.Footstep = currentOptions.footstepVolumeLevel;
                this.Music = currentOptions.musicVolumeLevel;
                this.Sound = currentOptions.soundVolumeLevel;
            }

            public void SetOptions(Options options)
            {
                options.ambientVolumeLevel = this.Ambient;
                options.footstepVolumeLevel = this.Footstep;
                options.musicVolumeLevel = this.Music;
                options.soundVolumeLevel = this.Sound;

                options.reApplySetOptions();
            }

            public void Log(IMonitor monitor)
            {
                monitor.Log($"Audio = Ambient: {Ambient}, Footstep: {Footstep}, Music: {Music}, Sound: {Sound}", LogLevel.Info);
            }
        }

        private static readonly AudioLevels MutedAudioLevels = new AudioLevels();
        private AudioLevels normalAudioLevels;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.config = helper.ReadConfig<ModConfig>();

            SaveEvents.AfterLoad += OnGameLoaded;

            // mouse movement shouldn't interrupt idle (imo... anyway)
            ControlEvents.KeyPressed += this.OnActivity;
            ControlEvents.ControllerButtonPressed += this.OnActivity;
            ControlEvents.ControllerTriggerPressed += this.OnActivity;

            GameEvents.OneSecondTick += OnTick;
        }

        private void OnGameLoaded(object sender, EventArgs e)
        {
            this.Monitor.Log("Game loading...");
            this.normalAudioLevels = new AudioLevels(Game1.options);
            this.normalAudioLevels.Log(this.Monitor);

            if (this.config.MuteOnLoseFocus)
            {
                Game1.game1.Deactivated += (o, ev) => this.ToggleMute(true);
                Game1.game1.Activated += (o, ev) => this.ToggleMute(false);
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            this.idleTime += 1;

            if (this.idleTime >= this.config.IdleMuteTime)
            {
                this.SetIdle(true);
            }
        }

        private void OnActivity(object sender, EventArgs e)
        {
            this.idleTime = 0;
            this.SetIdle(false);
        }

        private void SetIdle(bool state)
        {
            if (this.inIdle == state)
            {
                return;
            }

            this.inIdle = state;

            if (state)
            {
                this.Monitor.Log($"Entering idle state after {this.idleTime} seconds.");
            }
            else
            {
                this.Monitor.Log("Exiting idle state.");
            }

            this.ToggleMute(state);
        }

        private void ToggleMute(bool muted)
        {
            this.Monitor.Log($"Setting mute tstate to {muted}.");
            if (muted)
            {
                MutedAudioLevels.SetOptions(Game1.options);
            }
            else
            {
                this.normalAudioLevels.SetOptions(Game1.options);
            }
        }
    }
}
