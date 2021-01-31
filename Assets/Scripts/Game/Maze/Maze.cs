using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.romainimberti.ggj2021.game;
using com.romainimberti.ggj2021.utilities;

namespace com.romainimberti.ggj2020.game.maze
{

    ///<summary>
    ///
    ///</summary>
    public class Maze
    {
        #region Variables
        #region Editor

        #endregion
        #region Public

        #endregion
        #region Private


        private Vector2Int ERROR_VECTOR = new Vector2Int(-1, -1);

        public static Transform mazeObject;

        public bool enemiesFrozen = false;

        private int width, height;

        private Cell[,] maze;

        private Vector2Int startTile, endTile;

        private List<Vector2Int> enemies = new List<Vector2Int>();
        [SerializeField]
        private int minimumDistanceMobStart = 2;

        private bool generated = false;

        #endregion
        #endregion
        #region Methods
        #region Unity

        #endregion
        #region Public

        public Maze(int width, int height)
        {
            this.width = width;
            this.height = height;
            mazeObject = new GameObject("Maze").transform;
        }

        public void GenerateJumpUnlockTutorial()
        {
            maze = new Cell[width, height];
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j] = new Cell(CellKind.Floor, i, j);
                    if (i == 0 || j == 0 || i == maze.GetLength(0) - 1 || j == maze.GetLength(1) - 1)
                    {
                        maze[i, j].Dig();
                    }

                    if (i == maze.GetLength(0) / 2 && maze[i, j].IsWalkable())
                    {
                        maze[i, j].Dig();
                        maze[i, j].CutWall();
                    }

                }
            }
            SetCorrectWallSprite();
            CompleteGeneration();
        }

        public void GenerateCutUnlockTutorial()
        {
            maze = new Cell[width, height];
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j] = new Cell(CellKind.Floor, i, j);
                    if (i == 0 || j == 0 || i == maze.GetLength(0) - 1 || j == maze.GetLength(1) - 1)
                    {
                        maze[i, j].Dig();
                    }

                    if (i == maze.GetLength(0) / 2 && maze[i, j].IsWalkable())
                    {
                        maze[i, j].Dig();
                    }

                }
            }
            SetCorrectWallSprite();
            CompleteGeneration();
        }

        public void GenerateAttackUnlockTutorial()
        {
            maze = new Cell[width, height];
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j] = new Cell(CellKind.Floor, i, j);
                    if (i == 0 || j == 0 || i == maze.GetLength(0) - 1 || j == maze.GetLength(1) - 1)
                    {
                        maze[i, j].Dig();
                    }

                    if (i == maze.GetLength(0) / 2 && maze[i, j].IsWalkable())
                    {
                        enemies.Add(new Vector2Int(i, j));
                    }

                }
            }
            enemiesFrozen = true;
            SetCorrectWallSprite();
            CompleteGeneration();
        }

        public void Generate()
        {

            //We want a minimum maze size
            if (width < 4 || height < 4)
            {
                Debug.Log("Invalid dimensions.");
                return;
            }

            //Fill-in the maze with pavements only
            maze = new Cell[width, height];
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j] = new Cell(CellKind.Floor, i, j);
                    if (i == 0 || j == 0 || i == maze.GetLength(0) - 1 || j == maze.GetLength(1) - 1)
                    {
                        maze[i, j].Dig();
                    }

                }
            }

            //Start generating the maze
            RecursiveDivision(width - 2, height - 2, Vector2Int.one);
            //The algorithm is done

            GenerateTreeStump();
            SetCorrectWallSprite();
            CompleteGeneration();
            RandomEnemyGenerator();

        }

        public void RandomEnemyGenerator()
        {

            List<Vector2Int> possibleSpawns =  new List<Vector2Int>();

            for (int x = 0; x < maze.GetLength(0); x++)
            {
                for (int y = 0; y < maze.GetLength(1); y++)
                {
                    Vector2Int currentPos = new Vector2Int(x, y);
                    //TODO USE A  VARIABLE
                    if (IsWalkable(currentPos) && Vector2.Distance(currentPos, startTile) > minimumDistanceMobStart)
                    {
                        Vector3 fromPosition = new Vector3(currentPos.x, currentPos.y, -0.75f);
                        Vector3 toPosition = new Vector3(startTile.x, startTile.y, -0.75f);
                        bool toCloseFromAnOther = false;
                        for(int i = 0; i < possibleSpawns.Count; i++)
                        {
                            Vector3 currentPossibility = new Vector3(possibleSpawns[i].x, possibleSpawns[i].y, -0.75f);
                            if (Enemy.PlayerInRange(fromPosition, currentPossibility, 5))
                            {
                                toCloseFromAnOther = true;
                                break;
                            }
                        }
                        if (!Enemy.PlayerInRange(fromPosition, toPosition, 20) && !toCloseFromAnOther)
                            possibleSpawns.Add(currentPos);
                        //enemies.Add(currentPos);
                    };
                }
            }

            int nbOfEnemies = Random.Range(1, possibleSpawns.Count < 10 ? possibleSpawns.Count : 10);
            for (int i = 0; i < nbOfEnemies; i++)
            {
                enemies.Add(possibleSpawns[Random.Range(0, possibleSpawns.Count)]);
            }
        }

        public static GameObject Instantiate(GameObject o, int x, int y)
        {
            return GameObject.Instantiate(o, new Vector3(x, y, 0), Quaternion.identity, mazeObject);
        }

        //Destroy the maze
        public void Destroy()
        {
            if (mazeObject != null && mazeObject.gameObject != null)
            {
                GameObject.Destroy(mazeObject.gameObject);
            }
        }

        public Cell[,] GetTiles()
        {
            return maze;
        }

        //If a given position is valid or not (prevent OutOfBoundsException)
        public bool IsValid(Vector2Int position)
        {
            return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
        }

        //If a given position is walkable (is not a wall)
        public bool IsWalkable(Vector2Int position)
        {
            return IsValid(position) && maze != null && maze[position.x, position.y].IsWalkable();
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public Vector2Int GetStartTile()
        {
            return startTile;
        }

        public Vector2Int GetEndTile()
        {
            return endTile;
        }

        public List<Vector2Int> GetEnemies()
        {
            return enemies;
        }

        public static int Compare(int lhs, int rhs)
        {
            if (lhs < rhs)
            {
                return -1;
            }
            if (lhs > rhs)
            {
                return 1;
            }
            return 0;
        }

        //If a given position is at the border of the maze (if true, it will then be a wall)
        public bool IsBorder(Vector2Int cell)
        {
            return cell.x == 0 || cell.x == width - 1 || cell.y == 0 || cell.y == height - 1;
        }

        public bool IsGenerated()
        {
            return generated;
        }

        #endregion
        #region Protected

        #endregion
        #region Private

        private void RecursiveDivision(int width, int height, Vector2Int leftBotCorner)
        {

            //We reached a sufficient small size, we can stop the algorithm
            if (width <= 2 || height <= 2)
            {
                return;
            }

            //Has to cut horizontally
            bool horizontal = CutHorizontaly(width, height);

            //Possible start cells
            List<Vector2Int> possibilities = new List<Vector2Int>();
            if (horizontal)
            {
                for (int j = 1; j < height - 1; j++)
                {
                    possibilities.Add(new Vector2Int(leftBotCorner.x, leftBotCorner.y + j));
                }
            }
            else
            {
                for (int i = 1; i < width - 1; i++)
                {
                    possibilities.Add(new Vector2Int(leftBotCorner.x + i, leftBotCorner.y));
                }
            }

            //Choosing the starting cell
            Vector2Int wallStartingCell;
            do
            {
                wallStartingCell = possibilities[Random.Range(0, possibilities.Count)];

                if (!IsStartValid(horizontal, wallStartingCell, width, height))
                {
                    possibilities.Remove(wallStartingCell);
                }
            } while (!IsStartValid(horizontal, wallStartingCell, width, height) && possibilities.Count > 0);

            //No possibility, we stop this branch of the algorithm
            if (possibilities.Count == 0)
            {
                return;
            }

            //Choosing the position of the hole
            Vector2Int hole = Vector2Int.zero;
            hole.x = wallStartingCell.x + (horizontal ? Random.Range(0, width) : 0);
            hole.y = wallStartingCell.y + (horizontal ? 0 : Random.Range(0, height));

            //Create the wall
            if (horizontal)
            {
                for (int i = 0; i < width; i++)
                {
                    Vector2Int cellPosition = new Vector2Int(wallStartingCell.x + i, wallStartingCell.y);
                    if (!cellPosition.Equals(hole))
                    {
                        maze[cellPosition.x, cellPosition.y].Dig();
                    }
                }
            }
            else
            {
                for (int j = 0; j < height; j++)
                {
                    Vector2Int cellPosition = new Vector2Int(wallStartingCell.x, wallStartingCell.y + j);
                    if (!cellPosition.Equals(hole))
                    {
                        maze[cellPosition.x, cellPosition.y].Dig();
                    }
                }
            }

            //Launch the other executions
            if (horizontal)
            {
                RecursiveDivision(width, wallStartingCell.y - leftBotCorner.y, new Vector2Int(leftBotCorner.x, leftBotCorner.y));
                RecursiveDivision(width, height - (wallStartingCell.y - leftBotCorner.y) - 1, new Vector2Int(leftBotCorner.x, wallStartingCell.y + 1));
            }
            else
            {
                RecursiveDivision(wallStartingCell.x - leftBotCorner.x, height, new Vector2Int(leftBotCorner.x, leftBotCorner.y));
                RecursiveDivision(width - (wallStartingCell.x - leftBotCorner.x) - 1, height, new Vector2Int(wallStartingCell.x + 1, leftBotCorner.y));
            }
        }

        private void SetCorrectWallSprite()
        {
            for (int x = 0; x < maze.GetLength(0); x++)
            {
                for (int y = 0; y < maze.GetLength(1); y++)
                {
                    if (IsACompleteWall(x, y))
                    {
                        maze[x, y].SetAsCompleteWall();
                    }
                }
            }
        }

        private bool IsACompleteWall(int x, int y)
        {
            return maze[x, y].IsAWall() &&
                   (y == 0 || !maze[x, y - 1].IsWalkable() && !maze[x, y - 1].IsATreeStump());
        }

        private void GenerateTreeStump()
        {
            for (int x = 0; x < maze.GetLength(0); x++)
            {
                for (int y = 0; y < maze.GetLength(1); y++)
                {
                    CutWall(x, y);
                }
            }
        }

        private void CutWall(int x, int y)
        {
            if (IsACutableWall(x, y))
            {
                // TODO --> Randomly generate treeStumps
                // 90% % to be a treeStump, but it can be changed
                if (Random.Range(0, 100) < 10)
                    maze[x, y].CutWall();
            }
        }

        public bool IsACutableWall(int x, int y)
        {
            return maze[x, y].IsAWall() &&
                   !IsBorder(new Vector2Int(x, y)) &&
                   HasOnlyOppositesAsFloor(x, y);
        }

        private bool HasOnlyOppositesAsFloor(int x, int y)
        {
            return ((maze[x + 1, y].IsWalkable() && maze[x - 1, y].IsWalkable()) && (!maze[x, y + 1].IsWalkable() && !maze[x, y - 1].IsWalkable())) ||
                   ((maze[x, y + 1].IsWalkable() && maze[x, y - 1].IsWalkable()) && (!maze[x + 1, y].IsWalkable() && !maze[x - 1, y].IsWalkable()));
        }

        //Is a given starting cell valid (can we start a wall from this cell)
        private bool IsStartValid(bool horizontal, Vector2Int wallStartingCell, int width, int height)
        {
            //If we want to cut horizontally
            if (horizontal)
            {

                if (wallStartingCell.x > 0)
                {
                    if (maze[wallStartingCell.x - 1, wallStartingCell.y].IsWalkable())
                    {
                        return false;
                    }
                }

                if (width + wallStartingCell.x < this.width)
                {
                    return !maze[width + wallStartingCell.x, wallStartingCell.y].IsWalkable();
                }

                return true;
            }

            if (wallStartingCell.y > 0)
            {
                if (maze[wallStartingCell.x, wallStartingCell.y - 1].IsWalkable())
                {
                    return false;
                }
            }

            if (height + wallStartingCell.y < this.height)
            {
                return !maze[wallStartingCell.x, height + wallStartingCell.y].IsWalkable();
            }

            return true;
        }

        //Choose if we want to cut horizontally or not
        private bool CutHorizontaly(int width, int height)
        {
            if (width < height)
            {
                return true;
            }
            if (width > height)
            {
                return false;
            }
            //width == height, we choose randomly
            return Random.Range(0, 2) == 1;
        }


        private void CompleteGeneration()
        {
            //Compute the start and end tiles
            startTile = ComputeStartTile();
            endTile = ComputeEndTile();
            if (startTile == ERROR_VECTOR || endTile == ERROR_VECTOR)
            {
                Debug.Log("No start | end tiles possible, aborting..");
                return;
            }

            maze[startTile.x, startTile.y].Dig();
            maze[endTile.x, endTile.y].Dig();

            maze[startTile.x, startTile.y].obj = Instantiate(GameManager.Instance.startPrefab, startTile.x, startTile.y);
            maze[endTile.x, endTile.y].obj = Instantiate(GameManager.Instance.endPrefab, endTile.x, endTile.y);

            generated = true;
        }

        private Vector2Int ComputeStartTile()
        {
            List<Vector2Int> possibleStarts = new List<Vector2Int>();
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                if (maze[1, j].IsWalkable())
                {
                    possibleStarts.Add(new Vector2Int(0, j));
                }
            }

            return possibleStarts[Random.Range(0, possibleStarts.Count - 1)];
        }
        private Vector2Int ComputeEndTile()
        {
            List<Vector2Int> possibleEnds = new List<Vector2Int>();
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                if (maze[maze.GetLength(0) - 2, j].IsWalkable())
                {
                    possibleEnds.Add(new Vector2Int(maze.GetLength(0) - 1, j));
                }
            }

            return possibleEnds[Random.Range(0, possibleEnds.Count - 1)];
        }

        private void Display()
        {
            string res = "";
            for (int i = 0; i < maze.GetLength(0); i++)
            {

                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    if (!maze[i, j].IsWalkable())
                    {
                        res += 1;
                    }
                    else
                    {
                        res += 0;
                    }

                }
                res += "\n";
            }
            res += "";
            Debug.LogError(res);
        }
        #endregion
        #endregion
    }
}
