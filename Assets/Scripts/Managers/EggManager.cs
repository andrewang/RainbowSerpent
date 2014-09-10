using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using SerpentExtensions;
using Serpent;

public class EggManager : MonoBehaviour
{
	#region Serialized Fields
	
	[SerializeField] private MazeController mazeController = null;
	[SerializeField] private GameManager gameManager = null;
	
	#endregion Serialized Fields
	
	#region Fields
		
	private Egg[] eggs = new Egg[2];
	
	#endregion Fields
				
	#region Egg
	
	private bool EggLayingEventExists(Side side)
	{
		EventIdentifier id = (side == Side.Player) ? EventIdentifier.PlayerEggLaying : EventIdentifier.EnemyEggLaying;
		GameEvent e = Managers.GameClock.GetEvent(id);
		return (e != null);
	}
	
	public void CreateEggLayingEvent(Side side, bool initial = false)
	{
		if (EggLayingEventExists(side))
		{
			// event exists - abort!
			return;
		}
		
		DifficultySettings difficulty = Managers.SettingsManager.GetCurrentSettings();
		
		EventIdentifier id = (side == Side.Player) ? EventIdentifier.PlayerEggLaying : EventIdentifier.EnemyEggLaying;
		
		if (side == Side.Player)
		{
			float delay;
			if (initial)
			{
				delay = difficulty.PlayerInitialEggLayingDelay;
			}
			else
			{
				delay = difficulty.PlayerEggLayingDelay;
			}
			Managers.GameClock.RegisterEvent(delay, CheckIfPlayerEggLayingPossible, id);
		}
		else
		{
			float delay = difficulty.EnemyEggLayingDelay;
			Managers.GameClock.RegisterEvent(delay, CheckIfEnemyEggLayingPossible, id);
		}
	}
	
	private void CheckIfPlayerEggLayingPossible()
	{
		CheckIfEggLayingPossible(Side.Player);
	}
	
	private void CheckIfEnemyEggLayingPossible()
	{
		CheckIfEggLayingPossible(Side.Enemy);
	}
	
	private void CheckIfEggLayingPossible(Side side)
	{
		Snake snake = this.gameManager.GetQualifiedEggLayer(side);		
		if (snake == null)
		{
			// Can't spawn an egg right now.  Reset the timer until later
			CreateEggLayingEvent(side);
			return;
		}
		
		
		
		CreateEgg(snake);
	}
	
	public void CreateEgg(Snake snake)
	{
		Egg e = snake.CreateEgg();
		SetEgg( snake.Side, e );		
		
		e.Side = snake.Side;		
		e.CreatureDied += EggDied;
		e.Hatched += EggHatched;
		e.FullyGrown += EggFullyGrown;		
		
		e.Setup();
		
		SnakeBody lastSegment = snake.Tail;
		lastSegment.BeginToCreateEgg(e);
	}
	
	private void EggFullyGrown( Egg egg )
	{					
		egg.SetParent( this.mazeController );
		// set rotation to vertical
		egg.transform.localEulerAngles = new Vector3(0,0,0);
	}
		
	public void EggHatched( Egg egg )
	{
		MazeCell cell = this.mazeController.GetCellForPosition( egg.transform.localPosition );
		List<Direction> availableDirections = cell.UnblockedDirections;
		int randomIndex = UnityEngine.Random.Range (0, availableDirections.Count);
		Direction dir = availableDirections[randomIndex];
		
		this.gameManager.CreateNewlyHatchedSnake(egg.Side, cell, dir);
		
		EggDied( egg );
	}
	
	private void EggDied( Creature creature )
	{
		Egg e = creature as Egg;
		if (e == null) { return; }
		
		e.FullyGrown -= this.EggFullyGrown;
		e.Hatched -= this.EggHatched;
		
		SetEgg(e.Side, null);
		
		CreateEggLayingEvent(e.Side);
	}
	
	public void RemoveUnlaidPlayerEgg()
	{
		// Do a direct access of the eggs array because GetEgg() screens out eggs which aren't laid
		Egg egg = this.eggs[(int)Side.Player];
		if (egg && egg.IsFullyGrown == false)
		{
			this.gameManager.PlayerSnake.Tail.RemoveEgg();
		}
	}
	
	public List<Creature> GetEggs()
	{
		List<Creature> eggs = new List<Creature>();
		
		for (int i = 0; i < this.eggs.Length; ++i)
		{
			if (this.eggs[i] == null || this.eggs[i].IsFullyGrown == false) { continue; }
			
			eggs.Add( this.eggs[i] );
		}
		return eggs;
	}
	
	public Egg GetEgg( Side side )
	{
		int iSide = (int) side;
		if (this.eggs[iSide] == null || this.eggs[iSide].IsFullyGrown == false)
		{
			return null;
		}
		return this.eggs[ (int) side ];
	}
	
	private void SetEgg( Side side, Egg egg )
	{
		int intSide = (int) side;
		if (this.eggs[intSide] != null)
		{
			// This object should be destroyed!  Though we shouldn't be creating an egg at the same time as 
			// one already exists.
			Destroy(this.eggs[intSide].gameObject);
		}
		this.eggs[ (int) side ] = egg;
	}
	
	#endregion Snake Eggs
	
	#region Snake Death
	
	public void HandleEggsAfterPlayerDeath()
	{
		for( int side = 0; side <= (int) Side.Enemy; ++side )
		{
			Egg e = this.eggs[side];
			if (e == null) { continue; }
			
			if (side == (int)Side.Enemy)
			{				
				EggHatched(e);
			}
			e.Die();
		}
	}
		
	#endregion Snake Death
	
	
}
