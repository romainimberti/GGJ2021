using com.romainimberti.ggj2021.utilities;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2021
{
    public class Consts
    {
        public const float MIN = -9999999;
        public const float MAX = 9999999;

        public static Vector2Int VECTOR2INT_NULL = new Vector2Int(-1, -1);
        public static Vector2Int VECTOR2INT_MIN = new Vector2Int((int)MIN, (int)MIN);
        public static Vector2Int VECTOR2INT_MAX = new Vector2Int((int)MAX, (int)MAX);

        public static readonly Dictionary<Enums.Direction, Vector2Int> DIRECTION_TO_VECTOR = new Dictionary<Enums.Direction, Vector2Int>()
        {
            { Enums.Direction.Left, Vector2Int.left },
            { Enums.Direction.Right, Vector2Int.right },
            { Enums.Direction.Up, Vector2Int.up },
            { Enums.Direction.Down, Vector2Int.down }
        };
        public static readonly Dictionary<Vector2Int, Enums.Direction> VECTOR_TO_DIRECTION = new Dictionary<Vector2Int, Enums.Direction>()
        {
            { Vector2Int.left, Enums.Direction.Left },
            { Vector2Int.right, Enums.Direction.Right },
            { Vector2Int.up, Enums.Direction.Up },
            { Vector2Int.down, Enums.Direction.Down }
        };

        public const string GAME_SCENE = "GameScene";

        public static bool AdsRemoved() => PlayerPrefsX.GetBool(PlayerPrefsKeys.KEY_ADS_REMOVED, false);
        public static void RemoveAds() => PlayerPrefsX.SetBool(PlayerPrefsKeys.KEY_ADS_REMOVED, true);

    }
}