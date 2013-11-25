using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerpentExtensions;
using MiniJSON;

public class Maze : MonoBehaviour 
{
	public int Width { get; set; }
	public int Height { get; set; }

	public MazeCell[,] Cells;

	// Use this for initialization
	void Start () 
	{
	
	}

	/// <summary>
	/// Sets up the maze using the provided text asset
	/// </summary>
	/// <param name="textAsset">Text asset.</param>
	public void SetUp(TextAsset textAsset)
	{
		if (textAsset.text == null) { return; }
		string jsonString = textAsset.text;
        Dictionary<string,object> dict = Json.Deserialize(jsonString) as Dictionary<string,object>;
		if (dict == null) { return; }

		SetUp(dict);
	}

	/// <summary>
	/// Set ups the maze using the provided dictionary of data
	/// </summary>
	/// <param name="mazeData">Maze data.</param>
	public void SetUp(Dictionary<string,object> mazeData)
	{
		// Parse dictionary data

		int width = mazeData.GetInt(SerpentConsts.WidthKey);
		int height = mazeData.GetInt(SerpentConsts.HeightKey);
		object rawWallData = mazeData.GetObject(SerpentConsts.WallsKey);
		List<object> wallData = rawWallData as List<object>;

		if (width == 0 || height == 0 || wallData == null) 
		{ 
			return; 
		}

		this.Width = width;
		this.Height = height;

		// Create all the cells
		this.Cells = new MazeCell[width,height];
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				this.Cells[x,y] = new MazeCell(x, y);
			}
		}

		CreateWalls(wallData);
	}

	private void CreateWalls(List<object> wallData)
	{		
		// Create walls. NOTE: the wall data comes in, with the rows ordered top to bottom.
		// But for ease of working with Unity we're going to assign these from bottom to top,
		// so that 0,0 is the lower left corner of the map.
		int y = (this.Height) - 1;
		foreach (object rowWallData in wallData)
		{
			List<object> cellsWallData = rowWallData as List<object>;
			if (cellsWallData == null) { break; }

			for (int x = 0; x < cellsWallData.Count; ++x)
			{
				IntVector2 position = new IntVector2(x, y);
				string cellWallData = cellsWallData[x] as string;
				// Each character in the string represents a wall.
				foreach (char c in cellWallData)
				{
					if (SerpentConsts.DirectionIndexes.ContainsKey(c) == false) { continue; }
					
					int side = SerpentConsts.DirectionIndexes[c];
					CreateWall(position, side);
				}
			}			

			--y;
			if (y < 0) { break; }
		}
	}

	private void CreateWall(IntVector2 position, int side)
	{
		MazeWall wall = new MazeWall();
		this.Cells[position.x,position.y].Walls[side] = wall;

		// Connect the other side of the wall as well, provided the other side of the wall is not "out of bounds"
		IntVector2 newPosition = position + SerpentConsts.DirectionVector[side];
		if (newPosition.x < 0 || newPosition.x >= this.Width || newPosition.y < 0 || newPosition.y >= this.Height) { return; }

		this.Cells[newPosition.x,newPosition.y].Walls[ SerpentConsts.OppositeDirection[side] ] = wall;

	}
}
