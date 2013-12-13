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
	
	public bool MotionBlocked(SerpentConsts.Dir direction)
	{		
		MazeWall wall = this.Walls[(int)direction];
		return (wall != null);
	}

}
