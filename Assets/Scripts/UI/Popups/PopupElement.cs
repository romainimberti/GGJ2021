using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2021.ui.popups
{
    /// <summary>
    /// Base class for popups
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupElement : MonoBehaviour
    {
        #region Variables
        #region Public
        /// <summary>
        /// the delegate for the events
        /// </summary>
        public delegate void eventDelegate();
        /// <summary>
        /// the event when cancel is send
        /// </summary>
        public event eventDelegate CancelEvent;
        /// <summary>
        /// the event when dismiss is send
        /// </summary>
        public event eventDelegate DismissEvent;
        /// <summary>
        /// the event when succeed is send
        /// </summary>
        public event eventDelegate SucceedEvent;

        /// <summary>
        /// Whether the dim is Interactable;
        /// </summary>
        public bool DimIsInteractable { get; protected set; } = true;

        /// <summary>
        /// Whether the popup is interactable
        /// </summary>
        public bool Interactable { get; protected set; } = true;

        /// <summary>
        /// Returns the amount of time that has passed since startTime was called
        /// </summary>
        public float TimePassed => Mathf.RoundToInt(Time.time - startTime);
        #endregion
        #region Protected
        /// <summary>
        /// Time at which the popup was visible to the user
        /// </summary>
        protected float startTime;
        #endregion
        #region Private
        /// <summary>
        /// the canvasgroup of the popup
        /// </summary>
        protected CanvasGroup CanvasGroup { private set; get; }
        /// <summary>
        /// the smallest the popups will become in animation
        /// </summary>
        protected static Vector3 SmallestSizeDuringAnimation { get; } = Vector3.one * 0.7f;
        /// <summary>
        /// the biggest the popups become in animation
        /// </summary>
        protected static Vector3 LargestSizeDuringAnimation { get; set; } = Vector3.one;
        /// <summary>
        /// The easetype used for the animate in animation
        /// Duration values can be changed in <see cref="PopupManagerV2"/>
        /// </summary>
        private static LeanTweenType AnimateInEase = LeanTweenType.easeOutBack;
        /// <summary>
        /// The easetype used for the animate out animation
        /// Duration values can be changed in <see cref="PopupManagerV2"/>
        /// </summary>
        private static LeanTweenType AnimateOutEase = LeanTweenType.easeInBack;

        private bool active = true;

        /// <summary>
        /// The show event
        /// </summary>
        protected event Action ShowEvent;
        #endregion
        #endregion

        #region Methodes
        #region Unity
        /// <summary>
        /// the init of the popupelement
        /// </summary>
        protected virtual void Awake()
        {
            CanvasGroup = gameObject.GetComponent<CanvasGroup>();
            CanvasGroup.alpha = 0;
            transform.localScale = SmallestSizeDuringAnimation;
        }

        /// <summary>
        /// remove button manager conditional block.
        /// </summary>
        private void OnDisable() => active = false;

        /// <summary>
        /// Remove button manager conditional block.
        /// </summary>
        private void OnDestroy() => active = false;
        #endregion
        #region Public
        /// <summary>
        /// cancels the popup
        /// </summary>
        public virtual void ExecuteCancel()
        {
            if (!Interactable)
                return;
            Interactable = false;
            CancelEvent?.Invoke();
            Destroy();
        }

        /// <summary>
        /// Define the condition of this popup before it can spawn
        /// Override to give a condition, default to always allowed to spawn
        /// 
        /// when spawing a popup here you should always retun false
        /// </summary>
        /// <param name="onSpawn"> Callback when popup succesfully spawned, required to transition data to other popups </param>
        /// <typeparam name="T">The type of popup to call class defined methods</typeparam>
        /// <returns>Whether the popup is allowed to spawn should be false when another popup is spawned</returns>
        public virtual bool CanSpawn(Action<PopupElement> onSpawn = null)
        {
            return true;
        }


        /// <summary>
        /// Dismisses the popup (needs to be executed when the popup isnt sucsessfull but also didn't cancel)
        /// </summary>
        public virtual void ExecuteDismiss()
        {
            if (!Interactable)
                return;

            Interactable = false;
            DismissEvent?.Invoke();
            Destroy();
        }

        /// <summary>
        /// lets the popup succeed
        /// </summary>
        public virtual void ExecuteSucceed()
        {
            if (!Interactable)
                return;

            Interactable = false;
            SucceedEvent?.Invoke();
            Destroy();
        }

        /// <summary>
        /// Invokes the <see cref="SucceedEvent"/>
        /// </summary>
        protected void ExecuteSucceedEvent()
        {
            SucceedEvent?.Invoke();
        }

        /// <summary>
        /// destroys the popup
        /// </summary>
        public void Destroy()
        {           
            LeanTween.scale(gameObject, SmallestSizeDuringAnimation, PopupManager.AnimTime).setEase(AnimateOutEase).setIgnoreTimeScale(true);
            LeanTween.alphaCanvas(CanvasGroup, 0, PopupManager.AnimTime).setOnComplete(() => Destroy(gameObject)).setIgnoreTimeScale(true);
        }

        /// <summary>
        /// shows the popup
        /// </summary>
        public virtual void Show()
        {
            startTime = Time.time;
            LeanTween.alphaCanvas(CanvasGroup, 1, PopupManager.AnimTime).setIgnoreTimeScale(true);
            LeanTween.scale(gameObject, LargestSizeDuringAnimation, PopupManager.AnimTime).setEase(AnimateInEase).setIgnoreTimeScale(true).setOnComplete(() => ShowEvent?.Invoke());
        }

        #endregion
        #endregion

    }
}
