using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SerpentExtensions;
using MiniJSON;
using System;
using Serpent;

public class Maze : MonoBehaviour 
{
	public int Width { get; set; }
	public int Height { get; set; }
	
	public IntVector2 PlayerStartPosition { get; private set; }
	public Direction PlayerStartFacing { get; private set; }
	
	public IntVector2 PlayerStartZoneEntrance { get; private set; }
	public IntVector2 PlayerStartZoneCentre { get; private set; }
	public IntVector2 PlayerStartZoneExit { get; private set; }
	
	public IntVector2 EnemyStartPosition { get; private set; }
	public Direction EnemyStartFacing { get; private set; }
	
	public MazeCell[,] Cells;
	
	private List<Door> doors = new List<Door>();

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
		List<object> wallData = mazeData.GetObject(SerpentConsts.WallsKey) as List<object>;
		List<object> doorData = mazeData.GetObject(SerpentConsts.DoorsKey) as List<object>;
		List<object> playerStartZoneData = mazeData.GetObject(SerpentConsts.PlayerStartZoneKey) as List<object>;
		
		// Check data and abort if data is missing  Door data is optional so don't cancel if there isn't any
		if (width == 0 || height == 0 || wallData == null || playerStartZoneData == null)
		{ 
			return; 
		}

		// Add border width to the width and height of the map in order to have room around the edge of the maze
		width = width + SerpentConsts.BorderWidth * 2;
		height = height + SerpentConsts.BorderWidth * 2;		
		this.Width = width;
		this.Height = height;

		// Create all the cells
		CreateCells();
		FlagPlayerStartZone(playerStartZoneData);
		
		// Create walls
		CreateWalls(wallData, doorData);
		
		// Mark the "centre position" of the player start
		int x = Math.Min(this.PlayerStartZoneEntrance.x, this.PlayerStartZoneExit.x);
		int y = Math.Min(this.PlayerStartZoneEntrance.y, this .PlayerStartZoneExit.y);
		this.PlayerStartZoneCentre = new IntVector2(x,y);
		
	}
	
	private void CreateCells()
	{
		this.Cells = new MazeCell[this.Width,this.Height];
		for (int y = 0; y < this.Height; ++y)
		{
			for (int x = 0; x < this.Width; ++x)
			{
				this.Cells[x,y] = new MazeCell(x, y);
			}
		}		
	}
	
	private void FlagPlayerStartZone(List<object> playerStartZoneData)
	{
		// Each element in playerZoneData should be a dictionary with a value for "X" and a value for "Y".  Each
		// position corresponds with a cell which should be set as part of the player starting zone. 
		foreach( object o in playerStartZoneData )
		{
			Dictionary<string,object> dict = o as Dictionary<string,object>;
			if (dict == null) { continue; }
			
			int x = dict.GetInt(SerpentConsts.XKey);
			int y = dict.GetInt(SerpentConsts.YKey);
			
			if (x < this.Width && y < this.Height)
			{
				MazeCell cell = this.Cells[x,y];
				cell.SetInPlayerZone(true);
			}
		}
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
				CreateWall(position, Direction.N);
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
				CreateWall(position, Direction.W);
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

	private void CreateWall(IntVector2 position, Direction side)
	{
		if (position.x < 0 || position.x >= this.Width || position.y < 0 || position.y >= this.Height) 
		{
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
		Direction dir = GetDoorDirection( door );
		string special = GetSpecialDoorInfo( door );
		LevelState levelStateRequired = GetDoorLevelStateRequired( door );
		
		// Use the door direction to offset the placement of the door so that it appears in the correct place.
		// In the case of a south-facing door, the y position must be 1 less.
		// In the case of an east-facing door, the x position must be 1 less.
		if (dir == Direction.S)
		{
			position.y += 1;
		}
		else if (dir == Direction.E)
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
		else if (special == "PlayerZoneExit")
		{
			this.PlayerStartZoneExit = position;
		}
		else if (special == "PlayerZoneEntrance")
		{
			// In order to be useful we want this position to be beyond the actual door, INSIDE the player zone, rather 
			// than the position right in front of the player zone. 
			//IntVector2 otherSideOfDoor = GetNextCellPosition(position, dir);
			//this.PlayerStartZoneEntrance = otherSideOfDoor;
			this.PlayerStartZoneEntrance = position;
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
	private Direction GetDoorDirection(object doorInfo)
	{
		Dictionary<string,object> doorDict = doorInfo as Dictionary<string,object>;
		if (doorDict == null)
		{
			return Direction.None;
		}
		
		string dirStr = doorDict.GetString("direction");
		
		if (dirStr.Length == 0)
		{
			return Direction.None;
		}
		
		char c = dirStr[0];
		List<Direction> sides = SerpentConsts.DirectionIndexes[c];
		
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
		
	private LevelState GetDoorLevelStateRequired(object doorInfo)
	{
		Dictionary<string,object> doorDict = doorInfo as Dictionary<string,object>;
		if (doorDict == null)
		{
			return LevelState.None;
		}
		
		string str = doorDict.GetString("levelStateRequired");	
		if (str == null)
		{
			return LevelState.None;
		}
		
		// We have to turn this string into an enum value
		try
		{
			LevelState state = (LevelState) Enum.Parse(typeof(LevelState), str);        
			return state;
		}
		catch( ArgumentException )
		{
			Console.WriteLine("'{0}' is not a member of the LevelState enumeration.", str);			
			return LevelState.None;
		}
	}
	
	private Door CreateDoor(IntVector2 position, Direction side)
	{
		if (position.x < 0 || position.x >= this.Width || position.y < 0 || position.y >= this.Height || side == Direction.None) 
		{
			return null;
		}
		
		Door door = new Door(side);		
		SetupWallLinks(door, position, side);
		this.doors.Add(door);	
		return door;
	}
	
	private void SetupWallLinks( Wall wall, IntVector2 position, Direction side)
	{
		if (position.x < 0 || position.x >= this.Width || position.y < 0 || position.y >= this.Height)
		{
			return;
		}
		
		this.Cells[position.x,position.y].Walls[(int)side] = wall;
		
		// Connect the other side of the wall as well, provided the other side of the wall is not "out of bounds"
		IntVector2 newPosition = position + SerpentConsts.DirectionVector[(int)side];
		if (newPosition.x < 0 || newPosition.x >= this.Width || newPosition.y < 0 || newPosition.y >= this.Height) { return; }
		
		int oppositeSide = (int)SerpentConsts.OppositeDirection[(int)side];
		this.Cells[newPosition.x,newPosition.y].Walls[ oppositeSide ] = wall;		
	}
	
	private IntVector2 GetNextCellPosition(IntVector2 intCellPos, Direction direction)
	{
		IntVector2 newPos = intCellPos;
		IntVector2 step = SerpentConsts.DirectionVector[ (int)direction ];
		newPos += step;
		return newPos;
	}
	
	public void ShowDoors()
	{
		foreach (Door door in this.doors)
		{
			door.Show();
		}
	}
	
	public void HideDoors()
	{
		foreach (Door door in this.doors)
		{
			door.Hide();
		}
	}
}

