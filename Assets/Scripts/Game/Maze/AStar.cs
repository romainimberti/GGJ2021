using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2020.game.maze
{

	///<summary>
	///
	///</summary>
	public class AStar
	{

        #region Methods
        #region Unity

        #endregion
        #region Public

        public static List<Vector2Int> FindPath(Maze maze)
        {

            List<Vector2Int> path = new List<Vector2Int>();

            Vector2Int start = maze.GetStartTile();
            start.x += 1;
            Vector2Int end = maze.GetEndTile();
            end.x -= 1;

            //The path can't exist
            if (!maze.IsValid(start) || !maze.IsValid(end))
            {
                return path;
            }

            //Initialize all the nodes
            Cell[,] tiles = maze.GetTiles();
            Node[,] nodes = new Node[tiles.GetLength(0), tiles.GetLength(1)];
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    bool solid = tiles[i, j].wall;
                    nodes[i, j] = new Node(new Vector2Int(i, j), solid);
                }
            }
            Node startNode = new Node(start, tiles[start.x, start.y].wall);
            Node endNode = new Node(end, tiles[end.x, end.y].wall);

            //The path can't exist
            if (startNode.IsSolid() || endNode.IsSolid())
            {
                return path;
            }

            // Initialize both open and closed list
            List<Node> openList = new List<Node>();
            List<Node> closedList = new List<Node>();

            // Add the start node
            openList.Add(startNode);

            // Loop until we find the end
            while (openList.Count != 0)
            {

                // Get the current node
                Node current = FindLeastFValueNode(openList);
                openList.Remove(current);

                // Found the goal
                if (HasFoundTheGoal(current, endNode))
                {

                    while (current != null)
                    {
                        path.Add(current.pos);
                        current = current.parent;
                    }

                    path.Reverse();
                    return path;
                }

                // Generate children
                current.GenerateChildren(current, nodes);

                foreach (Node child in current.GetChildren())
                {

                    // Child is in the closedList
                    if (ContainsNode(closedList, child))
                    {
                        continue;
                    }

                    if (child.IsSolid())
                    {
                        continue;
                    }

                    // Create the f, g and h values
                    child.g = current.g + 1;
                    child.h = GetDistance(child, endNode);
                    child.f = child.g + child.h;

                    // Child is already in openList
                    List<Node> othersContained = child.GetNodesAtSamePosition(openList);
                    if (ContainsLeastGValue(othersContained, child))
                    {
                        continue;
                    }

                    // Add the child to the openList
                    if (!ContainsNode(openList, child))
                    {
                        openList.Add(child);
                    }
                    child.parent = current;
                }
                closedList.Add(current);

            }
            return path;
        }

        #endregion
        #region Protected

        #endregion
        #region Private


        //Check if at least one of the other nodes has a G value lower than the node's one
        private static bool ContainsLeastGValue(List<Node> nodes, Node node)
        {
            foreach (Node other in nodes)
            {
                if (other.g < node.g)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ContainsNode(List<Node> nodes, Node node)
        {
            foreach (Node current in nodes)
            {
                if (current.Equals(node))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasFoundTheGoal(Node current, Node endNode)
        {
            return current.Equals(endNode);
        }

        //Get the node that has the lowest F value
        private static Node FindLeastFValueNode(List<Node> openList)
        {
            if (openList.Count <= 0) return null;

            Node leastF = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].f < leastF.f)
                {
                    leastF = openList[i];
                }
            }
            return leastF;
        }

        //Checks if a given position is walkable (is not a wall)
        private static bool IsValid(Maze maze, Vector2Int pos)
        {
            return maze.IsWalkable(pos);
        }

        //Get the distance between two nodes
        private static double GetDistance(Node lhs, Node rhs)
        {
            return Mathf.Abs(rhs.pos.x - lhs.pos.x) + Mathf.Abs(rhs.pos.y - lhs.pos.y);
        }

        #endregion
        #endregion
    }
}
