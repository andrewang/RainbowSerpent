using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerpentExtensions;
using MiniJSON;
using System;

public class Maze : MonoBehaviour 
{
	public int Width { get; set; }
	public int Height { get; set; }
	
	public IntVector2 PlayerStartPosition
	{ 
		get; private set;
	}
	public SerpentConsts.Dir PlayerStartFacing
	{
		get; private set;
	}

	public IntVector2 EnemyStartPosition
	{ 
		get; private set;
	}
	public SerpentConsts.Dir EnemyStartFacing
	{
		get; private set;
	}
	
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

		// Add 2 to the width and height of the map in order to have room outside the maze for frogs to 
		// actually be tracked.
		width = width + 2;
		height = height + 2;
		Debug.Log("Maze width " + width + " and height " + height);
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

		CreateWalls(wallData, doorData);
		
	}

	private void CreateWalls(List<object> wallData, List<object> doorData)
	{		
		// Create walls. NOTE: the wall data comes in, with the rows ordered top to bottom.
		// But for ease of working with Unity, in memory these will be organized so that
		// so that 0,0 is the lower left corner of the map, not the top left.
		int y = (this.Height) - 2;
		bool horizontal = true;
		foreach (object rowData in wallData)
		{
			string rowDataString = rowData as string;
			if (rowDataString == null) 
			{
				Debug.Log ("Wall data is not a string, it's " + rowData.GetType().ToString () );
				continue;
			}
			
			// Alternate reading data for horizontal and vertical walls. Only go to the next y after
			// reading in the vertical data
			if (horizontal)
			{
				ReadHorizontalWalls(rowDataString, doorData, y);
				horizontal = false;
			}
			else
			{
				ReadVerticalWalls(rowDataString, doorData, y);
				--y;
				if (y < 0) { break; }
				horizontal = true;
			}
			
		}
	}
	
	private void ReadHorizontalWalls(string wallData, List<object> doorData, int y)
	{		
		// Look at every second character in wallData, starting with the second.
		// If there is a -, add a vertical wall
		// If there is a D, add a door
		
		int wallDataSize = wallData.Length;
		for (int x = 1; x < this.Width - 1; ++x)
		{
			int wallDataLoc = (x - 1) * 2 + 1;
			if (wallDataLoc >= wallDataSize)
			{
				break;
			}
			
			char wallInfo = wallData[wallDataLoc];
			if (wallInfo == '-')
			{
				// Add a wall on the west side of this cell
				IntVector2 position = new IntVector2(x, y);
				// Does position need to be deleted or will it be GC'd?
				CreateWall(position, SerpentConsts.Dir.N);
			}
			else if (wallInfo == 'D')
			{
				IntVector2 position = new IntVector2(x, y);				
				int doorIndex = GetDoorIndex( wallData[wallDataLoc+1] );
				CreateDoor(position, doorData, doorIndex ); 
			}
		}				
	}
	
	
	private void ReadVerticalWalls(string wallData, List<object> doorData, int y)
	{
		// Look at every second character in wallData, starting with the first.
		// If there is a |, add a vertical wall
		// If there is a D, add a door
		int wallDataSize = wallData.Length;
		for (int x = 1; x < this.Width; ++x)
		{
			int wallDataLoc = (x - 1) * 2;
			if (wallDataLoc >= wallDataSize)
			{
				break;
			}
			
			char wallInfo = wallData[wallDataLoc];
			if (wallInfo == '|')
			{
				// Add a wall on the west side of this cell
				IntVector2 position = new IntVector2(x, y);
				// Does position need to be deleted or will it be GC'd?
				CreateWall(position, SerpentConsts.Dir.W);
			}
			else if (wallInfo == 'D')
			{
				// is this -1 predicated on an east facing door? If so, it's really being hardcoded... and i think it is.
				IntVector2 position = new IntVector2(x, y);				
				int doorIndex = GetDoorIndex( wallData[wallDataLoc+1] );
				CreateDoor(position, doorData, doorIndex ); 				
			}
			
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
	
	private void CreateDoor(IntVector2 position, List<object> doorData, int doorIndex)
	{
		if (doorIndex < 0 || doorIndex >= doorData.Count)
		{
			return;
		}
		
		object door = doorData[doorIndex];
		SerpentConsts.Dir dir = GetDoorDirection( door );
		string special = GetSpecialDoorInfo( door );
		SerpentConsts.LevelState levelStateRequired = GetDoorLevelStateRequired( door );
		
		// Use the door direction to offset the placement of the door so that it appears in the correct place.
		// In the case of a south-facing door, the y position must be 1 less.
		// In the case of an east-facing door, the x position must be 1 less.
		if (dir == SerpentConsts.Dir.S)
		{
			position.y += 1;
		}
		else if (dir == SerpentConsts.Dir.E)
		{
			position.x -= 1;
		}
		
		Door d = CreateDoor(position, dir);
		d.LevelStateRequired = levelStateRequired;
		if (special == "PlayerStart")
		{
			this.PlayerStartFacing = dir;
			this.PlayerStartPosition = position;
		}
		else if (special == "EnemyStart")
		{
			this.EnemyStartFacing = dir;
			this.EnemyStartPosition = position;
		}
	}
	
	private int GetDoorIndex(char doorInfoChar)
	{
		int val = -1;
		if (doorInfoChar != ' ')
		{
			string doorInfoStr = doorInfoChar.ToString();
			// NOTE: doors are numbered starting with 1 in the level data
			val = Convert.ToInt32(doorInfoStr) - 1;
		}
		return val;
	}
	
	// Extract any specified door direction from the object doorInfo
	private SerpentConsts.Dir GetDoorDirection(object doorInfo)
	{
		Dictionary<string,object> doorDict = doorInfo as Dictionary<string,object>;
		if (doorDict == null)
		{
			return SerpentConsts.Dir.None;
		}
		
		string dirStr = doorDict.GetString("direction");
		
		if (dirStr.Length == 0)
		{
			return SerpentConsts.Dir.None;
		}
		
		char c = dirStr[0];
		List<SerpentConsts.Dir> sides = SerpentConsts.DirectionIndexes[c];
		
		return sides[0];
	}
	
	private string GetSpecialDoorInfo(object doorInfo)
	{
		Dictionary<string,object> doorDict = doorInfo as Dictionary<string,object>;
		if (doorDict == null)
		{
			return "";
		}
		
		string outStr = doorDict.GetString("special");
		return outStr;		
	}	
		
	private SerpentConsts.LevelState GetDoorLevelStateRequired(object doorInfo)
	{
		Dictionary<string,object> doorDict = doorInfo as Dictionary<string,object>;
		if (doorDict == null)
		{
			return SerpentConsts.LevelState.None;
		}
		
		string str = doorDict.GetString("levelStateRequired");	
		if (str == null)
		{
			return SerpentConsts.LevelState.None;
		}
		
		// We have to turn this string into an enum value
		try
		{
			SerpentConsts.LevelState state = (SerpentConsts.LevelState) Enum.Parse(typeof(SerpentConsts.LevelState), str);        
			return state;
		}
		catch( ArgumentException )
		{
			Console.WriteLine("'{0}' is not a member of the LevelState enumeration.", str);			
			return SerpentConsts.LevelState.None;
		}
	}
	
	private Door CreateDoor(IntVector2 position, SerpentConsts.Dir side)
	{
		if (position.x < 0 || position.x >= this.Width || position.y < 0 || position.y >= this.Height || side == SerpentConsts.Dir.None) 
		{
			Debug.Log("Bad position given for door");
			return null;
		}
		
		Door door = new Door(side);		
		SetupWallLinks(door, position, side);		
		return door;
	}
	
	private void SetupWallLinks( Wall wall, IntVector2 position, SerpentConsts.Dir side)
	{
		if (position.x < 0 || position.x >= this.Width || position.y < 0 || position.y >= this.Height)
		{
			Debug.Log("Bad position in SetupWallLinks!");
			return;
		}
		
		this.Cells[position.x,position.y].Walls[(int)side] = wall;
		
		// Connect the other side of the wall as well, provided the other side of the wall is not "out of bounds"
		IntVector2 newPosition = position + SerpentConsts.DirectionVector[(int)side];
		if (newPosition.x < 0 || newPosition.x >= this.Width || newPosition.y < 0 || newPosition.y >= this.Height) { return; }
		
		int oppositeSide = (int)SerpentConsts.OppositeDirection[(int)side];
		this.Cells[newPosition.x,newPosition.y].Walls[ oppositeSide ] = wall;		
	}
}

