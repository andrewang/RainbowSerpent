using System;

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
	public int Level { get; set; }
	
	public int NumLevels { get; private set; }
	
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
			int numBonusLives = this.score / SerpentConsts.ScoreForBonusLife;
			this.score = value;
			// Handle bonus lives here.
			int newBonusLives = this.score / SerpentConsts.ScoreForBonusLife;
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
	
	public SerpentConsts.LevelState LevelState { get; set; }

	public GameState ()
	{
		Reset();
		CountLevels();
	}
	
	/// <summary>
	/// Resets all variables to their defaults.
	/// </summary>
	public void Reset ()
	{
		this.Level = 1;
		this.Score = 0;
		this.ExtraSnakes = SerpentConsts.InitialNumPlayerSnakes;
		this.LevelState = SerpentConsts.LevelState.LevelStart;
	}
	
	public void CountLevels()
	{
		int levelNum = 1;
		while( true )
		{
			TextAsset mazeTextAsset = Resources.Load("level" + levelNum.ToString()) as TextAsset;
			if (mazeTextAsset == null)
			{
				break;
			}
			levelNum++;			
		}
		this.NumLevels = levelNum - 1;
	}
}

