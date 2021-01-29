using UnityEngine;
using System;
using System.Collections;

namespace com.romainimberti.ggj2021.utilities.audio
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
        AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionalHideAttribute : PropertyAttribute
    {
        public string ConditionalSourceField = "";
        public bool HideInInspector = false;
        public AudioManager.CHANNEL DesiredChannel;

        /// <summary>
        /// Holds required values to determine if a property should be hidden.
        /// </summary>
        /// <param name="conditionalSourceField"></param>
        /// <param name="desiredChannel"></param>
        public ConditionalHideAttribute(string conditionalSourceField, AudioManager.CHANNEL desiredChannel)
        {
            this.DesiredChannel = desiredChannel;
            this.ConditionalSourceField = conditionalSourceField;
        }

        public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector)
        {
            this.ConditionalSourceField = conditionalSourceField;
            this.HideInInspector = hideInInspector;
        }
    } 

}