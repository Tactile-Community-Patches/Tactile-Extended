using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
#if MONOGAME
using Microsoft.Xna.Framework.Audio;
#else
using MonoGame.Framework.Audio;
#endif

/*bgm
bgs
me...?		only arena victory?
se


fade bgm volume to 0
fade bgm volume back to full

fade bgm volume to ducked volume
fade bgm volume back to normal

fade sfx
fade bgs


load audio files into memory?????
***like 4 simultaneous bgms able to be playing?
1 bgs
***1 me
24 sfxes


***map theme and battle theme separate, 'saveable', restartable
***cueing music to start after the current song fades out
***bgm pauses entirely while music effect is playing


ablaze functionality (playing two audio tracks synchronized and fading between them)
pitch shifting on sfx
per sound effect volume control
sfx pan
reverse sfx pan for reversed anim
flip sfx baking in pan...?









***play music
***play music or restore volume
***play map theme
***resume map theme, or play if new
***	if already playing restore volume; if not playing and resuming, fade in
***play battle theme
***resume battle theme, or play if new
***	if already playing restore volume; if not playing and resuming, fade in
***stop music
***fade out music









audio streaming performance tester
new MusicPlayer subclass
every second/half second/test as needed, change to a new BGM
set the start position of that new track to 25-75% of the way through its loop
make the frame rate stay above 60 even with all these music changes
*/

namespace Tactile.Services.Audio
{
    enum MusicFadeStates { None, FadingOut, FadedOut, FadingIn, FadedIn }

    class MusicInstance: IDisposable
    {
        #region Fields
        private List<MusicInstanceChannel> Channels;
        public  int ActiveChannelIndex { get; private set; }

        private MusicFadeStates FadeState;
        private int FadeTime, FadeRemaining;
        private float BgmVolumeLevel;
        private float Volume = 1f;
        public bool Finished { get; private set; }
        #endregion
        #region Properties
        private List<MusicInstanceChannel> InactiveChannels
        {
            get { return Channels.Where(channel => channel != ActiveChannel).ToList(); }
        }
        public string BgmName { get { return ActiveChannel.BgmName; } }
        private MusicInstanceChannel ActiveChannel
        {
            get { return Channels[ActiveChannelIndex]; }
        }
        public bool IsFadeOut
        {
            get
            {
                return FadeState == MusicFadeStates.FadingOut ||
                    FadeState == MusicFadeStates.FadedOut;
            }
        }
        public bool IsFadeIn
        {
            get
            {
                return FadeState == MusicFadeStates.FadingIn ||
                    FadeState == MusicFadeStates.FadedIn;
            }
        }
        public bool FadedOut { get { return FadeState == MusicFadeStates.None; } }
        public float FadeVolume
        {
            get
            {
                if (FadeTime == 0)
                    return 1f;

                if (this.IsFadeOut)
                    return FadeRemaining / (float)FadeTime;
                else
                    return (FadeTime - FadeRemaining) / (float)FadeTime;
            }
        }
        public bool IsPlaying { get { return ActiveChannel.IsPlaying; } }
        #endregion

        #region "inherited" methods
        public void FadeOut(int time)
        {
            // If already fading out, adjust fadeout time to keep the
            // current volume the same
            if (this.IsFadeOut)
                FadeRemaining = (FadeRemaining * time) / FadeTime;
            else if (this.IsFadeIn)
                FadeRemaining = time - ((FadeRemaining * time) / FadeTime);
            else
                FadeRemaining = time;
            FadeTime = time;

            FadeState = MusicFadeStates.FadingOut;
            Volume = this.FadeVolume;
            RefreshVolume();
        }
        public void FadeIn(int time)
        {
            // If already fading out, adjust fadeout time to keep the
            // current volume the same
            if (this.IsFadeOut)
                FadeRemaining = time - ((FadeRemaining * time) / FadeTime);
            else if (this.IsFadeIn)
                FadeRemaining = (FadeRemaining * time) / FadeTime;
            else
                FadeRemaining = time;
            FadeTime = time;

            FadeState = MusicFadeStates.FadingIn;
            Volume = this.FadeVolume;
            RefreshVolume();
        }

        private void EndFade()
        {
            FadeRemaining = 0;
            FadeState = MusicFadeStates.None;
            Volume = this.FadeVolume;
        }

        public void RefreshVolume(float musicVolume)
        {
            BgmVolumeLevel = musicVolume;
            RefreshVolume();
        }
        private void RefreshVolume()
        {
            foreach (MusicInstanceChannel channel in Channels)
                channel.RefreshVolume(BgmVolumeLevel * Volume);
        }

        public void Play()
        {
            foreach (MusicInstanceChannel channel in Channels)
                channel.Play();
        }
        public void Pause()
        {
            foreach (MusicInstanceChannel channel in Channels)
                channel.Pause();
        }
        public void Update(float musicVolume)
        {
            UpdateFade();

            RefreshVolume(musicVolume);

            if (!ActiveChannel.IsLooped && ActiveChannel.State == SoundState.Stopped)
                Finished = true;

            foreach (MusicInstanceChannel channel in Channels)
                channel.Update(musicVolume);
        }
        private void UpdateFade()
        {
            switch (FadeState)
            {
                case MusicFadeStates.FadingOut:
                case MusicFadeStates.FadingIn:
                    if (FadeRemaining > 0)
                        FadeRemaining--;
                    Volume = this.FadeVolume;
                    if (FadeRemaining == 0)
                    {
                        if (FadeState == MusicFadeStates.FadingOut)
                            FadeState = MusicFadeStates.FadedOut;
                        else
                            FadeState = MusicFadeStates.FadedIn;
                    }
                    break;
                case MusicFadeStates.FadedOut:
                case MusicFadeStates.FadedIn:
                    if (FadeState == MusicFadeStates.FadedOut)
                        Pause();

                    EndFade();
                    break;
            }
        }
        #endregion

        public bool containsCue(string cue)
        {
            foreach (MusicInstanceChannel channel in Channels)
                if (channel.BgmName == cue)
                    return true;
            return false;
        }
        public MusicInstance(List<SoundEffectInstance> instances, List<string> bgmNames, float musicVolume, int activeChannel)
        {
            Channels = new List<MusicInstanceChannel> { };
            ActiveChannelIndex = activeChannel;

            for (int n = 0; n < instances.Count; n++)
                Channels.Add(new MusicInstanceChannel(instances[n], bgmNames[n], musicVolume,
                    (n == ActiveChannelIndex)));
        }
        public void next_channel(int time)
        {
            ActiveChannel.FadeOut(time);
            ActiveChannelIndex = (ActiveChannelIndex + 1) % Channels.Count;
            ActiveChannel.FadeIn(time);
        }
        public void switch_channel(int index, int time)
        {
            if (time > 0)
            {
                ActiveChannel.FadeOut(time);
                ActiveChannelIndex = index;
                ActiveChannel.FadeIn(time);
            }
            else
                ActiveChannelIndex = index;
        }
        public void switch_channel(string cue, int time)
        {
            for (int n = 0; n < Channels.Count; n++)
                if (Channels[n].BgmName == cue)
                    switch_channel(n, time);
        }
        public void Dispose()
        {
            foreach (MusicInstanceChannel channel in Channels)
                channel.Dispose();
        }
    }
    class MusicInstanceChannel : IDisposable
    {
        public string BgmName { get; private set; }
        private SoundEffectInstance Music;
        private MusicFadeStates FadeState;
        private int FadeTime, FadeRemaining;
        private float BgmVolumeLevel;
        private float Volume = 1f;
        public bool Finished { get; private set; }

        public bool IsLooped { get { return Music.IsLooped; } }
        public SoundState State { get { return Music.State; } }
        public bool IsFadeOut
        {
            get
            {
                return FadeState == MusicFadeStates.FadingOut ||
                    FadeState == MusicFadeStates.FadedOut;
            }
        }
        public bool IsFadeIn
        {
            get
            {
                return FadeState == MusicFadeStates.FadingIn ||
                    FadeState == MusicFadeStates.FadedIn;
            }
        }

        public bool FadedOut { get { return FadeState == MusicFadeStates.None; } }
        public float FadeVolume
        {
            get
            {
                if (FadeTime == 0)
                    return 1f;

                if (this.IsFadeOut)
                    return FadeRemaining / (float)FadeTime;
                else
                    return (FadeTime - FadeRemaining) / (float)FadeTime;
            }
        }
        public bool IsPlaying { get { return Music.State == SoundState.Playing; } }

        public MusicInstanceChannel(SoundEffectInstance instance, string bgmName, float musicVolume, bool Active)
        {
            Music = instance;
            BgmName = bgmName;
            Volume = Active ? 1f : 0f;

            RefreshVolume(musicVolume);
        }

        public override string ToString()
        {
            return string.Format("MusicInstance: {0}, {1}", BgmName, Music.State);
        }

        #region Update
        public void Update(float musicVolume)
        {
            UpdateFade();

            RefreshVolume(musicVolume);

            if (!Music.IsLooped && Music.State == SoundState.Stopped)
                Finished = true;
        }

        private void UpdateFade()
        {
            switch (FadeState)
            {
                case MusicFadeStates.FadingOut:
                case MusicFadeStates.FadingIn:
                    if (FadeRemaining > 0)
                        FadeRemaining--;
                    Volume = this.FadeVolume;
                    if (FadeRemaining == 0)
                    {
                        if (FadeState == MusicFadeStates.FadingOut)
                            FadeState = MusicFadeStates.FadedOut;
                        else
                            FadeState = MusicFadeStates.FadedIn;
                    }
                    break;
                case MusicFadeStates.FadedOut:
                case MusicFadeStates.FadedIn:
                    //if (FadeState == MusicFadeStates.FadedOut)
                        //Pause();

                    EndFade();
                    break;
            }
        }

        public void RefreshVolume(float musicVolume)
        {
            BgmVolumeLevel = musicVolume;
            RefreshVolume();
        }
        private void RefreshVolume()
        {
            Music.Volume = BgmVolumeLevel * Volume;
        }
        #endregion

        #region Controls
        public void Play()
        {
            if (Music.State != SoundState.Initial)
                Music.Resume();

            if (Music.State != SoundState.Playing)
                Music.Play();

            // Maybe switch to fading in if fading out, instead? //@Debug
            if (FadeState == MusicFadeStates.FadingOut ||
                    FadeState == MusicFadeStates.FadedOut)
                EndFade();
        }

        public void Pause()
        {
            if (Music.State != SoundState.Paused)
                Music.Pause();
        }
        #endregion

        #region Fade
        public void FadeOut(int time)
        {
            // If already fading out, adjust fadeout time to keep the
            // current volume the same
            if (this.IsFadeOut)
                FadeRemaining = (FadeRemaining * time) / FadeTime;
            else if (this.IsFadeIn)
                FadeRemaining = time - ((FadeRemaining * time) / FadeTime);
            else
                FadeRemaining = time;
            FadeTime = time;

            FadeState = MusicFadeStates.FadingOut;
            Volume = this.FadeVolume;
            RefreshVolume();
        }
        public void FadeIn(int time)
        {
            // If already fading out, adjust fadeout time to keep the
            // current volume the same
            if (this.IsFadeOut)
                FadeRemaining = time - ((FadeRemaining * time) / FadeTime);
            else if (this.IsFadeIn)
                FadeRemaining = (FadeRemaining * time) / FadeTime;
            else
                FadeRemaining = time;
            FadeTime = time;
            
            FadeState = MusicFadeStates.FadingIn;
            Volume = this.FadeVolume;
            RefreshVolume();
        }

        private void EndFade()
        {
            FadeRemaining = 0;
            FadeState = MusicFadeStates.None;
            //Volume = this.FadeVolume;
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Music != null)
                    {
                        Music.Stop();
                        Music.Dispose();
                    }
                    Music = null;
                }

                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
