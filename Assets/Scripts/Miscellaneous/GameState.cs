using System;
public class GameState
{
	public int Level { get; set; }
	public int Score { get; set; }
	public int ExtraSnakes { get; set; }

	public GameState ()
	{
		Reset();
	}
	
	public void Reset ()
	{
		this.Level = 1;
		this.Score = 0;
		this.ExtraSnakes = 2;
	}
}

