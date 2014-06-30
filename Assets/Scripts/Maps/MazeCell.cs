using System.Collections;
using System.Collections.Generic;
using Serpent;

public class MazeCell
{
	public int X { get; private set; }
	public int Y { get; private set; }
	public bool InPlayerZone { get; private set; }

	public Wall[] Walls; 
	
	public List<Direction> UnblockedDirections
	{
		get
		{
			List<Direction> availableDirections = new List<Direction>();
			for (Direction dir = Direction.First; dir <= Direction.Last; ++dir)
			{
				if (IsMotionBlocked(dir))
				{
					continue;
				}
				availableDirections.Add(dir);
			}	
			
			return availableDirections;
		}
	}
	
	public MazeCell(int x, int y)
	{
		this.X = x;
		this.Y = y;
		this.InPlayerZone = false;
		this.Walls = new Wall[(int)Direction.Count];
	}
	
	public void SetInPlayerZone(bool inZone)
	{
		this.InPlayerZone = inZone;
	}
	
	public bool IsMotionBlocked(Direction direction)
	{		
		if (direction < Direction.First || direction > Direction.Last)
		{
			// out of range.
			return false;
		}
		
		if (this.InPlayerZone)
		{
			// Check the level state
			if (Managers.GameState.LevelState == LevelState.Playing)
			{
				return false;
			}
		}
		
		int intDirection = (int)direction;		
		Wall wall = this.Walls[intDirection];
		if (wall is Door)
		{
			Door door = wall as Door;
			return !(door.OpenableFrom(direction));
		}
		else
		{
			return (wall != null);
		}
	}
	

		
}
