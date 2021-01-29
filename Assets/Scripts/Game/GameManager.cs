using com.romainimberti.ggj2021.game.camera;
using com.romainimberti.ggj2020.game.maze;
using com.romainimberti.ggj2021.utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2021.game
{

	///<summary>
	///Class that handles the game manager
	///</summary>
	public class GameManager : SingletonBehaviour<GameManager>
	{
        #region Variables
        #region Editor

        #endregion
        #region Public

        public GameObject wallPrefab;
        public GameObject floorPrefab;
        public GameObject endPrefab;
        public GameObject startPrefab;
        public GameObject treeStumpPrefab;

        public Camera cam;

        #endregion
        #region Private

        private Maze maze;

        #endregion
        #endregion
        #region Methods
        #region Unity

        private void Awake()
        {
            CreateMaze(29, 17);
            SetCameraDimensions(29, 17);
        }

        #endregion
        #region Public

        #endregion
        #region Protected

        #endregion
        #region Private

        public void SetCameraDimensions(int width, int height)
        {

            if (width < 1 || height < 1)
            {
                return;
            }

            cam.transform.position = new Vector3(width * 1 / 2, height * 1 / 2, -10);
            cam.orthographicSize = Mathf.Max(width / 3, height / 3);
        }

        //Create the maze
        private void CreateMaze(int width, int height)
        {
            //Destroy the previous maze
            if (maze != null)
            {
                maze.Destroy();
            }

            maze = new Maze(width, height);

            //Start generating the maze
            maze.Generate();

            GameObject.Find("Player").transform.position = new Vector3(maze.GetStartTile().x, maze.GetStartTile().y, 0);
        }

        #endregion
        #endregion
    }
}
