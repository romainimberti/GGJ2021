using com.romainimberti.ggj2021.game;
using com.romainimberti.ggj2021.ui;
using com.romainimberti.ggj2021.utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.romainimberti.ggj2020
{

	///<summary>
	///Class that handles the game over screen
	///</summary>
	public class GameOverUI : MonoBehaviour
	{
		#region Variables
		#region Editor

		[SerializeField]
		private ButtonWithClickAnimation btn_play;

        [SerializeField]
        private Image playerDeath;

        [SerializeField]
        public List<Sprite> playerDeathAnimation;

        [SerializeField]
        private GameObject gameOverText;

        #endregion
        #region Public

        #endregion
        #region Private

        #endregion
        #endregion
        #region Methods
        #region Unity

        private void Awake()
        {
            LeanTween.scale(gameOverText, Vector3.one * 1.03f, 2f).setEaseInOutQuad().setLoopPingPong(-1);
        }

        private void OnEnable()
        {
            btn_play.Init(Play);
        }

        #endregion
        #region Public

        public void SetFirstPlayerImage(Sprite sprite)
        {
            playerDeath.sprite = sprite;
        }

        public void PlayDeathAnimation()
        {
            CoroutineManager.Instance.Wait(0.25f, () =>
            {
                int clip = Random.Range(0, 4);
                switch (clip)
                {
                    case 0:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SamuraiDeath1);
                        break;
                    case 1:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SamuraiDeath2);
                        break;
                    case 2:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SamuraiDeath3);
                        break;
                    case 3:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SamuraiDeath4);
                        break;
                }
            });
            CoroutineManager.Instance.Wait(0.5f, () =>
            {
                playerDeath.sprite = playerDeathAnimation[0];
                CoroutineManager.Instance.Wait(0.5f, () => {
                    playerDeath.sprite = playerDeathAnimation[1];
                });
            });
        }

        #endregion
        #region Protected

        #endregion
        #region Private

        private void Play()
        {
            GameManager.Instance.GenerateMaze();
        }

        #endregion
        #endregion
    }
}
