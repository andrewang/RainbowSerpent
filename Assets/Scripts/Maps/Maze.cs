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
		object rawDoorsData = mazeData.GetObject(SerpentConsts.DoorsKey);
		
		List<object> wallData = rawWallData as List<object>;
		List<object> doorData = rawDoorsData as List<object>;
		
		// door data is optional so don't cancel if there isn't any
		if (width == 0 || height == 0 || wallData == null) 
		{ 
			return; 
		}

		this.Width = width;
		this.Height = height;
		Debug.Log("Maze width " + width + " and height " + height);

		// Create all the cells
		this.Cells = new MazeCell[width,height];
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				this.Cells[x,y] = new MazeCell(x, y);
			}
		}

		CreateOutsideWalls();
		CreateWalls(wallData);
		
		if (doorData != null)
		{
			CreateDoors(doorData);
		}
	}
	
	private void CreateOutsideWalls()
	{
		IntVector2 position = new IntVector2(0, 0);
		for (int x = 0; x < this.Width; ++x)
		{
			position.x = x;
			position.y = 0;
			CreateWall(position, SerpentConsts.Dir.S);
			position.y = (this.Height - 1);
			CreateWall(position, SerpentConsts.Dir.N);
		}
		
		for (int y = 0; y < this.Height; ++y)
		{
			position.y = y;
			position.x = 0;
			CreateWall(position, SerpentConsts.Dir.W);
			position.x = (this.Width - 1);
			CreateWall(position, SerpentConsts.Dir.E);
		}
	}

	private void CreateWalls(List<object> wallData)
	{		
		// Create walls. NOTE: the wall data comes in, with the rows ordered top to bottom.
		// But for ease of working with Unity we're going to assign these from bottom to top,
		// so that 0,0 is the lower left corner of the map.
		int y = (this.Height) - 1;
		foreach (object rowWallData in wallData)
		{
			string cellsWallData = rowWallData as string;
			if (cellsWallData == null) 
			{
				Debug.Log ("Wall data is not a string, it's " + rowWallData.GetType().ToString () );
				continue;
			}
			for (int x = 0; x < cellsWallData.Length; ++x)
			{
				char c = cellsWallData[x];
				IntVector2 position = new IntVector2(x, y);
				
				if (SerpentConsts.DirectionIndexes.ContainsKey(c)) 
				{				
					List<SerpentConsts.Dir> sides = SerpentConsts.DirectionIndexes[c];
					foreach( SerpentConsts.Dir side in sides )
					{
						CreateWall(position, side);	
					}
				}
			}
			
			--y;
			if (y < 0) { break; }
		}
	}

	private void CreateWall(IntVector2 position, SerpentConsts.Dir side)
	{
		if (position.x < 0 || position.x >= this.Width || position.y < 0 || position.y >= this.Height) 
		{
			Debug.Log("Bad position given for wall");
			return; 
		}
		
		Wall wall = new Wall();
		
		SetupWallLinks(wall, position, side);
	}
	
	private void CreateDoors(List<object> doorsData)
	{		
		// doorData should be a list, each element of which is a list with 3 values - two integers for x,y and a string for direction.
		foreach( object o in doorsData )
		{
			Dictionary<string,object> doorData = o as Dictionary<string,object>;
			if (doorData == null) 
			{ 
				continue; 
			}
			
			int x = doorData.GetInt(SerpentConsts.XKey);
			int y = doorData.GetInt(SerpentConsts.YKey);
			string directionString = doorData.GetString (SerpentConsts.DirectionKey);
			if (string.IsNullOrEmpty(directionString)) 
			{
				continue;
			}

			IntVector2 position = new IntVector2(x, y);
			
			// Read the first character out of the string.  It should be a recognizable mapping to side data.  There
			// should only be one side in question, but the code is stronger handling all cases.
			char c = directionString[0];			
			if (SerpentConsts.DirectionIndexes.ContainsKey(c)) 
			{				
				List<SerpentConsts.Dir> sides = SerpentConsts.DirectionIndexes[c];
				foreach( SerpentConsts.Dir side in sides )
				{
					CreateDoor(position, side);
				}
			}
			
		}
	}
	
	private void CreateDoor(IntVector2 position, SerpentConsts.Dir side)
	{
		if (position.x < 0 || position.x >= this.Width || position.y < 0 || position.y >= this.Height) 
		{
			Debug.Log("Bad position given for door");
			return; 
		}
		
		Door door = new Door(side);
		
		SetupWallLinks(door, position, side);
	}
	
	private void SetupWallLinks( Wall wall, IntVector2 position, SerpentConsts.Dir side)
	{
		this.Cells[position.x,position.y].Walls[(int)side] = wall;
		
		// Connect the other side of the wall as well, provided the other side of the wall is not "out of bounds"
		IntVector2 newPosition = position + SerpentConsts.DirectionVector[(int)side];
		if (newPosition.x < 0 || newPosition.x >= this.Width || newPosition.y < 0 || newPosition.y >= this.Height) { return; }
		
		int oppositeSide = (int)SerpentConsts.OppositeDirection[(int)side];
		this.Cells[newPosition.x,newPosition.y].Walls[ oppositeSide ] = wall;		
	}
}

