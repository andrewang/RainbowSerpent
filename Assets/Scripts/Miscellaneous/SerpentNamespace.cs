namespace Serpent
{
	public enum Direction
	{
		N = 0,
		E,
		S,
		W,
		None,
		First = N,
		Last = W,
		Count = (W + 1)
	}
	
	public enum LevelState
	{
		None,
		LevelStart,
		Playing,
		LevelEnd
	}
	
	public enum Side
	{
		Player,
		Enemy,
		Frog,
	}	
	
	public enum Difficulty
	{
		Easy,
		Classic
	}
	
}

