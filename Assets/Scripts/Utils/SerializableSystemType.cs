// Simple helper class that allows you to serialize System.Type objects.
// Use it however you like, but crediting or even just contacting the author would be appreciated (Always 
// nice to see people using your stuff!)
//
// Written by Bryan Keiren (http://www.bryankeiren.com)

using UnityEngine;

namespace com.romainimberti.ggj2021.utilities
{
    [System.Serializable]
    public class SerializableSystemType
    {
        #region variables
        /// <summary>
        /// the name of the type
        /// </summary>
        [SerializeField]
        [Tooltip("the name of the type")]
        private string name = "";

        /// <summary>
        /// returns the name of the type
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// the full name of the type
        /// </summary>
        [SerializeField]
        [Tooltip("the full type name of the type")]
        private string assemblyQualifiedName = "";

        /// <summary>
        /// returns the full typename of the type
        /// </summary>
        public string AssemblyQualifiedName
        {
            get { return assemblyQualifiedName; }
        }

        /// <summary>
        /// the assembly name of the type
        /// </summary>
        [SerializeField]
        [Tooltip("the assembly name of the type")]
        private string assemblyName = "";

        /// <summary>
        /// returns the assembly name of the type
        /// </summary>
        public string AssemblyName
        {
            get { return assemblyName; }
        }

        /// <summary>
        /// the system type 
        /// </summary>
        private System.Type systemType;
        /// <summary>
        /// returns the system type 
        /// </summary>
        public System.Type SystemType
        {
            get
            {
                if (systemType == null)
                {
                    GetSystemType();
                }
                return systemType;
            }
        }

        #endregion
        #region methodes
        #region private
        /// <summary>
        /// gets the system type from the full name
        /// </summary>
        private void GetSystemType()
        {
            systemType = System.Type.GetType(assemblyQualifiedName);
        }

        #endregion
        #region public
        /// <summary>
        /// the constructor 
        /// </summary>
        /// <param name="SystemType"></param>
        public SerializableSystemType(System.Type SystemType)
        {
            systemType = SystemType;
            name = SystemType.Name;
            assemblyQualifiedName = SystemType.AssemblyQualifiedName;
            assemblyName = SystemType.Assembly.FullName;
        }

        /// <summary>
        /// check if we are equal to an other type
        /// </summary>
        /// <param name="obj">the other object</param>
        /// <returns>if this is the same type</returns>
        public override bool Equals(System.Object obj)
        {
            SerializableSystemType temp = obj as SerializableSystemType;
            if ((object)temp == null || this == null)
            {
                return false;
            }
            return this.Equals(temp);
        }

        /// <summary>
        /// internal check if this is eaqual to the other type
        /// </summary>
        /// <param name="Object">the other object</param>
        /// <returns>if we are equal</returns>
        public bool Equals(SerializableSystemType Object)
        {
            return string.Equals( AssemblyQualifiedName,Object.AssemblyQualifiedName);
            //return Object.SystemType.Equals(SystemType);
        }

        /// <summary>
        /// operator overload for equalety
        /// </summary>
        /// <param name="a">LH opperator</param>
        /// <param name="b">RH opperator</param>
        /// <returns>if we are equal</returns>
        public static bool operator ==(SerializableSystemType a, SerializableSystemType b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }
        /// <summary>
        /// operator overload for inequality
        /// </summary>
        /// <param name="a">LH opperator</param>
        /// <param name="b">RH opperator</param>
        /// <returns>if we are not equal to each other</returns>
        public static bool operator !=(SerializableSystemType a, SerializableSystemType b)
        {
            return !(a == b);
        }
        #endregion
        #endregion
    }
}