using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerpentExtensions;

public class Maze : MonoBehaviour 
{
	public int Width { get; set; }
	public int Height { get; set; }

	public MazeCell[,] Cells;

	// Use this for initialization
	void Start () 
	{
	
	}

	/*
	// Update is called once per frame
	void Update () {
	
	}
	*/

	public void Setup(Dictionary<string,object> mazeData)
	{
		int width = mazeData.GetInt(SerpentConsts.Width);
		int height = mazeData.GetInt(SerpentConsts.Height);
		object cellData = mazeData.GetObject(SerpentConsts.Cells);
		List<object> cellList = cellData as List<object>;

		if (width == 0 || height == 0 || cellList == null) { return; }

		this.Width = width;
		this.Height = height;

		this.Cells = new MazeCell[width,height];
		for (int j = 0; j < height; ++j)
		{
			for (int i = 0; i < width; ++i)
			{
				this.Cells[i,j] = new MazeCell(i, j);
			}
		}

		foreach (object cellObj in cellList)
		{
			Dictionary<string,object> cellDict = cellObj as Dictionary<string,object>;
			if (cellDict == null) { break; }

			int x = cellDict.GetInt(SerpentConsts.X);
			int y = cellDict.GetInt(SerpentConsts.Y);
			string wallData = cellDict.GetString(SerpentConsts.Walls);
			this.Cells[x,y].Setup(wallData);

		}
	}
}
