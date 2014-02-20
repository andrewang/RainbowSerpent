using System.Collections;
using System.Collections.Generic;

public class MazeCell
{
	public int X { get; set; }
	public int Y { get; set; }

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
		this.Walls = new Wall[SerpentConsts.NumDirections];
	}
	
	public bool IsMotionBlocked(SerpentConsts.Dir direction)
	{		
		int intDirection = (int)direction;
		if (intDirection < 0 || intDirection > SerpentConsts.NumDirections)
		{
			// out of range.
			return false;
		}
		
		Wall wall = this.Walls[intDirection];
		if (wall is Door)
		{
			Door door = wall as Door;
			return door.OpenableFrom(direction);
		}
		else
		{
			return (wall != null);
		}
	}
	

		
}
