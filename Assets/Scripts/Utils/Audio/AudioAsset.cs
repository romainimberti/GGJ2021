using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2021.utilities.audio
{

	///<summary>
	///Holds are audio files.
	///</summary>
    [CreateAssetMenu(fileName = "AudioAsset", menuName = "AudioAsset",order = 1)]
	public class AudioAsset : ScriptableObject
	{
        #region Variables
        #region Editor
        public string name;

        public AudioManager.CHANNEL channel;

        [ConditionalHide("channel",desiredChannel: AudioManager.CHANNEL.MUSIC, HideInInspector = true)]
        public AudioManager.MUSIC musicalClip;
        

        /// <summary>
        /// The sound effect clip enum.
        /// </summary>
        [ConditionalHide("channel", desiredChannel: AudioManager.CHANNEL.SFX, HideInInspector = true)]
        public AudioManager.SFX sfxClip;

        /// <summary>
        /// The sound effect clip enum.
        /// </summary>
        [ConditionalHide("channel", desiredChannel: AudioManager.CHANNEL.SECOND_MUSIC, HideInInspector = true)]
        public AudioManager.SECOND_MUSIC secondMusicClip;
        #endregion
        #region Public
        public AudioClip[] possibleClips;

        /// <summary>
        /// used for scaling audio down.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("This can be used to turn down non-music audio if it's too loud")]
        public float Volume = 1f;
           

        public int NumberOfClips => possibleClips.Length;


        public AudioClip GetClip(int index = -1)
        {
            if (index == -1)
                return possibleClips[UnityEngine.Random.Range(0, possibleClips.Length)];
            else
                return possibleClips[Mathf.Clamp(index, 0, possibleClips.Length - 1)];
        }
        #endregion
        #region Private

        #endregion
        #endregion
        #region Methods
        #region Unity

        #endregion
        #region Public

        #endregion
        #region Protected

        #endregion
        #region Private

        #endregion
        #endregion
    }
}
