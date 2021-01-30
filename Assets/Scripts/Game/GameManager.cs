using com.romainimberti.ggj2021.game.camera;
using com.romainimberti.ggj2020.game.maze;
using com.romainimberti.ggj2021.utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.romainimberti.ggj2020;
using com.romainimberti.ggj2021.ui;

namespace com.romainimberti.ggj2021.game
{

    ///<summary>
    ///Class that handles the game manager
    ///</summary>
    public class GameManager : SingletonBehaviour<GameManager>
    {
        #region Variables
        #region Editor

        [SerializeField]
        private Player player;

        [SerializeField]
        private RawImage img_fog;

        [SerializeField]
        private Camera fogCameraMain;
        [SerializeField]
        private Camera fogCameraSecondary;

        [SerializeField]
        private GameObject go_fogMainCircle;
        [SerializeField]
        private GameObject go_fogSecondaryCircle;

        #endregion
        #region Public

        public GameObject completeWallPrefab;
        public GameObject wallPrefab;
        public GameObject floorPrefab;
        public GameObject endPrefab;
        public GameObject startPrefab;
        public GameObject treeStumpPrefab;
        public GameObject enemyPrefab;

        public GameObject finishGameObject;
        public ButtonWithClickAnimation btnFinish;

        public Camera cam;

        public Maze maze;

        public Player Player => player;

        #endregion
        #region Private

        #endregion
        #endregion
        #region Methods
        #region Unity

        protected override void Awake()
        {
            base.Awake();

            Application.targetFrameRate = 60;

            GenerateMaze();
            btnFinish.Init(GenerateMaze);
        }

        #endregion
        #region Public

        public void MazeFinished()
        {
            player.Disable();
            finishGameObject.SetActive(true);
        }

        public void GenerateMaze()
        {
            finishGameObject.SetActive(false);
            CreateMaze(29, 17);
            SetCameraDimensions(29, 17);
        }

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

            fogCameraMain.transform.position = cam.transform.position;
            fogCameraSecondary.transform.position = cam.transform.position;

            go_fogMainCircle.SetActive(true);
            go_fogSecondaryCircle.SetActive(true);
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
            Vector3 startPosition = new Vector3(maze.GetStartTile().x, maze.GetStartTile().y, -0.75f);
            player.transform.position = startPosition;
            player.Enable();
            DisplayEnemies(maze.GetEnemies());
            GameObject.Find("Player").transform.position = new Vector3(maze.GetStartTile().x, maze.GetStartTile().y, -0.75f);
        }

        private void DisplayEnemies(List<Vector2Int> monsterPositions)
        {
            foreach (Vector2Int monPos in monsterPositions)
            {
                Instantiate(enemyPrefab, new Vector3(monPos.x, monPos.y, -1), Quaternion.identity);
            }

        }

        #endregion
        #endregion
    }
}
