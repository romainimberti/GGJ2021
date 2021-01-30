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

        [SerializeField]
        private RenderTexture fogMainTexture;
        [SerializeField]
        private RenderTexture fogSecondaryTexture;

        #endregion
        #region Public

        public GameObject completeWallPrefab;
        public GameObject wallPrefab;
        public GameObject floorPrefab;
        public GameObject endPrefab;
        public GameObject startPrefab;
        public GameObject treeStumpPrefab;
        public Enemy enemyPrefab;

        public GameObject finishGameObject;
        public GameObject capacitiesGameObject;
        public ButtonWithClickAnimation btnFinish;


        public ButtonWithClickAnimation btnJump;
        public ButtonWithClickAnimation btnCut;
        public ButtonWithClickAnimation btnAttack;

        public Image imgAttack;
        public Image imgJump;
        public Image imgCut;

        public Sprite jumpSprite;
        public Sprite cutSprite;
        public Sprite attackSprite;
        public Sprite lockedSprite;

        public Camera cam;

        public Maze maze;

        public Player Player => player;

        #endregion
        #region Private

        private float level = 1;

        private Vector2Int interactableCell;

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
            btnJump.Init(Jump);
            btnCut.Init(Cut);
            btnAttack.Init(Attack);
        }

        #endregion
        #region Public

        public void MazeFinished()
        {
            level += 0.5f;
            player.Disable();
            finishGameObject.SetActive(true);
            capacitiesGameObject.SetActive(false);
            fogMainTexture.Release();
            fogSecondaryTexture.Release();
        }

        public void GenerateMaze()
        {
            capacitiesGameObject.SetActive(true);
            finishGameObject.SetActive(false);
            HandleCapacitiesUnlock();
            int width;
            int heigth;
            switch (level)
            {
                case 1.5F:
                    width = 9;
                    heigth = 5;
                    break;
                case 2.5F:
                    width = 9;
                    heigth = 5;
                    break;
                case 3.5F: // TODO Attack Tutorial
                default:
                    width = 23;
                    heigth = 13;
                    break;
            }
            CreateMaze(width, heigth);
            fogMainTexture.Release();
            fogSecondaryTexture.Release();
        }

        public void EnableCapacities(int x, int y)
        {
            Cell[,] mazeCells = maze.GetTiles();

            if (mazeCells[x, y].IsATreeStump()) {
                if (level > 1)
                {
                    btnJump.Interactable = true;
                    interactableCell = new Vector2Int(x, y);
                }
            }

            if (mazeCells[x, y].IsAWall()) {
                if (level > 2)
                {
                    if (maze.IsACutableWall(x, y))
                    {
                        btnCut.Interactable = true;
                        interactableCell = new Vector2Int(x, y);
                    }
                }
            }
        }

        public void DisableCapacities()
        {
            btnJump.Interactable = false;
            btnCut.Interactable = false;
            btnAttack.Interactable = false;
        }

        #endregion
        #region Protected

        #endregion
        #region Private

        private void Jump()
        {
            Cell[,] mazeCells = maze.GetTiles();
            bool horizontalJump = mazeCells[interactableCell.x + 1, interactableCell.y].IsWalkable();
            Vector3 newPlayerPos;

            if (horizontalJump)
            {
                if(player.transform.position.x < (float)interactableCell.x)
                    newPlayerPos = new Vector3(interactableCell.x + 1, player.transform.position.y, player.transform.position.z);
                else
                    newPlayerPos = new Vector3(interactableCell.x - 1, player.transform.position.y, player.transform.position.z);
            }
            else
            {
                if (player.transform.position.y < (float)interactableCell.y)
                    newPlayerPos = new Vector3(player.transform.position.x, interactableCell.y + 1, player.transform.position.z);
                else
                    newPlayerPos = new Vector3(player.transform.position.x, interactableCell.y - 1, player.transform.position.z);
            }

            LeanTween.move(player.gameObject, newPlayerPos, 0.2f).setEaseInOutQuad();
        }

        private void Cut()
        {
            Cell[,] mazeCells = maze.GetTiles();
            mazeCells[interactableCell.x, interactableCell.y].CutWall();
        }

        private void Attack()
        {
            Debug.Log("Attack");
        }

        private void HandleCapacitiesUnlock()
        {

            btnJump.Interactable = false;
            btnCut.Interactable = false;
            btnAttack.Interactable = false;

            imgJump.sprite = lockedSprite;
            imgCut.sprite = lockedSprite;
            imgAttack.sprite = lockedSprite;

            if (level > 1) {
                imgJump.sprite = jumpSprite;
            }

            if (level > 2) {
                imgCut.sprite = cutSprite;
            }

            if (level > 3) {
                imgAttack.sprite = attackSprite;
            }
        }

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
            SetCameraDimensions(width, height);

            switch (level)
            {
                case 1.5F:
                    maze.GenerateJumpUnlockTutorial();
                    break;
                case 2.5F:
                    maze.GenerateCutUnlockTutorial();
                    break;
                case 3.5F: // TODO Attack Tutorial
                default:
                    maze.Generate();
                    break;
            }
            Vector3 startPosition = new Vector3(maze.GetStartTile().x, maze.GetStartTile().y, -0.75f);
            player.transform.position = startPosition;
            player.Enable();
            DisplayEnemies(maze.GetEnemies());
        }

        private void DisplayEnemies(List<Vector2Int> monsterPositions)
        {
            foreach (Vector2Int monPos in monsterPositions)
            {
                Enemy enemy = Instantiate(enemyPrefab, new Vector3(monPos.x, monPos.y, -1), Quaternion.identity);
                enemy.transform.parent = Maze.mazeObject;
            }

        }

        #endregion
        #endregion
    }
}
