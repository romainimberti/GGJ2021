using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace com.romainimberti.ggj2021.utilities.ui
{

    ///<summary>
    /// A Utils class that handles the equalisation of provided texts
    /// Using the script it can be completely handled by inspector or setup in code using below methods
    ///</summary>
    public class TextSizeEqualiser : MonoBehaviour
	{
        #region Variables
        #region Editor
        /// <summary>
        /// Whether to equalise the text on start
        /// </summary>
        [Tooltip("Whether to equalise the text on start")]
        [SerializeField]
        private bool runOnStart = true;

        /// <summary>
        /// If a CanvasGroup is assigned it will update the text everytime it turns from invisble
        /// to visible state
        /// </summary>
        [Tooltip("Whether to equalise the text when it turns visible")]
        [SerializeField]
        private CanvasGroup useCanvasVisible;


        /// <summary>
        /// If we should skip disabled elements
        /// </summary>
        [SerializeField]
        private bool skipDisabled = false;

        /// <summary>
        /// Whether to run the equalise for all <see cref="TextMeshProUGUI"/> in children
        /// </summary>
        [Tooltip("Whether to run the equalise for all TextMeshProUGUI in children")]
        [SerializeField]
        private bool runOnChildren = false;


        /// <summary>
        /// Optional, specifically define which <see cref="TextMeshProUGUI"/> components should be equalised with eachother
        /// </summary>
        [SerializeField]
        [Tooltip("Optional: specifically define which TextMeshProUGUI components should be equalised with eachother")]
        private List<TextMeshProUGUI> textsToEqualise;
        #endregion
        #region Public
        public float CurrentFontSize => lastEqualisedFontSize;
        #endregion
        #region Private
        /// <summary>
        /// The last fontsize we equalised to
        /// </summary>
        private float lastEqualisedFontSize;
        /// <summary>
        /// Whether we where invisible the last time we checked
        /// </summary>
        private bool wasInvisible = true;
        /// <summary>
        /// Reuse a single List to prevent memory allocation
        /// </summary>
        private List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
        #endregion
        #endregion
        #region Methods
        #region Unity
        /// <summary>
        /// If we have runOnStart enabled equalise the text
        /// </summary>
        private void Start()
        {
            if (runOnStart)           
                    EqualiseText();
        }

        /// <summary>
        /// On update we check if we have a canvasgroup assigned
        /// If we do and it was not visible the last time we update the text size
        /// </summary>
        private void Update()
        {
            if (useCanvasVisible != null)
            {
                if (useCanvasVisible.alpha == 0)
                    wasInvisible = true;
                else if (wasInvisible)
                {
                    EqualiseText();
                    wasInvisible = false;
                }
            }
        }
        #endregion
        #region Public
        /// <summary>
        /// Equalise the text to the same fontsize (smallest out of all provided texts)
        /// </summary>
        public void EqualiseText()
        {
            CoroutineManager.Instance.WaitForEndOfFrame(() =>
            {
                UpdateTextList();
                GetCalculatedTextSize((s) => SetTextSizes(s));
            });
        }

        /// <summary>
        /// Equalise the text to the same fontsize
        /// <param name="textSize"/>The fontsize to set for all texts</param>
        /// </summary>
        public void EqualiseText(float textSize)
        {
            CoroutineManager.Instance.WaitForEndOfFrame(() =>
            {
                UpdateTextList();
                SetTextSizes(textSize);
            });
        }

        /// <summary>
        /// Override the current array of texts with provided text array
        /// </summary>
        /// <param name="texts">The TextMeshProUGUI array to resize</param>
        public void UpdateTexts(TextMeshProUGUI[] texts)
        {
            this.texts.Clear();
            this.texts.AddRange(texts);

                CoroutineManager.Instance.WaitForEndOfFrame(() =>
                {
                    GetCalculatedTextSize((s) => SetTextSizes(s));
                });
        }

        /// <summary>
        /// Add a <see cref="TextMeshProUGUI"/> at runtime to update everytime equalise is called
        /// </summary>
        /// <param name="text">The text component to equalise</param>
        public void AddText(TextMeshProUGUI text)
        {
            textsToEqualise.Add(text);
        }
        /// <summary>
        /// Add an array of <see cref="TextMeshProUGUI"/> at runtime to update everytime equalise is called
        /// </summary>
        /// <param name="textArray">The texts to equalise</param>
        public void AddText(TextMeshProUGUI[] textArray)
        {
            textsToEqualise.AddRange(textArray);
        }

        #endregion

        #region Private
        /// <summary>
        /// Refreshes the list of texts and makes sure there is no copies of the same Text component
        /// </summary>
        private void UpdateTextList()
        {
            texts.Clear();
            texts.AddRange(textsToEqualise);

            if (runOnChildren)
                texts.AddRange(GetComponentsInChildren<TextMeshProUGUI>());

            texts = texts.Distinct().ToList();
        }

        /// <summary>
        /// Calculates the size of the smallest text and returns this value
        /// </summary>
        /// <returns>The smallest fontsize out of all currently texts evaluated for equalisation</returns>
        private void GetCalculatedTextSize(System.Action<float> result)
        {
            float size = float.MaxValue;
            int completed = 0;
            foreach (TextMeshProUGUI text in texts)
            {
                if (text == null)
                {
                    Debug.LogError(string.Format("Text in TextSizeEqualiser: element[{0}] is null in {1}", texts.IndexOf(text), gameObject.name));
                    completed++;
                    continue;
                }

                if (!text.gameObject.activeInHierarchy && skipDisabled)
                {
                    completed++;
                    continue;
                }

                // We set auto sizing true so that we get the correct value for calculation
                text.enableAutoSizing = true;
                CoroutineManager.Instance.WaitForEndOfFrame(() =>
                {
                    if (text.fontSize < size)
                        size = text.fontSize;
                    completed++;
                });
            }
            CoroutineManager.Instance.WaitUntil(() => completed >= texts.Count, () =>
            {
                result?.Invoke(size);
            });
        }

        /// <summary>
        /// Sets the textSize for all currently evaluated texts
        /// </summary>
        /// <param name="textSize">The textsize to set the texts to</param>
        private void SetTextSizes(float textSize)
        {
            lastEqualisedFontSize = textSize;
            foreach (TextMeshProUGUI text in texts)
            {
                if (!text)
                    continue;

                // We set autosizing false so that the texts stays the provided value
                text.enableAutoSizing = false;
                text.fontSize = textSize;
            }
        }
        #endregion
        #endregion
    }
}
