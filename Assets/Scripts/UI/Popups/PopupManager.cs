using com.romainimberti.ggj2021.utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.romainimberti.ggj2021.ui.popups
{

    /// <summary>
    /// PopupManager is a class that Creates the popups and manages the popup dim
    /// The popupmanager should have no other purpose than to spawn the popups, other behaviour should be handled in specific popup class methods
    /// </summary>
    public class PopupManager : SingletonBehaviour<PopupManager>
    {
        #region Consts
        /// <summary>
        /// The LeanTween animation time
        /// </summary>
        public const float AnimTime = 0.35f;
        /// <summary>
        /// The animation time for the dim to appear / disappear
        /// </summary>
        public const float DimTime = 0.2f;

        /// <summary>
        /// The duration to wait on top of the <see cref="AnimTime"/> before the dim is interactable
        /// </summary>
        public const float DimInteractionDelay = 0.05f;
        #endregion

        #region Internal
        /// <summary>
        /// the internal datatype for serializing the dictionary 
        /// used to link the type of popup to the popup prefab
        /// </summary>
        [Serializable]
        public struct Data
        {
            //test
            /// <summary>
            /// the type of the popup
            /// </summary>
            public SerializableSystemType type;
            /// <summary>
            /// the prefb of the popup
            /// </summary>
            public GameObject prefab;
            /// <summary>
            /// override to make distinct work
            /// </summary>
            /// <returns>The hashCode of the data struct</returns>
            public override int GetHashCode()
            {
                if(type == null || type.Name == null || type.SystemType == null)
                    return -1;
                return type.SystemType.GetHashCode();
            }
        }
        /// <summary>
        /// object for remembering where the highlighted objects where
        /// </summary>
        private struct History
        {
            /// <summary>
            /// the gameobject
            /// </summary>
            public GameObject GameObject;
            /// <summary>
            /// the parent
            /// </summary>
            public GameObject Parent;
            /// <summary>
            /// the order it is in the parent 
            /// </summary>
            public int Order;
            /// <summary>
            /// if the sorting is overridden?
            /// </summary>
            public bool OverRideSorting;

        }
        #endregion

        #region Variables
        #region Public 

        public PopupElement CurrentPopup { get; private set; }
        public PopupElement PreviousPopup { get; private set; }

        /// <summary>
        /// check for if anny popup is active
        /// </summary>
        public bool IsPopupActive { get { return activePopups > 0; } }

        /// <summary>
        /// The close panel
        /// </summary>
        public Transform ClosePanel => closePanelButton.transform;

        /// <summary>
        /// Time since we have undimmed
        /// </summary>
        public float TimeSinceDimmed;
        #endregion
        /// <summary>
        /// the list of editor data for the popups
        /// </summary>
        [SerializeField]
        private List<Data> PopupsEditor;
        /// <summary>
        /// Delegate calling whether the popup is beling show or not
        /// </summary>
        public delegate void PopupActivated(bool active);
        ///<summary>
        /// Event called when popups are shown or hidden.
        /// </summary>
        public static event PopupActivated popupStatusChange;
        /// <summary>
        /// the dictionary containing all the popups and there types
        /// </summary>
        private Dictionary<Type, GameObject> Popups = new Dictionary<Type, GameObject>();
        /// <summary>
        /// the queue off the popup so we dont show two popups at once
        /// </summary>
        private Queue<PopupElement> PopupQueue = new Queue<PopupElement>();
        /// <summary>
        /// The panel to close the current popup
        /// </summary>
        [SerializeField]
        [Tooltip("the close panel (dimmer) prefab")]
        private GameObject pn_closePanelPrefab;
        /// <summary>
        /// the main canvas of the popupmanager
        /// </summary>
        [SerializeField]
        [Tooltip("the main canvas of the popupmanager")]
        private Canvas Canvas;
        /// <summary>
        /// The number of currently active popups
        /// </summary>
        private int activePopups;
        /// <summary>
        /// The panel to close the current popup
        /// </summary>
        private GameObject pn_closePanel;
        /// <summary>
        /// The image component of the pn_closePanel
        /// </summary>
        private Image img_closePanelImage;
        /// <summary>
        /// The button to close the pop up
        /// </summary>
        private Button closePanelButton;
        /// <summary>
        /// the elements to highlight
        /// </summary>
        private List<History> elements = new List<History>();

        #endregion

        #region Methods
        #region Unity
        /// <summary>
        /// initilizes the data and spawns the dimmer
        /// </summary>
        private void Start()
        {
            pn_closePanel = Instantiate(pn_closePanelPrefab, transform);
            pn_closePanel.SetActive(false);

            closePanelButton = pn_closePanel.GetComponent<Button>();
            img_closePanelImage = pn_closePanel.GetComponent<Image>();

            for(int i = 0;i < PopupsEditor.Count;i++)
            {
                if(PopupsEditor[i].prefab != null)
                    Popups.Add(PopupsEditor[i].type.SystemType, PopupsEditor[i].prefab);
            }
        }
        #endregion
        #region Public
        /// <summary>
        /// Create a popup of given type that inherits <see cref="PopupElementV3"/>
        /// </summary>
        /// <typeparam name="T">The Type of popup to create</typeparam>
        /// <param name="canSpawn">Action that returns the <see cref="PopupElementV3"/> only if popup was succesfully spawned this action is invoked</param>
        /// <param name="hideOnDimAction">Whether to hide the popup when the user clicks anywhere outside the popup</param>
        public void CreatePopup<T>(Action<PopupElement> canSpawn = null, bool hideOnDimAction = true) where T : PopupElement
        {
            CreatePopup(typeof(T), canSpawn, hideOnDimAction);
        }

        /// <summary>
        /// Create a popup of given type that inherits <see cref="PopupElementV3"/>
        /// </summary>
        /// <param name="T">The Type of popup to create</param>
        /// <param name="canSpawn">Action that returns the <see cref="PopupElementV3"/> only if popup was succesfully spawned this action is invoked</param>
        /// <param name="hideOnDimAction">Whether to hide the popup when the user clicks anywhere outside the popup</param>
        public void CreatePopup(Type T, Action<PopupElement> canSpawn = null, bool hideOnDimAction = true)
        {
#if UNITY_EDITOR
            if(!Popups.ContainsKey(T))
            {
                Debug.LogError(string.Concat("You forgot to add the prefab for the ", T.Name, " to the popupmanager."));
                return;
            }
#endif 

            Popups[T].SetActive(false);
            PopupElement popup = (PopupElement)Instantiate(Popups[T], transform).GetComponent(T);

            // Check whether we can spawn the popup using it's own given condition
            if(!popup.CanSpawn(canSpawn))
                return;

            // If the popup spawned setup popupmanagers side
            activePopups++;
            if(activePopups > 1)
                PopupQueue.Enqueue(popup);
            else
            {
                popupStatusChange?.Invoke(true);
                popup.gameObject.SetActive(true);
                CoroutineManager.Instance.WaitForEndOfFrame(() => popup.Show());
                ShowDimmer(popup, hideOnDimAction);
                CurrentPopup = popup;
            }

            // Add hide popup method for closing the dim
            popup.CancelEvent += () => Closepopup(popup);
            popup.DismissEvent += () => Closepopup(popup);
            popup.SucceedEvent += () => Closepopup(popup);

            // sets up the popups side
            canSpawn?.Invoke(popup);
        }


        /// <summary>
        /// closes the currently active popup and shows the next one in line if there is one
        /// </summary>
        public void Closepopup(PopupElement previousPopup)
        {
            activePopups--;
            PreviousPopup = previousPopup;

            if(PopupQueue.Count != 0)
            {
                PopupElement popup = PopupQueue.Dequeue();
                popup.gameObject.SetActive(true);
                CoroutineManager.Instance.WaitForEndOfFrame(() => {
                    CoroutineManager.Instance.Wait(0.2f, () => popup.Show());
                });

                StartCoroutine(SetCancelListener(popup));
            }

            if(activePopups <= 0)
            {
                HideDimmer();
                activePopups = 0;
                popupStatusChange?.Invoke(false);
            }
        }
        #endregion

        #region Private
        /// <summary>
        /// shows the dimmer
        /// </summary>
        /// <param name="popup">The popup we are attaching the dimmer to</param>
        /// <param name="hideOnDimAction">Whether to hide the popup when the user clicks anywhere outside the popup</param>
        private void ShowDimmer(PopupElement popup, bool hideOnDimAction)
        {
            pn_closePanel.SetActive(true);
            LeanTween.cancel(img_closePanelImage.gameObject, false);
            LeanTween.value(img_closePanelImage.gameObject, img_closePanelImage.color.a, 0.7f, DimTime).setIgnoreTimeScale(true)
                .setOnUpdate(x => img_closePanelImage.color = new Color(img_closePanelImage.color.r, img_closePanelImage.color.g, img_closePanelImage.color.b, x))
                .setOnComplete(() =>
                {
                    if(hideOnDimAction)
                        StartCoroutine(SetCancelListener(popup));
                });
        }

        /// <summary>
        /// hides the dimmer
        /// </summary>
        private void HideDimmer()
        {
            closePanelButton.interactable = false;
            LeanTween.cancel(img_closePanelImage.gameObject);
            LeanTween.value(img_closePanelImage.gameObject, 0.7f, 0.0f, DimTime).setIgnoreTimeScale(true).setOnUpdate(x => img_closePanelImage.color = new Color(img_closePanelImage.color.r, img_closePanelImage.color.g, img_closePanelImage.color.b, x))
                .setOnComplete(() =>
                {
                    TimeSinceDimmed = Time.time;
                    pn_closePanel.SetActive(false);
                });
        }

        /// <summary>
        /// adds the event to the close panel 
        /// </summary>
        /// <param name="popup">the popup on wich the event is triggered</param>
        private IEnumerator SetCancelListener(PopupElement popup)
        {
            if(!popup.DimIsInteractable)
                yield break;
            
            yield return new WaitForSecondsRealtime(AnimTime + DimInteractionDelay);
            closePanelButton.onClick.RemoveAllListeners();
            closePanelButton.onClick.AddListener(popup.ExecuteCancel);
            closePanelButton.interactable = true;
        }
        #endregion

        #region hilighting

        /// <summary>
        /// shows the given elements
        /// </summary>
        /// <param name="elements"> the given elements</param>
        public void ShowElements(List<GameObject> elements)
        {
            if(elements == null) return;

            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.renderMode = RenderMode.WorldSpace;

            for(int i = 0;i < elements.Count;i++)
            {
                if(elements[i].Equals(null)) continue;

                Canvas canvas = elements[i].GetComponent<Canvas>();
                bool sorting = false;
                if(canvas != null)
                    if(canvas.overrideSorting)
                    {
                        sorting = true;
                        canvas.overrideSorting = false;
                    }

                this.elements.Add(new History
                {
                    GameObject = elements[i],
                    Order = elements[i].transform.GetSiblingIndex(),
                    Parent = elements[i].transform.parent.gameObject,
                    OverRideSorting = sorting
                });

                elements[i].transform.SetParent(pn_closePanel.transform, true);

            }
        }

        /// <summary>
        /// hides the elements that are aboave the dim
        /// </summary>
        public void HideElements()
        {
            for(int i = 0;i < elements.Count;i++)
            {
                if(elements[i].GameObject != null)
                {
                    if(elements[i].GameObject.transform.parent == pn_closePanel.transform)
                    {
                        elements[i].GameObject.transform.SetParent(elements[i].Parent.transform, true);
                        elements[i].GameObject.transform.SetSiblingIndex(elements[i].Order);
                    }

                    if(elements[i].OverRideSorting)
                    {
                        Canvas canvas = elements[i].GameObject.GetComponent<Canvas>();
                        canvas.overrideSorting = true;
                    }
                }
            }
            elements.Clear();

            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        /// <summary> 
        /// simulating a popup opening / closing for classes that use popup which aren't created by the popupmanager
        /// but still need the event
        /// This is a hotfix, there should be a better way to make sure popups not created here should have this event
        /// <param name="toggle"> true = open | false = closed </param>
        /// </summary>
        public void SimulatePopupToggleEvent(bool toggle) => popupStatusChange?.Invoke(toggle);
        #endregion
        #endregion
    }
}
