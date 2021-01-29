using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2021.utilities
{

	///<summary>
	///Class that handles the extension methods for Vector3
	///</summary>
	public static class Vector3Extenstions
	{

        /// <summary>
        /// Combines a Vector3 with given values
        /// </summary>
        /// <param name="original">The vector3</param>
        /// <param name="x">The x value to set</param>
        /// <param name="y">The y value to set</param>
        /// <param name="z">The z value to set</param>
        /// <returns>A Vector3</returns>
        public static Vector3 With(this Vector3 original, float? x = null, float? y = null, float? z = null)
        {
            float newX = x.HasValue ? x.Value : original.x;
            float newY = y.HasValue ? y.Value : original.y;
            float newZ = z.HasValue ? z.Value : original.z;
            return new Vector3(newX, newY, newZ);
        }

        /// <summary>
        /// Combines a Vector2Int with given values
        /// </summary>
        /// <param name="original">The vector2int</param>
        /// <param name="x">The x value to set</param>
        /// <param name="y">The y value to set</param>
        /// <returns>A Vector3</returns>
        public static Vector2Int With(this Vector2Int original, int? x = null, int? y = null)
        {
            int newX = x.HasValue ? x.Value : original.x;
            int newY = y.HasValue ? y.Value : original.y;
            return new Vector2Int(newX, newY);
        }

        /// <summary>
        /// Combines a Vector2 with given values
        /// </summary>
        /// <param name="original">The Vector2</param>
        /// <param name="x">The x value to set</param>
        /// <param name="y">The y value to set</param>
        /// <returns>A Vector3</returns>
        public static Vector2 With(this Vector2 original, float? x = null, float? y = null)
        {
            float newX = x.HasValue ? x.Value : original.x;
            float newY = y.HasValue ? y.Value : original.y;
            return new Vector2(newX, newY);
        }

    }
}
