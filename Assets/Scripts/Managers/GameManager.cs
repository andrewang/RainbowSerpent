using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using SerpentExtensions;
using Serpent;

// TODO REFACTOR into multiple classes.

public class GameManager : MonoBehaviour
{
	#region Serialized Fields
	// Main component systems
	[SerializeField] private MazeController mazeController = null;
	//[SerializeField] private AnimationManager animationManager = null;
	
	// Prefabs used to instantiate creatures
	[SerializeField] private GameObject snakePrefab = null;
	[SerializeField] private GameObject frogPrefab = null;
	[SerializeField] private GameObject playerSnakeConfig = null;
	
	#endregion Serialized Fields
	
	#region Fields
	
	// Instantiated player configurations
	private SnakeConfig playerSnakeConf;
		
	private List<Snake> snakes = new List<Snake>();
	
	private Snake playerSnake = null; 

	private SnakeConfig enemySnakeConf;	
	private int maxNumEnemySnakes;	
	
	private bool updateSnakeColours = false;
		
	private Egg[] eggs = new Egg[2];
	
	private Frog frog = null;
	
	private LevelTheme theme;
	
	private int levelNum;
	
	#endregion Fields
	
	#region Properties
	
	public LevelTheme Theme
	{
		get
		{
			return this.theme;
		}
	}
	
	public Snake PlayerSnake
	{
		get
		{
			return this.playerSnake;
		}
	}
	#endregion Properties
	
	#region Action callbacks
	
	public event Action GameOver;
	
	#endregion Action callbacks
	
	public GameManager ()
	{
	}
	
	#region Setup
	
	public void Setup(int levelNum)
	{
		Managers.GameState.LevelState = LevelState.LevelStart;
		
		this.levelNum = levelNum;
		
		// The level # and theme # to use depends on how many are available!
		int levelToUse = ((levelNum - 1) % Managers.GameState.NumLevels) + 1;
		int themeToUse = ((levelNum - 1) % Managers.GameState.NumThemes) + 1;
		
		// New rule for casual: only have 1 snake per level until that value equals MaxNumEnemySnakes.
		if (Managers.GameState.Difficulty == Difficulty.Easy)
		{
			this.maxNumEnemySnakes = Math.Min(levelNum, SerpentConsts.MaxNumEnemySnakes);
		}
		else
		{
			this.maxNumEnemySnakes = SerpentConsts.MaxNumEnemySnakes;
		}
		
		LoadTheme(themeToUse);
		
		// NOTE: snakes need to be created before input can be configured.  So snakes need to be created here.
		CreateSnakes();	
		
		LoadMapData(levelToUse);
	}
	
	public void Begin()
	{
		Managers.GameClock.Reset();
	
		PlaceSnakes();
		CreateFrogCreationEvent();
	}
	
	public void LoadTheme(int levelNum)
	{
		if (this.theme != null)
		{
			Destroy(this.theme.gameObject);
		}
		UnityEngine.Object prefab = Resources.Load("theme" + levelNum.ToString());
		GameObject obj = Instantiate(prefab) as GameObject;
		this.theme = obj.GetComponent<LevelTheme>();
	}
	
	public void LoadMapData(int levelNum)
	{
		TextAsset mazeTextAsset = Resources.Load("level" + levelNum.ToString()) as TextAsset;
		this.mazeController.SetUp(levelNum, mazeTextAsset, this.theme.WallColour);		

		// Only place snakes once the map screenshot has been made.  So we pass a reference to the Begin method in here to be invoked when CreateScreenShot is done.
		this.mazeController.CreateScreenshot(Begin);		
	}
		
	private void PlaceCreature(Creature creature, int x, int y, Direction direction)
	{		
		Vector3 position = this.mazeController.GetCellCentre(x, y);
		creature.SetInitialLocation(position, direction);		
	}
	
	#endregion Setup
	
	#region Game State
	
	private void StartPlay()
	{
		Managers.GameState.LevelState = LevelState.Playing;
	}
	
	#endregion Game State
	
	#region Update
	
	private void Update()
	{
		if (this.playerSnake == null || this.playerSnake.Dead) { return; }
		
		UpdateSnakes();
		UpdateFrog();
		
		if (Managers.GameState.LevelState != LevelState.Playing) 
		{
			return;
		}
				
		if (this.updateSnakeColours)
		{
			UpdateEnemySnakeColours();
			this.updateSnakeColours = false;
		}			
	}
	
	private void UpdateSnakes()
	{
		Egg playerEgg = GetEgg( Side.Player );
		
		InteractionState playerInteractionState = InteractionState.Nothing;
		
		bool enemySnakeDied = false;
		// Test for snake interactions, based on enemies first
		List<Snake> enemySnakes = GetEnemySnakes();
		for (int i = 0; i < enemySnakes.Count;)
		{		
			Snake enemySnake = enemySnakes[i];
			if (enemySnake.Visible == false) 
			{ 
				++i;
				continue; 
			}
			
			// Do reciprocal tests for snake interaction
			InteractionState tempInteractionState = this.playerSnake.TestForInteraction(enemySnake);
			if (tempInteractionState > playerInteractionState)
			{
				playerInteractionState = tempInteractionState;
			}
			if (tempInteractionState == InteractionState.KilledSomething)
			{
				// By removing a snake from enemySnakes, we move all the snakes after it up one in the list
				// So we continue the loop by reiterating with the same 'i' value as before.
				enemySnakeDied = true;
				enemySnakes.RemoveAt(i);
				continue;
			}
			
			// TODO: snake death should be triggered by the head so that any reason for a snake to die, works.  Currently
			// PlayerSnakeDied is not invoked if the player dies due to egg-laying. 
			InteractionState enemyInteractionState = enemySnake.TestForInteraction(playerSnake);
			if (enemyInteractionState == InteractionState.KilledSomething)
			{
				// player died!  stop everything!
				return;
			}
			
			// Test for frog
			if (this.frog != null)
			{
				InteractionState state = enemySnake.TestForInteraction(this.frog);
				if (enemyInteractionState < state)
				{
					enemyInteractionState = state;
				}
			}
			
			// Test for eating player egg
			if (playerEgg != null)
			{
				InteractionState state = enemySnake.TestForInteraction(playerEgg);				
				if (enemyInteractionState < state)
				{
					enemyInteractionState = state;
				}
			}
			
			enemySnake.Head.UpdateInteractionState(enemyInteractionState);
			
			++i;
		}
		
		if (this.frog != null)
		{
			InteractionState result =  this.playerSnake.TestForInteraction(this.frog);
			if (result > playerInteractionState)
			{
				playerInteractionState = result;
			}
		}		
		
		// Check for player eating enemy egg
		Egg enemyEgg = GetEgg( Side.Enemy );
		bool enemyEggDied = false;
		if (enemyEgg != null)
		{
			InteractionState result = this.playerSnake.TestForInteraction(enemyEgg);
			enemyEggDied = (result == InteractionState.KilledSomething);
			if (result > playerInteractionState)
			{
				playerInteractionState = result;
			}			
		}
		
		if (enemySnakeDied && enemySnakes.Count == 0 && enemyEgg == null)
		{
			// all enemy snakes are dead and no egg exists
			EndLevel();			
		}
		else if (enemyEggDied && enemySnakes.Count == 0)
		{
			// egg eaten and no enemy snakes exist
			EndLevel();			
		}
		
		this.playerSnake.Head.UpdateInteractionState(playerInteractionState);
	}
	
	private void EndLevel()
	{
		// Remove any unlaid player egg (because it might hatch in the start zone!) and
		// put the player under AI control so it can zoom back to the start zone.		
		
		Managers.GameState.LevelState = LevelState.LevelEnd;			
		
		RemoveUnlaidPlayerEgg();
		
		PlayerSnakeController psc = this.playerSnake.Controller as PlayerSnakeController;
		if (psc == null) { return; }
		psc.PlayerControlled = false;
		this.playerSnake.UpdateSpeed();		
	}
	
	private void UpdateFrog()
	{
		// if there is a frog, test against player egg, and test against enemy egg.
		if (this.frog != null)
		{
			for (Side side = Side.Player; side <= Side.Enemy; ++side)
			{
				Egg e = GetEgg(side);
				if (e == null) { continue; }
				
				this.frog.TestForInteraction(e);
			}
		}
	}
	
	#endregion Update
	
	#region Snakes
	
	private void CreateSnakes()
	{
		// Delete any existing snakes first.
		// Cache the length of the player's snake?
		int playerSnakeLength = SerpentConsts.GetSnakeLength(Side.Player, this.levelNum);
		if (this.playerSnake != null && this.playerSnake.Dead == false)
		{
			playerSnakeLength = this.playerSnake.NumSegments;
		}
		DestroySnakes();
		
		this.playerSnakeConf = this.playerSnakeConfig.GetComponent<SnakeConfig>();		
		CreatePlayerSnake(playerSnakeLength);
		
		this.enemySnakeConf = this.theme.EnemySnakeConfig;
		
		List<Snake> enemySnakes = GetEnemySnakes();
		int currNumEnemySnakes = enemySnakes.Count;
		
		int snakeLength = SerpentConsts.GetSnakeLength(Side.Enemy, this.levelNum);
		
		for (int i = 0; i < (this.maxNumEnemySnakes - currNumEnemySnakes); ++i)
		{
			CreateEnemySnake(snakeLength);
		}
	}
	
	private void DestroySnakes()
	{
		this.playerSnake = null; 
		for (int i = 0; i < this.snakes.Count; ++i)
		{
			Snake s = this.snakes[i];
			s.ReturnSegmentsToCache();
			Destroy(s.gameObject);		
		}
		this.snakes.Clear();
	}
	
	private Snake CreatePlayerSnake(int length)
	{
		// Player snake is added to array and also assigned to direct pointer.
		Snake s = CreateSnake(this.playerSnakeConf, length);
		s.SetSpriteDepth(10);
		s.CreatureDied += PlayerSnakeDied;
		this.snakes.Add( s );

		this.playerSnake = s;
		
		PlayerSnakeController psc = s.Controller as PlayerSnakeController;
		psc.SnakeReturnedToStart += PlayerReturnedToStart;
		psc.SnakeExitedStart += PlayerExitedStart;
				
		return this.playerSnake;
	}
		
	private Snake CreateEnemySnake(int length)
	{
		Snake enemySnake = CreateSnake(this.enemySnakeConf, length);
		enemySnake.SetSpriteDepth(1 + this.snakes.Count);
		enemySnake.CreatureDied += EnemySnakeDied;
		enemySnake.ChangeColour(this.theme.EnemySnakeColour);
		this.snakes.Add(enemySnake);
		EnemySnakeAdded();
		return enemySnake;
	}
	
	private void EnemySnakeAdded()
	{
		// We should update snake colours again so the enemy snake is painted the right colour
		this.updateSnakeColours = true;
	}
	
	private Snake CreateSnake(SnakeConfig config, int length)
	{
		Snake snake = SerpentUtils.Instantiate<Snake>(this.snakePrefab, this.mazeController.transform);
		snake.SetUp(this.mazeController, config, length);
		snake.SnakeSegmentsChanged += this.NumSnakeSegmentsChanged;
		snake.SnakeSegmentEaten += this.SnakeSegmentEaten;
		return snake;
	}	
	
	private void PlaceSnakes()
	{
		// Reset state to level start since the player is respawning.
		Managers.GameState.LevelState = LevelState.LevelStart;
		
		// For a subsequent level, in case playersnake has been cleared, reattach it.
		FindPlayerSnake();
		
		// Remove one extra snake on initial placement
		Managers.GameState.ExtraSnakes--;
		
		this.mazeController.PlaceSnake(this.playerSnake, true);
		PlayerSnakeController psc = this.playerSnake.Controller as PlayerSnakeController;
		psc.PlayerControlled = false;
		
		// Every time we place the player snake, reset the lay-egg timer.
		CreateEggLayingEvent(Side.Player);
				
		List<Snake> enemySnakes = GetEnemySnakes();
		for (int i = 0; i < enemySnakes.Count; ++i)
		{	
			Snake enemySnake = enemySnakes[i];
			Managers.GameClock.RegisterEvent(4.0f * i, 
				() => this.mazeController.PlaceSnake(enemySnake, false)
			);
		}				
	}
	
	private void FindPlayerSnake()
	{
		if (this.playerSnake != null) { return; }
		
		List<Snake> playerSnakes = this.snakes.FindAll( s => s.Side == Side.Player );
		if (playerSnakes.Count == 0) { return; }
		
		this.playerSnake = playerSnakes[0];	
	}
	
	private void PlaceSnake(Snake snake, int x, int y, Direction direction, bool justHatched = false)
	{
		// NOTE, now we've got this one and the one in maze controller.  This one is needed for spawning from eggs.
		Vector3 position = this.mazeController.GetCellCentre(x, y);
		snake.SetInitialLocation(position, direction, justHatched);
		snake.Visible = true;
		snake.Controller.StartMoving(direction);		
	}
	
	private void PlayerExitedStart(Snake playerSnake)
	{
		StartPlay();		
	}
	
	private void PlayerReturnedToStart(Snake playerSnake)
	{
		this.playerSnake.Visible = false;
		//this.playerSnake.ReturnToCache();
		//this.playerSnake = null;
		
		// Add one again to player snakes.  This allows new player snakes to increase the total.  When the initial player is spawned that subtracts one.
		Managers.GameState.ExtraSnakes += 1;
		
		// Does a player egg exist?  If so, make it hatch
		Egg egg = GetEgg ( Side.Player );
		if ( egg != null )
		{
			EggHatched( egg );
			egg.Die();
			return;
		}
		// What if the player had an egg growing at the time of level end
		
		// Time to go to the next level.		
		Managers.GameState.Level += 1;		
		//Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Game);
		GameSceneController gsc = Managers.SceneManager.CurrentController as GameSceneController;
		if (gsc == null)
		{
			// should never happen
			return;
		}
		
		// Clear frog
		if (this.frog != null)
		{
			RemoveFrog();
		}
		
		gsc.LoadGameLevel(Managers.GameState.Level);
		
	}
	
	#endregion Snakes
	
	#region Frogs
	
	private void CreateFrogCreationEvent()
	{
		DifficultySettings difficulty = Managers.SettingsManager.GetCurrentSettings();
		float delay =  difficulty.FrogRespawnDelay;
		
		Managers.GameClock.RegisterEvent(delay, CreateFrog, EventIdentifier.CreateFrog);		
	}
	
	private void FrogDied(Creature frog)
	{
		CreateFrogCreationEvent();
	}
	
	private void RemoveFrog()
	{
		if (this.frog != null)
		{
			this.frog.Die();
			this.frog = null;
		}
	}
	
	private void CreateFrog()
	{
		this.frog = SerpentUtils.Instantiate<Frog>(this.frogPrefab, this.mazeController.transform);
		if (this.frog == null) { return; }
		
		this.frog.CreatureDied += FrogDied;
		this.frog.SetUp(this, this.mazeController);
		
		// Randomize frog placement.
		int x = 0;
		int y = 0;
		
		if (UnityEngine.Random.Range(0,2) == 0)
		{
			// top or bottom edge
			if (UnityEngine.Random.Range(0,2) == 0)
			{
				y = this.mazeController.Maze.Height - 1;
			}
			
			// avoid spawning right in the corners
			x = 1 + UnityEngine.Random.Range(0, this.mazeController.Maze.Width - 1);
		}
		else
		{
			// left or right edge
			if (UnityEngine.Random.Range(0,2) == 0)
			{
				x = this.mazeController.Maze.Width - 1;
			}
			
			// avoid spawning right in the corners
			y = 1 + UnityEngine.Random.Range(0, this.mazeController.Maze.Height - 1);			
		}
		
		PlaceCreature(this.frog, x, y, Direction.N);
	}
		
	#endregion Frogs
	
	#region Snake Egg
	
	private bool EggLayingEventExists(Side side)
	{
		EventIdentifier id = (side == Side.Player) ? EventIdentifier.PlayerEggLaying : EventIdentifier.EnemyEggLaying;
		GameEvent e = Managers.GameClock.GetEvent(id);
		return (e != null);
	}
	
	private void CreateEggLayingEvent(Side side)
	{
		if (EggLayingEventExists(side))
		{
			// event exists - abort!
			return;
		}
		
		DifficultySettings difficulty = Managers.SettingsManager.GetCurrentSettings();
		float delay = difficulty.GetEggLayingDelay(side);
		
		EventIdentifier id = (side == Side.Player) ? EventIdentifier.PlayerEggLaying : EventIdentifier.EnemyEggLaying;
		
		if (side == Side.Player)
		{
			Managers.GameClock.RegisterEvent(delay, CheckIfPlayerEggLayingPossible, id);
		}
		else
		{
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
		// time for a random snake to start laying an egg.  But we need a snake with 3+ segments.
		// TODO allow player snake to lay egg when only 2 segments!
		List<Snake> qualifiedSnakes = this.snakes.FindAll( s => s.Side == side && s.NumSegments >= 3 );
		if (qualifiedSnakes.Count == 0)
		{
			// Can't spawn an egg right now.  Reset the timer until later
			CreateEggLayingEvent(side);
			return;
		}
		
		int i = UnityEngine.Random.Range(0, qualifiedSnakes.Count);
		Snake snake = qualifiedSnakes[i];
		Egg e = CreateEgg(snake);
		SetEgg( side, e );
		e.Setup();
	}
	
	private Egg CreateEgg(Snake snake)
	{
		Egg e = snake.CreateEgg();
		
		e.Side = snake.Side;		
		e.CreatureDied += EggDied;
		e.Hatched += EggHatched;
		e.FullyGrown += EggFullyGrown;		
		
		SnakeBody lastSegment = snake.Tail;
		lastSegment.BeginToCreateEgg(e);
		
		return e;
	}
	
	private void EggFullyGrown( Egg egg )
	{					
		egg.SetParent( this.mazeController );
	}
	
	private void EggHatched( Egg egg )
	{
		MazeCell cell = this.mazeController.GetCellForPosition( egg.transform.localPosition );
		List<Direction> availableDirections = cell.UnblockedDirections;
		int randomIndex = UnityEngine.Random.Range (0, availableDirections.Count);
		Direction dir = availableDirections[randomIndex];
		
		Snake newSnake = null;
		int length = SerpentConsts.GetNewlyHatchedSnakeLength( egg.Side );
		if (egg.Side == Side.Enemy)
		{
			newSnake = CreateEnemySnake(length);
		}
		else
		{
			newSnake = CreatePlayerSnake(length);
		}
		EggDied( egg );
		
		PlaceSnake(newSnake, cell.X, cell.Y, dir, true);	
	}
	
	private void EggDied(Creature creature)
	{
		Egg e = creature as Egg;
		if (e == null) { return; }
		
		e.FullyGrown -= this.EggFullyGrown;
		e.Hatched -= this.EggHatched;
		
		SetEgg(e.Side, null);
		
		if (e.Side == Side.Enemy)
		{
			CreateEggLayingEvent(Side.Enemy);
		}
	}
	
	private void RemoveUnlaidPlayerEgg()
	{
		// Do a direct access of the eggs array because GetEgg() screens out eggs which aren't laid
		Egg egg = this.eggs[(int)Side.Player];
		if (egg && egg.IsFullyGrown == false)
		{
			this.playerSnake.Tail.RemoveEgg();
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
		this.eggs[ (int) side ] = egg;
	}
	
	#endregion Snake Eggs
	
	#region Snake Changes
	
	private void NumSnakeSegmentsChanged(Snake snake)
	{
		// Player snake changed in size, or an enemy did.  We need to check whether any snake colours should change
		// (to indicate whether the player can eat that snake or not).
		this.updateSnakeColours = true;
	}
	
	private void SnakeSegmentEaten(Side side, Vector3 position)
	{		
		if (side == Side.Enemy)
		{
			Managers.GameState.Score += SerpentConsts.ScoreForEatingSegment;
		}		
	}
	
	
	#endregion Snake Changes
	
	#region Snake Death
	
	/// <summary>
	/// Handle player snake death
	/// </summary>
	/// <param name="snake">Snake.</param>
	private void PlayerSnakeDied(Creature creature)
	{
		Snake snake = creature as Snake;
		if (snake == null) { return; }
		
		snake.Dead = true;
		
		if (Managers.GameState.ExtraSnakes > 0)
		{			
			// Trigger post-death sequence
			TriggerPlayerDeathSequence();
		}
		else if (this.GameOver != null)			
		{
			// Trigger game-over sequence.
			this.GameOver();
		}
	}
	
	/// <summary>
	/// Handle death of an enemy snake
	/// </summary>
	/// <param name="snake">Snake.</param>
	private void EnemySnakeDied(Creature creature)
	{
		Snake snake = creature as Snake;
		if (snake == null) { return; }
		snake.Dead = true;
		
		snake.SnakeSegmentsChanged -= this.NumSnakeSegmentsChanged;
		snake.ReturnSegmentsToCache();
		this.snakes.Remove(snake);
		Destroy(snake.gameObject);
		
		CreateEggLayingEvent(Side.Enemy);
	}
	
	private void TriggerPlayerDeathSequence()
	{
		if (Managers.GameClock.Paused)
		{
			Debug.Log("Clock unpaused.  Why was this necessary?");
			Managers.GameClock.Paused = false;
		}
		Managers.GameClock.RegisterEvent(3.0f,
			() => PlayerDeathSequence() );
	}
	
	private void PlayerDeathSequence ()
	{
		HandleEggsAfterPlayerDeath();

		// reset stuff		
		ResetSnakes();
		RemoveFrog();
		
		Begin();	
	}
	
	private void HandleEggsAfterPlayerDeath()
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
	
	private void ResetSnakes()
	{
		for( int i = 0; i < this.snakes.Count; ++i )
		{
			this.snakes[i].Reset();
		}
	}
	
	#endregion Snake Death
	
	#region Snake Colours 
	
	private void UpdateEnemySnakeColours()
	{
		if (this.playerSnake.Dead) { return; }
		
		int playerLength = this.playerSnake.NumSegments;
		
		List<Snake> enemySnakes = GetEnemySnakes();
		for( int i = 0; i < enemySnakes.Count; ++i)
		{
			Snake enemySnake = enemySnakes[i];
			if (enemySnake.NumSegments >= playerLength)
			{
				// dangerous colour
				enemySnake.ChangeColour(this.theme.EnemySnakeColour);
			}
			else
			{
				// safe colour
				enemySnake.ChangeColour(this.theme.WeakEnemySnakeColour);
			}
		}		
	}
	
	#endregion Snake Colours
	
	#region External accessors for AI
	
	public List<Snake> GetSnakes()
	{
		return this.snakes;
	}
	
	public List<Snake> GetEnemySnakes()
	{
		List<Snake> enemies = this.snakes.FindAll( s => s.Side == Side.Enemy );
		return enemies;
	}

	#endregion External accessors for AI
	
}
