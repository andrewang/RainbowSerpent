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
		None = 0,
		LevelStart,
		Playing,
		PlayerDead,
		LevelEnd
	}
	
	public enum Side
	{
		Player = 0,
		Enemy,
		Frog,
	}	
	
	public enum Difficulty
	{
		Easy = 0,
		Classic
	}
	
	public enum InteractionState
	{
		Nothing = 0,
		CloseToSomething,
		BitSomething,
		KilledSomething
	}
	
	public enum EventIdentifier
	{
		None,
		PlayerEggLaying,
		EnemyEggLaying,
		CreateFrog,
		FrogJump,
	}
}

