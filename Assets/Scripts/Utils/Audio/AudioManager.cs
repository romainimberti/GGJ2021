using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Text;
using UnityEngine.Audio;
using com.romainimberti.ggj2021.utilities.audio;

namespace com.romainimberti.ggj2021.utilities
{
    /// <summary>
    /// Controls all audio played to the user.
    /// Using specific channels which determine how the audio should be played.
    /// </summary>
    public class AudioManager : SingletonBehaviour<AudioManager>
    {

        #region Variables
        #region Enums
        /*
         * The Below region contains the enums for every audio clip, in a one to one relation.
         * They are split into different catagories, with each it's particular enum defining
         * how it should be played.
         * TO ADD - *****ADD TO THE BOTTOM******, if you add to the middle/top, they will all get reset
         * In the editor, this isn't fun. try work on a fix for this.
         *
         */

        /// <summary>
        /// Holds the different channels audio can be played on.
        /// </summary>
        public enum CHANNEL
        {
            MUSIC,
            SFX
        }


        /// <summary>
        /// The music clips playing in the background
        /// </summary>
        public enum MUSIC
        {

        }

        /// <summary>
        /// All our sound effects.
        /// </summary>
        public enum SFX
        {
            Click,
            Step1,
            Step2,
            Step3,
            Step4,
            Step5
        }

        #endregion
        #region Private

        /// <summary>
        /// This list contains all the clips we will play.
        /// </summary>
        [SerializeField]
        [Tooltip("The audio clips we wish to play in app")]
        public List<AudioAsset> clips;


        /// <summary>
        /// A set of dictionaries allowing quick lookup of the correct clipdata by its enum.
        /// This is implemented such that we don't need to put clips in the correct order in editor
        /// </summary>
        private Dictionary<SFX, AudioAsset> getSFXCLIP = new Dictionary<SFX, AudioAsset>();
        private Dictionary<MUSIC, AudioAsset> getMusicClip = new Dictionary<MUSIC, AudioAsset>();


        /// <summary>
        /// Dictionary giving a channel to it's output source on the master input.
        /// TODO automate the creation of this list.
        /// </summary>
        private Dictionary<CHANNEL, string> channelToOutput = new Dictionary<CHANNEL, string>()
        {
            {CHANNEL.MUSIC,"Music"},
            {CHANNEL.SFX,"SFX"}
        };

        /// <summary>
        /// The Master audio mixer for all channels.
        /// </summary>
        [SerializeField]
        [Tooltip("The master Audio Mixer")]
        private AudioMixer masterAudioMixer;


        #endregion
        #region Public

        /// <summary>
        /// A dictionary for audiochannel lookup.
        /// </summary>
        private Dictionary<CHANNEL, AudioChannel> getChannel = new Dictionary<CHANNEL, AudioChannel>();

        /// <summary>
        /// The AudioChannels used for playing sound effects.
        /// </summary>
        [SerializeField]
        [Tooltip("The AudioChannels to play sound through")]
        private List<ChannelData> AudioChannels = new List<ChannelData>();

        /// <summary>
        /// Relates a channel to its enum.
        /// </summary>
        [Serializable]
        private struct ChannelData
        {
            [SerializeField]
            [Tooltip("The channel type")]
            public CHANNEL type;

            [SerializeField]
            [Tooltip("The channel script handling this channel")]
            public AudioChannel controller;
        }

        #endregion
        #endregion
        #region Methods
        #region Unity
        /// <summary>
        /// Creates the channels to play audio through.
        /// Then checks you've added all the required sounds in.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            CreateChannels();
            CreateDictionaries();
#if UNITY_EDITOR
            ClipVerification();
#endif

            Mute(!PlayerPrefsX.GetBool(PlayerPrefsKeys.KEY_MUSIC_ON, true), AudioManager.CHANNEL.MUSIC);
            Mute(!PlayerPrefsX.GetBool(PlayerPrefsKeys.KEY_SOUND_EFFECTS_ON, true), AudioManager.CHANNEL.SFX);
        }

        #endregion
        #region Public

        /// <summary>
        /// Plays a music audio clip.
        /// </summary>
        /// <param name="clip">The music to play</param>
        /// <param name="onFinish">Callback on clip finished</param>
        public void PlayAudioClip(MUSIC clip, UnityAction onFinish = null)
        {
            if(!getMusicClip.ContainsKey(clip))
                Awake();
            getChannel[CHANNEL.MUSIC].PlayClip(getMusicClip[clip].GetClip());
        }

        /// <summary>
        /// returns if a specific channel is muted.
        /// </summary>
        /// <param name="channel"></param>
        public bool IsChannelMuted(CHANNEL channel) => getChannel[channel].IsMuted;

        /// <summary>
        /// Toggles whether a channel is muted.
        /// </summary>
        /// <param name="channel">The channel to (un)mute</param>
        public void ToggleMute(CHANNEL channel) => getChannel[channel].SetMuted(!getChannel[channel].IsMuted);

        /// <summary>
        /// Plays a SFX clip
        /// </summary>
        /// <param name="clip">The clip to play</param>
        /// <param name="onFinish"> the callback on finishing playing</param>
        public void PlayAudioClip(SFX clip, UnityAction onFinish = null, int index = -1, float delay = 0, float pitch = 1)
        {
            if(!getSFXCLIP.ContainsKey(clip))
                Awake();
            getChannel[CHANNEL.SFX].PlayClip(getSFXCLIP[clip].GetClip(index), onFinish, delay, volumeScale: getSFXCLIP[clip].Volume, pitch: pitch);
        }

        /// <summary>
        /// Sets the volume of a channel, if left to nothing, sets all channels.
        /// </summary>
        /// <param name="volume"> The volume to set the channel to.</param>
        /// <param name="channel"> The channel to channel</param>
        /// <param name="fadetime"> Time to fade out</param>
        public void SetVolume(float volume, CHANNEL channel, float fadetime = 0)
        {
            getChannel[channel].SetVolume(volume, fadetime);
            //If you turn down music, you want background stuff off aswell.
            /*if (channel == CHANNEL.MUSIC)
                getChannel[CHANNEL.BACKGROUND_SOUNDS].SetVolume(volume, fadetime);*/
        }

        /// <summary>
        /// Set all channels to this volume.
        /// </summary>
        /// <param name="volume"> the volume to set the channel at</param>
        /// <param name="fadetime"> Time to fade out</param>
        public void SetVolume(float volume, float fadetime = 0)
        {
            foreach (AudioChannel audioChannel in getChannel.Values)
                audioChannel.SetVolume(volume, fadetime);
        }

        /// <summary>
        /// Get a channels current volume.
        /// </summary>
        /// <param name="channel">The desired channel</param>
        /// <returns>its volume</returns>
        public float Volume(CHANNEL channel) => getChannel[channel].Volume;


        /// <summary>
        /// Mute a specific channel
        /// </summary>
        /// <param name="mute">to Mute or unMute</param>
        /// <param name="channel"></param>
        public void Mute(bool mute, CHANNEL channel) => getChannel[channel].SetMuted(mute);

        /// <summary>
        /// Mute all channels.
        /// </summary>
        /// <param name="mute"></param>
        public void Mute(bool mute)
        {
            foreach (AudioChannel audioChannel in getChannel.Values)
                audioChannel.SetMuted(mute);
        }

        /// <summary>
        /// Get if channel is playing anything.
        /// </summary>
        /// <param name="channel"></param>
        public bool IsPlaying(CHANNEL channel) => getChannel[channel].IsPlaying;

        /// <summary>
        /// Get is any channel is playing anything.
        /// (They can be playing but muted or 0 volume!)
        /// </summary>
        /// <returns>if any channels are</returns>
        public bool IsPlaying()
        {
            bool playing = false;
            foreach (AudioChannel audioChannel in getChannel.Values)
                playing = playing || audioChannel.IsPlaying;
            return playing;
        }

        /// <summary>
        /// Stop all clips playing on a single channel.
        /// </summary>
        public void StopPlaying(CHANNEL channel, float fadeTime = 0f) => getChannel[channel].StopAudio(fadeTime);

        /// <summary>
        /// Stop all clips playing on all channels.
        /// </summary>
        public void StopPlaying(float fadeTime = 0f)
        {
            foreach (AudioChannel audioChannel in getChannel.Values)
                audioChannel.StopAudio(fadeTime);
        }

        /// <summary>
        /// Sets the mixers volume
        /// (if you're not changing due to user input, you probably want to use SetVolume())
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        public void SetMasterVolume(CHANNEL channel, float value) => masterAudioMixer.SetFloat(channelToOutput[channel], value);

        /// <summary>
        /// Mutes the master audio mixer.
        /// WARNING, this will turn everything off till you run it again.
        /// (Unless this is settings, you probably want setMute or toggleMute())
        /// </summary>
        /// <param name="muted">whether we are currently muted</param>
        public void ToggleMasterMute(bool muted) => masterAudioMixer.SetFloat("Master", muted ? -80f : 0.0f);


        /// <summary>
        /// Gets the audio a channel currently has attatched to it.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public AudioSource GetAudioSource(CHANNEL channel)
        {
            return getChannel[channel].Source;
        }

        #endregion
        #region Private

        /// <summary>
        /// Creates dictionaries for quick lookup of sound effects by Enum.
        /// </summary>
        private void CreateDictionaries()
        {
            foreach (AudioAsset clipData in clips)
                switch (clipData.channel)
                {
                    case CHANNEL.SFX:
                        getSFXCLIP.Add(clipData.sfxClip, clipData);
                        break;
                    case CHANNEL.MUSIC:
                        getMusicClip.Add(clipData.musicalClip, clipData);
                        break;
                }
        }

        /// <summary>
        /// Adds the channels to the get channel dictionary
        /// </summary>
        private void CreateChannels()
        {
            foreach (ChannelData channelData in AudioChannels)
                getChannel.Add(channelData.type, channelData.controller);
        }

        /// <summary>
        /// Verifies all clips are added, if not display an DebugLog of missing Audio.
        /// Only run in editor.
        /// </summary>
        private void ClipVerification()
        {
            string intro = "Missing Audio Clips: \n";
            StringBuilder sb = new StringBuilder();
            sb.Append(intro);
            foreach (SFX values in Enum.GetValues(typeof(SFX)))
                if (!getSFXCLIP.ContainsKey(values)) sb.Append(values + ", ");
            foreach (MUSIC values in Enum.GetValues(typeof(MUSIC)))
                if (!getMusicClip.ContainsKey(values)) sb.Append(values + ", ");
            string result = sb.ToString();
            if (!result.Equals(intro)) Debug.Log(result);

            sb.Clear();
            intro = "Added But no audio file chosen: ";
            sb.Append(intro);
            foreach (AudioAsset clip in clips)
                if (clip.NumberOfClips == 0) sb.Append(clip.name + ", ");
            result = sb.ToString();
            if (result != intro) Debug.Log(result);
        }

        #endregion
        #endregion
    }
}
