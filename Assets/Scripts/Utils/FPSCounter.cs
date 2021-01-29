using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace com.romainimberti.ggj2021.ui
{
	public class FPSCounter : MonoBehaviour
	{
        #region Variables

        #region Editor

        [SerializeField]
        private TextMeshProUGUI txt_fps;

        #endregion
        #region Private

        private int avgFrameRate;

        private int[] previousFrameRate;

        private int pointer;

        #endregion
        #region Public

        #endregion

        #endregion
        #region Methods

        #region Unity

        private void Start()
        {
            if(txt_fps == null)
                Destroy(gameObject);
            previousFrameRate = new int[10];
        }

        private void Update()
        {
            float current = (1f / Time.unscaledDeltaTime);
            previousFrameRate[pointer] = (int)current;
            pointer = (pointer + 1) % previousFrameRate.Length;
            avgFrameRate = (int)previousFrameRate.Average();
            txt_fps.text = avgFrameRate.ToString() + " FPS";
        }

        #endregion
        #region Public

        #endregion
        #region Private

        #endregion

        #endregion
    }
}