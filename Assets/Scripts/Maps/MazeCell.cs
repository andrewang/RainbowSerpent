using System.Collections;

public class MazeCell
{
	public int X { get; set; }
	public int Y { get; set; }

	public MazeWall[] Walls; 

	public MazeCell(int x, int y)
	{
		this.X = x;
		this.Y = y;
		this.Walls = new MazeWall[SerpentConsts.NumDirections];
	}
	
	public bool IsMotionBlocked(SerpentConsts.Dir direction)
	{		
		int intDirection = (int)direction;
		if (intDirection < 0 || intDirection > SerpentConsts.NumDirections)
		{
			// out of range.
			return false;
		}
		
		MazeWall wall = this.Walls[intDirection];
		return (wall != null);
	}

}
