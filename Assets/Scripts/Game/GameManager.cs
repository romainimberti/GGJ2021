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

        [SerializeField]
        private RectTransform fogCanvas;

        [SerializeField]
        private GameOverUI gameOverGameObject;

        [SerializeField]
        private List<Sprite> firstCinematic;

        [SerializeField]
        private GameObject mainMenuBackground;

        [SerializeField]
        private GameObject cinematicBackground;

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
        public GameObject menuGameObject;
        public GameObject fogGameObject;
        public Joystick joystickGameObject;
        public ButtonWithClickAnimation btnFinish;
        public GameObject capacitiesGameObject;
        public GameObject cinematicGameObject;


        public ButtonWithClickAnimation btnJump;
        public ButtonWithClickAnimation btnCut;
        public ButtonWithClickAnimation btnAttack;
        public ButtonWithClickAnimation btnPlay;
        public ButtonWithClickAnimation cinematicNext;

        public Image imgAttack;
        public Image imgJump;
        public Image imgCut;

        public Sprite imgPlayerJump;
        public Sprite imgDarkStart;
        public Sprite imgDarkWall;
        public Sprite imgDarkTreeStump;
        public Sprite imgDarkCompleteWall;
        public Sprite imgDarkFloor;
        public Sprite imgDarkEnd;

        public Sprite imgFinishJump;
        public Sprite imgFinishCut;
        public Sprite imgFinishAttack;

        public Image imgCinematic;

        public List<Sprite> playerDeath;

        public Sprite imgLightStart;
        public Sprite imgLightWall;
        public Sprite imgLightTreeStump;
        public Sprite imgLightCompleteWall;
        public Sprite imgLightFloor;
        public Sprite imgLightEnd;

        public Sprite jumpSprite;
        public Sprite cutSprite;
        public Sprite attackSprite;
        public Sprite lockedJumpSprite;
        public Sprite lockedCutSprite;
        public Sprite lockedAttackSprite;

        public List<Sprite> attackSprites;

        public Camera cam;

        public Maze maze;

        public Player Player => player;

        public bool isGameOver = false;

        #endregion
        #region Private

        public float level = 1;

        private Vector2Int interactableCell;

        private int currentFirstCinematicSprite = 0;

        private bool playingFirstCinematic = false;

        private int tempo = 0;

        #endregion
        #endregion
        #region Methods
        #region Unity

        protected override void Awake()
        {
            base.Awake();

            Application.targetFrameRate = 60;

            LeanTween.scale(mainMenuBackground, Vector3.one * 1.03f, 5f).setEaseInOutQuad().setLoopPingPong(-1);
            LeanTween.scale(cinematicBackground, Vector3.one * 1.03f, 5f).setEaseInOutQuad().setLoopPingPong(-1);

            menuGameObject.SetActive(true);
            finishGameObject.SetActive(false);
            gameOverGameObject.gameObject.SetActive(false);
            fogGameObject.SetActive(false);
            joystickGameObject.gameObject.SetActive(false);
            capacitiesGameObject.SetActive(false);
            cinematicGameObject.SetActive(false);

            btnFinish.Init(GenerateMaze);
            btnJump.Init(Jump);
            btnCut.Init(Cut);
            btnAttack.Init(Attack);
            btnPlay.Init(PlayFirstCinematic);
            cinematicNext.Init(NextCinematicButton);

            AudioManager.Instance.PlayAudioClip(AudioManager.MUSIC.Menu);
        }

        private void FixedUpdate()
        {
            if (playingFirstCinematic)
            {
                tempo++;

                if (tempo > 5)
                {
                    tempo = 0;
                    Image imgCinematic = cinematicGameObject.GetComponentInChildren<Image>();
                    imgCinematic.sprite = firstCinematic[currentFirstCinematicSprite];
                    currentFirstCinematicSprite++;
                    if (currentFirstCinematicSprite > 3)
                        currentFirstCinematicSprite = 2;
                }
            }
        }

            #endregion
            #region Public

        public void GameOver()
        {
            AudioManager.Instance.Mute(true, AudioManager.CHANNEL.SECOND_MUSIC);
            int clip = Random.Range(0, 4);
            switch (clip)
            {
                case 0:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SamuraiDeath1);
                    break;
                case 1:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SamuraiDeath2);
                    break;
                case 2:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SamuraiDeath3);
                    break;
                case 3:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SamuraiDeath4);
                    break;
            }
            isGameOver = true;
            FrozeAllEnemies();
            player.Disable();
            joystickGameObject.gameObject.SetActive(false);
            capacitiesGameObject.SetActive(false);
            SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
            CoroutineManager.Instance.Wait(0.2f, () =>
            {
                AudioManager.Instance.PlayAudioClip(AudioManager.SFX.GameOver);
                playerSprite.sprite = playerDeath[0];
                CoroutineManager.Instance.Wait(0.5f, () => {
                    playerSprite.sprite = playerDeath[1];
                    CoroutineManager.Instance.Wait(0.5f, () => {
                        gameOverGameObject.SetFirstPlayerImage(player.playerSprites[player.currentSprite]);
                        gameOverGameObject.gameObject.SetActive(true);
                        gameOverGameObject.PlayDeathAnimation();
                        cinematicGameObject.SetActive(false);
                    });
                });
            });
        }

        public void EndGame()
        {
            menuGameObject.SetActive(true);
            finishGameObject.SetActive(false);
            gameOverGameObject.gameObject.SetActive(false);
            fogGameObject.SetActive(false);
            joystickGameObject.gameObject.SetActive(false);
            capacitiesGameObject.SetActive(false);
            cinematicGameObject.SetActive(false);

            btnFinish.Init(GenerateMaze);
            btnJump.Init(Jump);
            btnCut.Init(Cut);
            btnAttack.Init(Attack);
            btnPlay.Init(PlayFirstCinematic);
            cinematicNext.Init(NextCinematicButton);

            AudioManager.Instance.PlayAudioClip(AudioManager.MUSIC.Menu);

        }

        public void MazeFinished()
        {
            level += 0.5f;
            player.Disable();

            bool playCinematic = true;

            switch (level)
            {
                case 1.5F:
                    imgCinematic.sprite = imgFinishJump;
                    break;
                case 2.5F:
                    imgCinematic.sprite = imgFinishAttack;
                    break;
                case 3.5F:
                    imgCinematic.sprite = imgFinishCut;
                    break;
                case 4.5F:
                    imgCinematic.sprite = imgFinishAttack;
                    btnFinish.Init(EndGame);
                    break;
                default:
                    playCinematic = false;
                    break;
            }

            AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Congratulations);
            if (playCinematic)
            {
                if(level == 4.5F)
                {
                    AudioManager.Instance.PlayAudioClip(AudioManager.MUSIC.End);
                }
                finishGameObject.SetActive(true);
                gameOverGameObject.gameObject.SetActive(false);
                joystickGameObject.gameObject.SetActive(false);
                capacitiesGameObject.SetActive(false);
                cinematicGameObject.SetActive(false);
            }
            else
            {
                GenerateMaze();
            }
            /*fogMainTexture.Release();
            fogSecondaryTexture.Release();*/
        }

        public void GenerateMaze()
        {
            playingFirstCinematic = false;
            isGameOver = false;
            finishGameObject.SetActive(false);
            gameOverGameObject.gameObject.SetActive(false);
            menuGameObject.SetActive(false);
            //fogGameObject.SetActive(true);
            joystickGameObject.gameObject.SetActive(true);
            capacitiesGameObject.SetActive(true);
            cinematicGameObject.SetActive(false);
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
                case 3.5F:
                    width = 9;
                    heigth = 5;
                    break;
                default:
                    width = 23;
                    heigth = 13;
                    for (int i = 0; i < level - 1; i++)
                    {
                        width = (int)(width * 1.2);
                        heigth = (int)(heigth * 1.2);
                    }
                    break;
            }
            CreateMaze(width, heigth);
            player.gameObject.SetActive(true);
            /*fogMainTexture.Release();
            fogSecondaryTexture.Release();*/
            joystickGameObject.ResetJoystick();

            AudioManager.Instance.PlayAudioClip(AudioManager.MUSIC.Main);
            AudioManager.Instance.PlayAudioClip(AudioManager.SECOND_MUSIC.SpiderInRange);
            AudioManager.Instance.Mute(true, AudioManager.CHANNEL.SECOND_MUSIC);
        }

        public void EnableCapacities(int x, int y)
        {
            Cell[,] mazeCells = maze.GetTiles();

            if (mazeCells[x, y].IsATreeStump())
            {
                if (level > 1)
                {
                    btnJump.Interactable = true;
                    interactableCell = new Vector2Int(x, y);
                }
            }

            if (mazeCells[x, y].IsAWall())
            {
                if (level > 3)
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
        }

        #endregion
        #region Protected

        #endregion
        #region Private

        private void NextCinematicButton()
        {
            currentFirstCinematicSprite++;
            DisplayFirstCinematicSprite();
        }

        private void PlayFirstCinematic()
        {
            AudioManager.Instance.PlayAudioClip(AudioManager.MUSIC.Cinematic);
            cinematicGameObject.SetActive(true);
            DisplayFirstCinematicSprite();
        }

        private void DisplayFirstCinematicSprite()
        {
            Image imgCinematic = cinematicGameObject.GetComponentInChildren<Image>();
            imgCinematic.sprite = firstCinematic[currentFirstCinematicSprite];
            if(currentFirstCinematicSprite == 2)
            {
                playingFirstCinematic = true;
                cinematicNext.Init(GenerateMaze);
            }
        }

        private void Jump()
        {
            Cell[,] mazeCells = maze.GetTiles();
            bool horizontalJump = mazeCells[interactableCell.x + 1, interactableCell.y].IsWalkable();
            Vector3 newPlayerPos;


            int clip = Random.Range(0, 3);
            switch (clip)
            {
                case 0:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Jump1);
                    break;
                case 1:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Jump2);
                    break;
                case 2:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Jump3);
                    break;
            }

            if (horizontalJump)
            {
                if (player.transform.position.x < (float)interactableCell.x)
                    newPlayerPos = new Vector3(interactableCell.x + 1, player.transform.position.y, player.transform.position.z);
                else
                {
                    newPlayerPos = new Vector3(interactableCell.x - 1, player.transform.position.y, player.transform.position.z);
                }
            }
            else
            {
                if (player.transform.position.y < (float)interactableCell.y)
                    newPlayerPos = new Vector3(player.transform.position.x, interactableCell.y + 1, player.transform.position.z);
                else
                    newPlayerPos = new Vector3(player.transform.position.x, interactableCell.y - 1, player.transform.position.z);
            }

            SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
            playerSprite.sprite = imgPlayerJump;
            LeanTween.move(player.gameObject, newPlayerPos, 0.2f).setEaseInOutQuad().setOnComplete(() =>
                    playerSprite.sprite = player.playerSprites[player.currentSprite]
            );
        }

        private void Cut()
        {
            Cell[,] mazeCells = maze.GetTiles();
            SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
            playerSprite.sprite = attackSprites[0];
            CoroutineManager.Instance.Wait(0.03f, () =>
            {
                playerSprite.sprite = attackSprites[1];
                CoroutineManager.Instance.Wait(0.03f, () =>
                {
                    playerSprite.sprite = attackSprites[2];
                    CoroutineManager.Instance.Wait(0.03f, () =>
                    {
                        playerSprite.sprite = player.playerSprites[player.currentSprite];
                        mazeCells[interactableCell.x, interactableCell.y].CutWall();
                    });
                });
            });

            int clip = Random.Range(0, 3);
            switch (clip)
            {
                case 0:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordWood1);
                    break;
                case 1:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordWood2);
                    break;
                case 2:
                    AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordWood3);
                    break;
            }
        }

        private void Attack()
        {
            SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();

            if (player.enemiesInRange.Count == 0)
            {
                playerSprite.sprite = attackSprites[0];
                CoroutineManager.Instance.Wait(0.05f, () =>
                {
                    playerSprite.sprite = attackSprites[1];
                    CoroutineManager.Instance.Wait(0.05f, () =>
                    {
                        playerSprite.sprite = attackSprites[2];
                        CoroutineManager.Instance.Wait(0.05f, () =>
                        {
                            playerSprite.sprite = player.playerSprites[player.currentSprite];
                        });
                    });
                });

                int clip = Random.Range(0, 3);
                switch (clip)
                {
                    case 0:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordEmpty1);
                        break;
                    case 1:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordEmpty2);
                        break;
                    case 2:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordEmpty3);
                        break;
                }
            }
            else
            {
                int clip = Random.Range(0, 4);
                switch (clip)
                {
                    case 0:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordSpider1);
                        break;
                    case 1:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordSpider2);
                        break;
                    case 2:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordSpider3);
                        break;
                    case 3:
                        AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SwordSpider4);
                        break;
                }
            }

            for(int i = 0; i < player.enemiesInRange.Count; i++)
            {
                Enemy enemy = player.enemiesInRange[i];
                playerSprite.sprite = attackSprites[0];
                CoroutineManager.Instance.Wait(0.05f, () =>
                {
                    playerSprite.sprite = attackSprites[1];
                    CoroutineManager.Instance.Wait(0.05f, () =>
                    {
                        playerSprite.sprite = attackSprites[2];
                        enemy.Die();
                        CoroutineManager.Instance.Wait(0.05f, () =>
                        {
                            playerSprite.sprite = player.playerSprites[player.currentSprite];
                        });
                    });
                });
                enemy.Die();
            }
        }

        private void HandleCapacitiesUnlock()
        {
            btnJump.Interactable = false;
            btnCut.Interactable = false;
            btnAttack.Interactable = false;

            imgJump.sprite = lockedJumpSprite;
            imgCut.sprite = lockedCutSprite;
            imgAttack.sprite = lockedAttackSprite;

            if (level > 1)
            {
                imgJump.sprite = jumpSprite;
            }

            if (level > 3)
            {
                imgCut.sprite = cutSprite;
            }

            if (level > 2)
            {
                imgAttack.sprite = attackSprite;
                btnAttack.Interactable = true;
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
            /*
            fogCameraMain.transform.position = cam.transform.position;
            fogCameraSecondary.transform.position = cam.transform.position;

            fogCanvas.sizeDelta = new Vector2(cam.rect.width, cam.rect.height);
            fogCanvas.transform.position = cam.transform.position.With(z: fogCanvas.transform.position.z);

            go_fogMainCircle.SetActive(true);
            go_fogSecondaryCircle.SetActive(true);
            */
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

            if (level > 2.5)
            {
                startPrefab.GetComponent<SpriteRenderer>().sprite = imgDarkStart;
                wallPrefab.GetComponent<SpriteRenderer>().sprite = imgDarkWall;
                completeWallPrefab.GetComponent<SpriteRenderer>().sprite = imgDarkCompleteWall;
                treeStumpPrefab.GetComponent<SpriteRenderer>().sprite = imgDarkTreeStump;
                floorPrefab.GetComponent<SpriteRenderer>().sprite = imgDarkFloor;
                wallPrefab.GetComponent<SpriteRenderer>().sprite = imgDarkWall;
                endPrefab.GetComponent<SpriteRenderer>().sprite = imgDarkEnd;

            }
            else
            {
                startPrefab.GetComponent<SpriteRenderer>().sprite = imgLightStart;
                wallPrefab.GetComponent<SpriteRenderer>().sprite = imgLightWall;
                completeWallPrefab.GetComponent<SpriteRenderer>().sprite = imgLightCompleteWall;
                treeStumpPrefab.GetComponent<SpriteRenderer>().sprite = imgLightTreeStump;
                floorPrefab.GetComponent<SpriteRenderer>().sprite = imgLightFloor;
                endPrefab.GetComponent<SpriteRenderer>().sprite = imgLightEnd;
            }


            switch (level)
            {
                case 1.5F:
                    maze.GenerateJumpUnlockTutorial();
                    break;
                case 2.5F:
                    maze.GenerateAttackUnlockTutorial();
                    break;
                case 3.5F:
                    maze.GenerateCutUnlockTutorial();
                    break;
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
                enemy.freeze = maze.enemiesFrozen;
            }

        }

        private void FrozeAllEnemies()
        {
            Component[] enemies = Maze.mazeObject.GetComponentsInChildren(typeof(Enemy));
            foreach (Enemy enemy in enemies)
            {
                enemy.freeze = true;
            }
        }

        #endregion
        #endregion
    }
}
