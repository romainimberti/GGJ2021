using nl.DTT.Utils.Enums;
using nl.DTT.Utils.Extensions.Enums;
using TMPro;
using UnityEngine;

namespace nl.DTT.LocalizedLanguage.Texts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPUITranslatedText : AbstractTranslatedText
    {
        #region Variables
        /// <summary>
        /// Text-Component
        /// </summary>
        private TextMeshProUGUI textComponent;
        #endregion

        #region Methods
        /// <summary>
        /// Grabs Text-Component from GameObject
        /// </summary>
        protected override void Awake()
        {
            LoggingLevel.Development.Log(string.Format("TMPUITranslatedText[{0}]: Grabbing Text Object", name),
                logLevel, this, logMethods);
            textComponent = GetComponent<TextMeshProUGUI>();
            LoggingLevel.Debug.Log(
                string.Format("TMPUITranslatedText[{0}]: Loaded TextObject: {1}", name, textComponent != null),
                logLevel, textComponent, logMethods);
            base.Awake();
        }

        /// <summary>
        /// Text as currently displayed
        /// </summary>
        /// <returns>Text as currently displayed by TranslatedText (includes Formatting)</returns>
        public override string GetText()
        {
            if (textComponent == null)
                Awake();
            return textComponent.text;
        }

        /// <summary>
        /// Sets Text to GUI-Component
        /// </summary>
        /// <param name="textToSet">Text to display</param>
        protected override void SetTextToComponent(string textToSet)
        {
            if (textComponent == null)
                Awake();
            textComponent.text = ApplyTextRules(textToSet);   
        }
        #endregion
    }
}