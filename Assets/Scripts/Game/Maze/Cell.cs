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
		public bool wall;

		#endregion
		#region Private

		private int x, y;

		#endregion
		#endregion
		#region Methods
		#region Unity

		#endregion
		#region Public

		public Cell(bool wall, int x, int y)
		{
			this.wall = wall;
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
			GameObject o;
			if (wall)
			{
				GameObject wall = GameManager.Instance.wallPrefab;
				o = Maze.Instantiate(wall, x, y);
			}
			else
			{
				o = Maze.Instantiate(GameManager.Instance.floorPrefab, x, y);
			}
			return o;
		}
		//"Dig" the cell, if it is a wall, then it becomes a path and vice versa
		public void Dig(bool create = true)
		{
			this.wall = !this.wall;
			if (obj != null)
			{
				GameObject.Destroy(obj);
			}
			if (create)
			{
				this.obj = CreateObject();
			}
		}

		#endregion
		#endregion
	}
}
