using System;
using Serpent;

/// <summary>
/// Game state.  The game state class tracks the state of "the player's game" - what level they are on, what their score is,
/// and how many extra snakes they have left.
/// </summary>
using UnityEngine;


public class GameState
{
	/// <summary>
	/// Gets or sets the level of the map being played.
	/// </summary>
	/// <value>The level.</value>
	private int level = 0;
	public int Level { 
		get
		{
			return this.level;
		}
		set
		{
			this.level = value;
			this.gameSpeed = SerpentConsts.StartingSpeedMultiplier + SerpentConsts.SpeedIncreasePerLevel * (this.level - 1);
		}
	}
	
	public int NumLevels { get; private set; }
	public int NumThemes { get; private set; }
	
	/// <summary>
	/// The player's score (private storage)
	/// </summary>
	private int score;
	/// <summary>
	/// Gets or sets the player's score.
	/// </summary>
	/// <value>The score.</value>
	public int Score 
	{ 
		get
		{
			return this.score;
		}		
		set
		{
			int numBonusLives = this.score / SerpentConsts.ScorePerBonusLife;
			this.score = value;
			// Handle bonus lives here.
			int newBonusLives = this.score / SerpentConsts.ScorePerBonusLife;
			if (newBonusLives > numBonusLives)
			{
				// Score went over a "score for bonus life" threshold.  PRESUMABLY it only ever goes 
				// over one multiple of the "score for bonus life" threshold, but this handles all cases.				
				this.ExtraSnakes += (newBonusLives - numBonusLives);				
			}
		}
	}
	
	/// <summary>
	/// Gets or sets the number of extra snakes.
	/// </summary>
	/// <value>The number of extra snakes.</value>
	public int ExtraSnakes { get; set; }
	
	private float gameSpeed = 0.0f;
	public float GameSpeed 
	{
		get
		{
			if (this.Paused) { return 0.0f; }
			return this.gameSpeed;
		}
	}
	
	public LevelState LevelState { get; set; }
	
	public bool Paused { get; set; }

	public GameState ()
	{
		Reset();
		CountLevels();
		CountThemes();
	}
	
	/// <summary>
	/// Resets all variables to their defaults.
	/// </summary>
	public void Reset ()
	{
		this.Level = 1;
		this.Score = 0;
		this.ExtraSnakes = SerpentConsts.InitialNumPlayerSnakes;
		this.LevelState = LevelState.LevelStart;
		this.Paused = false;
	}
	
	// TODO move this code out of GameState
	public void CountLevels()
	{
		this.NumLevels = CountResources("level");		
	}
	
	public void CountThemes()
	{
		this.NumThemes = CountResources("theme");
	}
	
	private int CountResources(string namePrefix)
	{
		int number = 1;
		while( true )
		{
			UnityEngine.Object obj = Resources.Load(namePrefix + number.ToString());
			if (obj == null)
			{
				break;
			}
			number++;			
		}
		return number - 1;
	}
}

