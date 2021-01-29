using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2020.game.maze
{

	///<summary>
	///
	///</summary>
	public class Node
	{
		#region Variables
		#region Editor

		#endregion
		#region Public

		public Vector2Int pos;

		public double f, g, h;

		public Node parent;

		public List<Node> children;

		#endregion
		#region Private

		private bool solid;

        #endregion
        #endregion
        #region Methods
        #region Unity

        #endregion
        #region Public

        public Node(Vector2Int pos, bool solid)
        {
            this.pos = pos;
            this.solid = solid;
            this.f = 0;
            this.g = 0;
            this.h = 0;
            this.children = new List<Node>();
            this.parent = null;
        }

        public bool IsSolid()
        {
            return solid;
        }

        public bool Equals(Node other)
        {
            return pos.x == other.pos.x && pos.y == other.pos.y;
        }

        public void GenerateChildren(Node parent, Node[,] nodes)
        {

            //West
            if (pos.x > 0)
            {
                Node west = nodes[pos.x - 1, pos.y];
                children.Add(west);
            }
            //East
            if (pos.x < nodes.GetLength(0) - 1)
            {
                Node east = nodes[pos.x + 1, pos.y];
                children.Add(east);
            }
            //South
            if (pos.y > 0)
            {
                Node south = nodes[pos.x, pos.y - 1];
                children.Add(south);
            }
            //North
            if (pos.y < nodes.GetLength(1) - 1)
            {
                Node north = nodes[pos.x, pos.y + 1];
                children.Add(north);
            }
        }

        public List<Node> GetChildren()
        {
            return children;
        }

        //Get the list of nodes that have the same position as this node
        public List<Node> GetNodesAtSamePosition(List<Node> nodes)
        {
            List<Node> others = new List<Node>();
            foreach (Node node in nodes)
            {
                if (node.Equals(this))
                {
                    others.Add(node);
                }
            }
            return others;
        }

        #endregion
        #region Protected

        #endregion
        #region Private

        #endregion
        #endregion
    }
}
