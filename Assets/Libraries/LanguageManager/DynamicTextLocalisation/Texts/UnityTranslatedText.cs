using nl.DTT.Utils.Enums;
using nl.DTT.Utils.Extensions.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace nl.DTT.LocalizedLanguage.Texts
{
    [RequireComponent(typeof(Text))]
    public class UnityTranslatedText : AbstractTranslatedText
    {
        #region Variables
        /// <summary>
        /// Text-Component
        /// </summary>
        private Text textComponent;
        #endregion

        #region Methods
        /// <summary>
        /// Grabs Text-Component from GameObject
        /// </summary>
        protected override void Awake()
        {
            LoggingLevel.Development.Log(string.Format("UnityTranslatedText[{0}]: Grabbing Text Object", name),
                logLevel, this, logMethods);
            textComponent = GetComponent<Text>();
            LoggingLevel.Debug.Log(
                string.Format("UnityTranslatedText[{0}]: Loaded TextObject: {1}", name, textComponent != null),
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