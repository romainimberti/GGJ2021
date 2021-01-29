using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace com.romainimberti.ggj2021.utilities.audio
{
    /// <summary>
    /// Audio chanels play are given audioclips to play.
    /// They then handle this playing depending on their internal characterstics.
    /// </summary>
    public class AudioChannel : MonoBehaviour
    {
        #region Variables
        #region Public


        /// <summary>
        /// Get if mute is currently set.
        /// </summary>
        /// <returns></returns>
        public bool IsMuted => audioSource.mute;

        /// <summary>
        /// Returns channel volume
        /// </summary>
        public float Volume { get { return audioSource.volume; } }

        /// <summary>
        /// The last loaded audio clip.
        /// </summary>
        public AudioClip loadedClip { get; private set; }

        /// <summary>
        /// The time when all clips will be finished playing.
        /// </summary>
        private DateTime lastClipFinished;

        /// <summary>
        /// Returns the attatched audiosource.
        /// </summary>
        public AudioSource Source { get { return audioSource; } }

        /// <summary>
        /// Returns if any clips are still playing.
        /// </summary>
        public bool IsPlaying => DateTime.Now.Ticks < lastClipFinished.Ticks;

        #endregion
        #region Private
        /// <summary>
        /// name of the audio channel.
        /// Not needed just useful for Editor
        /// </summary>
        [SerializeField]
        [Tooltip("The name")]
        private string name;

        /// <summary>
        /// Whether the audio channel can play multiple clips at once.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether the channel can play multiple clips at once")]
        private bool single;
        /// <summary>
        /// Whether the audio channel repeats given clips or not.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether audio repeats on finish")]
        private bool repeating;


        /// <summary>
        /// The audioSource this channel is to play clips through (should be unique for each channel).
        /// </summary>
        [SerializeField]
        [Tooltip("The audio source used to play")]
        private AudioSource audioSource;


        private Coroutine fadeRoutine;
        #endregion
        #endregion
        #region Methods
        #region Unity

        /// <summary>
        /// Sets the audio source to repeat clips or not.
        /// </summary>
        private void Awake() => audioSource.loop = repeating;

        #endregion
        #region Public

        /// <summary>
        /// (un)mutes the channel.
        /// </summary>
        /// <param name="toggle"></param>
        public void SetMuted(bool toggle) => audioSource.mute = toggle;



        /// <summary>
        /// Play an audio clip on this channel.
        /// </summary>
        /// <param name="clip">The clip to play, if null, play the previous one again</param>
        /// <param name="onFinish">Action to be called once the audio finishes playing</param>
        /// <param name="delay">The delay before clip plays</param>
        /// <param name="pitch">The pitch of the clip</param>
        /// <param name="volumeScale">Clip specific volume scale (between 0 and 1)</param>
        public void PlayClip(AudioClip clip = null, UnityAction onFinish = null, float delay = 0f, float pitch = 1f,float volumeScale = 1f)
        {
            if (clip == null && loadedClip == null)
                return;
            else if (clip == null)
                clip = loadedClip;

            if (delay > 0)
            {
                StartCoroutine(PlayDelayed(clip, onFinish, delay, pitch));
                return;
            }
            //Stop others playing if only one allowed.
            if (single && !repeating)
                audioSource.Stop();

            audioSource.pitch = pitch;
            //If repeating, get it to play forever.
            if (repeating)
            {
                //So we don't have to start from the beginning every time.
                if (audioSource.clip == clip && IsPlaying)
                    return;
                audioSource.clip = clip;
                audioSource.Play();
                lastClipFinished = DateTime.MaxValue;
            }
            //If not, play a oneshot.
            else
            {
                audioSource.PlayOneShot(clip,volumeScale);
                lastClipFinished = DateTime.Now.AddSeconds(clip.length) > lastClipFinished ? DateTime.Now.AddSeconds(clip.length) : lastClipFinished;
                StartCoroutine(CallOnFinish(clip, onFinish));
            }
            loadedClip = clip;
        }



        /// <summary>
        /// Stops all audio.
        /// </summary>
        public void StopAudio(float fadeTime = 0)
        {
            float previousVolume = Volume;

            if (fadeTime > 0)
            {
                SetVolume(0, fadeTime);
                StartCoroutine(DelayFunction(fadeTime, StopAudio, previousVolume));
                return;
            }
            audioSource.Stop();
            lastClipFinished = DateTime.Now.AddSeconds(-1f);
        }

        /// <summary>
        /// Sets the volume of the channel.
        /// </summary>
        /// <param name="newVolume">The volume to set the channel to</param>
        /// <param name="fadeTime">Time to get from current volume to new volume</param>
        public void SetVolume(float newVolume, float fadeTime = 0)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);
            if (fadeTime == 0)
                audioSource.volume = newVolume;
            else
                fadeRoutine = StartCoroutine(FadeVolume(audioSource.volume, newVolume, fadeTime));
        }



        #endregion
        #region Private

        /// <summary>
        /// Play a clip after a given delay.
        /// </summary>
        /// <param name="clip">Clip to play</param>
        /// <param name="onFinish">action to call on finish</param>
        /// <param name="delay">time to wait before playing</param>
        /// <param name="pitch">The pitch of the clip, 1 is the clip's default</param>
        /// <returns></returns>
        private IEnumerator PlayDelayed(AudioClip clip,UnityAction onFinish, float delay, float pitch)
        {
            yield return new WaitForSecondsRealtime(delay);
            PlayClip(clip, onFinish, pitch: pitch);
        }

        /// <summary>
        /// Waits for clip length, then calls the desired action.
        /// </summary>
        /// <param name="clip">The clip</param>
        /// <param name="onFinish">The UnityAction to call</param>
        /// <returns></returns>
        private IEnumerator CallOnFinish(AudioClip clip,UnityAction onFinish)
        {
            if (onFinish == null)
                yield break;
            yield return new WaitForSecondsRealtime(clip.length);
            onFinish?.Invoke();
        }

       
        /// <summary>
        /// Run the desired function after a certain time
        /// </summary>
        /// <param name="delay">time to wait</param>
        /// <param name="function">the function to run</param>
        /// <param name="volumeReset">Set volume back to original after audio is faded out</param>
        /// <returns></returns>
        private IEnumerator DelayFunction(float delay,UnityAction<float> function,float volumeReset)
        {
            yield return new WaitForSecondsRealtime(delay);
            function(0f);
            //Reset the volume so future sounds still play properly.
            yield return new WaitForSeconds(0.2f);
            audioSource.volume = volumeReset;
        }



        /// <summary>
        /// Implements fading volume over time.
        /// </summary>
        /// <param name="volume">Start Volume</param>
        /// <param name="newVolume">End Volume</param>
        /// <param name="fadeTime">Time to get from start to end</param>
        /// <returns></returns>
        private IEnumerator FadeVolume(float volume, float newVolume, float fadeTime)
        {
            float timePassed = 0;
            while (timePassed < fadeTime)
            {
                timePassed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(volume, newVolume, (timePassed / fadeTime));
                yield return null;
            }
            //Double check it got to the correct volume.
            audioSource.volume = newVolume;
        }
        #endregion
        #endregion
    }
}
