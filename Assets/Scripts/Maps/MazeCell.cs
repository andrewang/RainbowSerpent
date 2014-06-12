using System.Collections;
using System.Collections.Generic;

public class MazeCell
{
	public int X { get; private set; }
	public int Y { get; private set; }
	public bool InPlayerZone { get; private set; }

	public Wall[] Walls; 
	
	public List<SerpentConsts.Dir> UnblockedDirections
	{
		get
		{
			List<SerpentConsts.Dir> availableDirections = new List<SerpentConsts.Dir>();
			for (int i = 0; i < SerpentConsts.NumDirections; ++i)
			{
				SerpentConsts.Dir dir = (SerpentConsts.Dir) i;
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
		this.Walls = new Wall[SerpentConsts.NumDirections];
	}
	
	public void SetInPlayerZone(bool inZone)
	{
		this.InPlayerZone = inZone;
	}
	
	public bool IsMotionBlocked(SerpentConsts.Dir direction)
	{		
		int intDirection = (int)direction;
		if (intDirection < 0 || intDirection > SerpentConsts.NumDirections)
		{
			// out of range.
			return false;
		}
		
		if (this.InPlayerZone)
		{
			// Check the level state
			if (Managers.GameState.LevelState == SerpentConsts.LevelState.Playing)
			{
				return false;
			}
		}
		
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
