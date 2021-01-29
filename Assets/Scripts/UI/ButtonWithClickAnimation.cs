using com.romainimberti.ggj2021.utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.romainimberti.ggj2021.ui
{

    ///<summary>
    ///Class that handles the button click effect animation
    ///</summary>
    [RequireComponent(typeof(Button))]
	public class ButtonWithClickAnimation : MonoBehaviour
	{
        #region Variables
        #region Editor

        [SerializeField]
        private List<Graphic> graphics;

        #endregion
        #region Private

        private LTDescr selectAnimation = null;

        private Button btn;

        private System.Action callback;
        private System.Func<bool> condition;

        private bool on = true;

        #endregion
        #region Public

        public Button Button
        {
            get
            {
                if(btn == null)
                    btn = GetComponent<Button>();
                return btn;
            }
        }

        public bool Interactable
        {
            set
            {
                on = value;
                Button.interactable = value;
                UpdateGraphics();
            }
        }

        public bool On
        {
            set
            {
                on = value;
                UpdateGraphics();
            }
        }
        
        #endregion
        #endregion
        #region Methods
        #region Unity

        private void OnEnable()
        {
            Button.onClick.AddListener(PlayClickEffect);
        }

        private void OnDisable()
        {
            Button.onClick.RemoveListener(PlayClickEffect);
        }

        #endregion
        #region Public

        public void Init(System.Action callback, System.Func<bool> condition=null)
        {
            this.callback = callback;
            this.condition = condition != null ? condition : () => true;
        }

        #endregion
        #region Private

        private void PlayClickEffect()
        {
            if(selectAnimation != null || condition == null)
                return;

            bool canClick = condition.Invoke();
            if(!canClick)
                return;

            AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Click);
            selectAnimation = LeanTween.scale(gameObject, Vector2.one * 0.95f, 0.05f).setEaseOutQuad().setOnComplete(() =>
            {
                if(selectAnimation != null && selectAnimation.passed == selectAnimation.time)
                {
                    callback?.Invoke();
                    selectAnimation = null;
                }
            }).setIgnoreTimeScale(true).setLoopPingPong(1).setOnCompleteOnRepeat(true);
        }

        private void UpdateGraphics()
        {
            foreach(Graphic g in graphics)
            {
                g.CrossFadeColor(on ? Button.colors.normalColor : Button.colors.disabledColor, 0f, true, true);
            }
        }

        #endregion
        #endregion
    }
}
