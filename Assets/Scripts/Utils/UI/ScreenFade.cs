using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.romainimberti.ggj2021.utilities.ui
{

	///<summary>
	///Class that handles a screen fade
	///</summary>
	public class ScreenFade : SingletonBehaviour<ScreenFade>
	{
        #region Consts

        /// <summary>
        /// The fade in duration
        /// </summary>
        private const float FADE_IN_DURATION = 1f;

        /// <summary>
        /// The fade out duration
        /// </summary>
        private const float FADE_OUT_DURATION = 1f;

        #endregion
        #region Variables
        #region Public

        /// <summary>
        /// Is the scene hidden?
        /// </summary>
        public bool SceneHidden { get; private set; } = false;

        public bool Fading { get; private set; } = false;

        #endregion
        #region Private

        /// <summary>
        /// Reference to the image component
        /// </summary>
        private Image img;

        #endregion
        #endregion
        #region Methods
        #region Unity

        /// <summary>
        /// Gets components
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            img = GetComponent<Image>();
        }

        #endregion
        #region Public

        public void SetOn(bool on)
        {
            if(img == null)
                img = GetComponent<Image>();

            Color c = img.color;
            c.a = on ? 1 : 0;
            img.color = c;
            SceneHidden = on;
        }

        /// <summary>
        /// Fades in
        /// </summary>
        /// <param name="callback">Callback on complete</param>
        public void FadeIn(System.Action callback)
        {
            Fading = true;
            LeanTween.value(img.color.a, 1f, FADE_IN_DURATION).setOnUpdate((float f) =>
            {
                Color c = img.color;
                c.a = f;
                img.color = c;
            }).setIgnoreTimeScale(true).setOnComplete(() => {
                callback?.Invoke();
                SceneHidden = true;
                Fading = false;
            }).setEaseInQuad();
        }

        /// <summary>
        /// Fades out
        /// </summary>
        /// <param name="callback">Callback on complete</param>
        public void FadeOut(System.Action callback)
        {
            Fading = true;
            LeanTween.value(img.color.a, 0f, FADE_OUT_DURATION).setOnUpdate((float f) =>
            {
                Color c = img.color;
                c.a = f;
                img.color = c;
            }).setIgnoreTimeScale(true).setOnComplete(() => {
                callback?.Invoke();
                SceneHidden = false;
                Fading = false;
            }).setEaseInQuad();
        }

        #endregion
        #endregion
    }
}
