using com.romainimberti.ggj2021.game;
using com.romainimberti.ggj2021.ui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        #endregion
        #region Public

        #endregion
        #region Private

        #endregion
        #endregion
        #region Methods
        #region Unity

        private void OnEnable()
        {
            btn_play.Init(Play);
        }

        #endregion
        #region Public

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
