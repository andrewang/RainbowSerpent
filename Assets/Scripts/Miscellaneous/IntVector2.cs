using System;

public class IntVector2
{
	public int x;
	public int y;

	public IntVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public IntVector2 (int[] v) 
	{
		this.x = v[0];
		this.y = v[1];
	}

	public void Add( IntVector2 other )
	{
		this.x += other.x;
		this.y += other.y;
	}

	public static IntVector2 operator +(IntVector2 v1, IntVector2 v2) 
	{
		return new IntVector2(v1.x + v2.x, v1.y + v2.y);
	}
}


