using com.romainimberti.ggj2021.game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2020.game.maze
{

    ///<summary>
    ///
    ///</summary>
    public class Cell
    {
        #region Variables
        #region Editor

        #endregion
        #region Public

        public GameObject obj; //The in-game object representing the cell

        #endregion
        #region Private


        private CellKind kind;

        private int x, y;

        #endregion
        #endregion
        #region Methods
        #region Unity

        #endregion
        #region Public

        public Cell(CellKind kind, int x, int y)
        {
            this.kind = kind;
            this.x = x;
            this.y = y;
            this.obj = CreateObject();
        }

        #endregion
        #region Protected

        #endregion
        #region Private

        private GameObject CreateObject()
        {
            GameObject prefab;
            switch (kind)
            {
                case CellKind.Wall:
                    prefab = GameManager.Instance.wallPrefab;
                    break;
                case CellKind.CompleteWall:
                    prefab = GameManager.Instance.completeWallPrefab;
                    break;
                case CellKind.TreeStump:
                    prefab = GameManager.Instance.treeStumpPrefab;
                    break;
                default:
                    prefab = GameManager.Instance.floorPrefab;
                    break;
            }
            return Maze.Instantiate(prefab, x, y);
        }
        //"Dig" the cell, if it is a wall, then it becomes a floor. If it is a Floor it randomly becomes a Wall or a treeStump
        public void Dig()
        {
            if (kind.Equals(CellKind.Floor))
                this.kind = CellKind.Wall;
            else
                this.kind = CellKind.Floor;

            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
            this.obj = CreateObject();
        }

        public void CutWall()
        {
            this.kind = CellKind.TreeStump;
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
            this.obj = CreateObject();
        }

        public CellKind GetKind()
        {
            return kind;
        }

        public void SetAsCompleteWall()
        {
            this.kind = CellKind.CompleteWall;
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
            this.obj = CreateObject();
        }

        public bool IsWalkable()
        {
            return kind.Equals(CellKind.Floor);
        }

        public bool IsAWall()
        {
            return kind.Equals(CellKind.Wall);
        }

        #endregion
        #endregion
    }
}
