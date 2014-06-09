using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using SerpentExtensions;

public class GameManager : MonoBehaviour
{
	#region Serialized Fields
	// Main component systems
	[SerializeField] private MazeController mazeController = null;
	[SerializeField] private AnimationManager animationManager = null;
	
	// Prefabs used to instantiate creatures
	[SerializeField] private GameObject snakePrefab = null;
	[SerializeField] private GameObject eggPrefab = null;
	[SerializeField] private GameObject frogPrefab = null;
	[SerializeField] private GameObject playerSnakeConfig = null;
	[SerializeField] private GameObject enemySnakeConfig = null;
	
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
	private DateTime[] eggTimers = new DateTime[2];
	
	private LevelTheme theme;
	
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
	
	public GameManager ()
	{
	}
	
	#region Setup
	
	public void Setup(int levelNum)
	{
		// Cap levelNum according to the maximum number of levels.
		// We don't want a zero so calculate this with a loop and subtraction, only
		// considering values greater than NumLevels as a problem.
		while (levelNum > Managers.GameState.NumLevels)
		{
			levelNum -= Managers.GameState.NumLevels;
		}
		
		LoadTheme(levelNum);
		LoadMapData(levelNum);
		SetTimers();
		CreateSnakes();		
	}
	
	public void Begin()
	{
		StartCoroutine( PlaceSnakes() );
	}
	
	public void LoadTheme(int levelNum)
	{
		UnityEngine.Object prefab = Resources.Load("theme" + levelNum.ToString());
		GameObject obj = Instantiate(prefab) as GameObject;
		this.theme = obj.GetComponent<LevelTheme>();
	}
	
	public void LoadMapData(int levelNum)
	{
		TextAsset mazeTextAsset = Resources.Load("level" + levelNum.ToString()) as TextAsset;
		this.mazeController.SetUp(levelNum, mazeTextAsset, this.theme.WallColour);		
	}
	
	private void SetTimers()
	{		
		for (int i = 0; i < this.eggs.Length; ++i)
		{
			this.eggTimers[i] = new DateTime(0);
		}
	}
	
	private void PlaceCreature(Creature creature, int x, int y, SerpentConsts.Dir direction)
	{		
		Vector3 position = this.mazeController.GetCellCentre(x, y);
		creature.SetInitialLocation(position, direction);		
	}
	
	#endregion Setup
	
	#region Game State
	
	private void StartPlay()
	{
		Managers.GameState.LevelState = SerpentConsts.LevelState.Playing;
	}
	
	#endregion Game State
	
	#region Update
	
	private void Update()
	{
		if (this.playerSnake == null || this.playerSnake.Dead) { return; }
		
		UpdateSnakes();
		
		if (Managers.GameState.LevelState != SerpentConsts.LevelState.Playing) 
		{
			// NB make player eggs hatch...?
			return;
		}
				
		if (this.updateSnakeColours)
		{
			UpdateEnemySnakeColours();
			this.updateSnakeColours = false;
		}
		
		if (GetEgg(SerpentConsts.Side.Enemy) == null)
		{
			List<Snake> enemySnakes = GetEnemySnakes();
			if (enemySnakes.Count < this.maxNumEnemySnakes)
			{
				HandleEggs(SerpentConsts.Side.Enemy);
			}
		}
		
		if (GetEgg(SerpentConsts.Side.Player) == null && this.playerSnake != null)
		{		
			HandleEggs(SerpentConsts.Side.Player);
		}		
	}
	
	private void UpdateSnakes()
	{
		Egg playerEgg = GetEgg( SerpentConsts.Side.Player );
		
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
			bool enemyDies = this.playerSnake.TestForInteraction(enemySnake);
			if (enemyDies)
			{
				// By removing a snake from enemySnakes, we move all the snakes after it up one in the list
				// So we continue the loop by reiterating with the same 'i' value as before.
				enemySnakeDied = true;
				enemySnakes.RemoveAt(i);
				continue;
			}
			
			// TODO: snake death should be triggered by the head so that any reason for a snake to die, works.  Currently
			// PlayerSnakeDied is not invoked if the player dies due to egg-laying. 
			bool playerDies = enemySnake.TestForInteraction(playerSnake);
			if (playerDies)
			{
				// stop everything!
				return;
			}
			
			// Test for eating player egg
			if (playerEgg != null)
			{
				enemySnake.TestForInteraction(playerEgg);				
			}
			
			++i;
		}
		
		// Check for player eating enemy egg
		Egg enemyEgg = GetEgg( SerpentConsts.Side.Enemy );
		bool enemyEggDied = false;
		if (enemyEgg != null)
		{
			enemyEggDied = this.playerSnake.TestForInteraction(enemyEgg);
		}
		
		if ( (enemySnakeDied || enemyEggDied) && enemySnakes.Count == 0 && enemyEgg == null)
		{
			// all enemy snakes are dead and no egg exists
			Managers.GameState.LevelState = SerpentConsts.LevelState.LevelEnd;
		}
	}
	
	private void UpdateFrog()
	{
		// if there is a frog, test against player egg, and test against enemy egg.
	}
	
	#endregion Update
	
	#region Snakes
	
	private void CreateSnakes()
	{
		this.playerSnakeConf = this.playerSnakeConfig.GetComponent<SnakeConfig>();		
		CreatePlayerSnake(SerpentConsts.PlayerSnakeLength);
		
		this.enemySnakeConf = this.enemySnakeConfig.GetComponent<SnakeConfig>();			
		this.maxNumEnemySnakes = SerpentConsts.MaxNumEnemySnakes;
		for (int i = 0; i < this.maxNumEnemySnakes; ++i)
		{
			CreateEnemySnake(SerpentConsts.EnemySnakeLength);
		}
	}
	
	private Snake CreatePlayerSnake(int length)
	{
		// Player snake is added to array and also assigned to direct pointer.
		// TODO does this need to be changed for player baby snake and player snake being onscreen at the same time? 
		// can they be together on map at the same time or not?
		Snake s = CreateSnake(this.playerSnakeConf, length);
		s.Side = SerpentConsts.Side.Player;
		s.CreatureDied += PlayerSnakeDied;
		this.snakes.Add( s );

		this.playerSnake = s;
		this.playerSnake.ChangeColour(this.theme.PlayerSnakeColour);
		
		PlayerSnakeController psc = s.Controller as PlayerSnakeController;
		psc.SnakeReturnedToStart += PlayerReturnedToStart;
				
		return this.playerSnake;
	}
		
	private Snake CreateEnemySnake(int length)
	{
		Snake enemySnake = CreateSnake(this.enemySnakeConf, length);
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
	
	// testing method
	/*
	private void PlaceSnakesInstantly()
	{
		PlaceSnake(this.playerSnake, 1, 0, SerpentConsts.Dir.E);
		
		for (int i = 0; i < this.enemySnakes.Count; ++i)
		{		
			PlaceSnake(this.enemySnakes[i], 8, 12, SerpentConsts.Dir.W);
		}	
	}
	*/
	
	private IEnumerator PlaceSnakes()
	{
		Managers.GameState.LevelState = SerpentConsts.LevelState.LevelStart;
		
		this.mazeController.PlaceSnake(this.playerSnake, true);
		PlayerSnakeController psc = this.playerSnake.Controller as PlayerSnakeController;
		psc.PlayerControlled = false;				
		yield return new WaitForSeconds(5.0f);		
		
		psc.PlayerControlled = true;
		Managers.GameState.LevelState = SerpentConsts.LevelState.Playing;
		
		// TODO: set level state to LevelEnd if the player wins.
		
		List<Snake> enemySnakes = GetEnemySnakes();
		for (int i = 0; i < enemySnakes.Count; ++i)
		{	
			this.mazeController.PlaceSnake(enemySnakes[i], false);	
			yield return new WaitForSeconds(5.0f);
		}		
	}
	
	private void PlaceSnake(Snake snake, int x, int y, SerpentConsts.Dir direction)
	{
		// NOTE, now we've got this one and the one in maze controller.  This one is needed for spawning from eggs.
		Vector3 position = this.mazeController.GetCellCentre(x, y);
		Debug.Log("Adding snake at (" + x + "," + y + "): " + position.x + "," + position.y);
		snake.SetInitialLocation(position, direction);
		snake.Visible = true;
		snake.Controller.StartMoving(direction);		
	}
	
	private void PlayerReturnedToStart(Snake playerSnake)
	{
		// time to go to the next level.
		this.playerSnake.ReturnToCache();
		this.playerSnake.Visible = false;
		this.playerSnake = null;
		Managers.GameState.Level += 1;
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Game);
	}
	
	#endregion Snakes
	
	#region Frogs
	
	private void CreateFrog()
	{
		Frog frog = SerpentUtils.Instantiate<Frog>(this.frogPrefab, this.mazeController.transform);
		if (frog == null) { return; }
		
		int x = 5;
		int y = 5;
		
		PlaceCreature(frog, x, y, SerpentConsts.Dir.N);
	}
	
	#endregion Frogs
	
	#region Snake Eggs	

	
	private void HandleEggs(SerpentConsts.Side side)
	{
		int intSide = (int) side;
		if (this.eggTimers[intSide].Ticks == 0)
		{
			SetEggTimer(side);
		}
		else if (DateTime.Now > this.eggTimers[intSide])
		{
			// time for a random enemy snake to start laying an egg.  But we need a snake with 3+ segments.
			List<Snake> qualifiedSnakes = this.snakes.FindAll( s => s.Side == side && s.NumSegments >= 3 );
			if (qualifiedSnakes.Count == 0)
			{
				// Can't spawn an egg.  Reset the timer.
				SetEggTimer(side);
				return;
			}
			
			int i = UnityEngine.Random.Range( 0, qualifiedSnakes.Count );
			Snake snake = qualifiedSnakes[i];
			Egg e = LayEgg(snake);
			SetEgg( side, e );		
			
			// Enemy snakes have a timed hatching period, while player snakes hatch at the end of the level.
			if (side == SerpentConsts.Side.Enemy)
			{
				e.SetHatchingTime( SerpentConsts.EnemyEggHatchingTime );			                
			}
			
			// reset the timer for next time.
			ResetEggTimer(side);
		}		
	}
	
	private void SetEggTimer(SerpentConsts.Side side)
	{
		this.eggTimers[(int)side] = DateTime.Now + SerpentConsts.GetEggLayingFrequency(side);				
	}
	
	private void ResetEggTimer(SerpentConsts.Side side)
	{
		this.eggTimers[(int)side] = new DateTime(0);				
	}
	
	private Egg LayEgg(Snake snake)
	{
		Egg e = CreateEgg();
		
		e.Side = snake.Side;
		e.CreatureDied += EggDied;
		SnakeBody lastSegment = snake.Tail;
		lastSegment.BeginToCreateEgg(e);			
		return e;
	}
	
	private Egg CreateEgg()
	{
		Egg egg = SerpentUtils.Instantiate<Egg>(this.eggPrefab, this.mazeController.transform);
		egg.Hatched += EggHatched;
		egg.FullyGrown += EggFullyGrown;		
		return egg;
	}
	
	private void EggFullyGrown( Egg egg )
	{			
		egg.SetParent( this.mazeController );
	}
	
	private void EggHatched( Egg egg )
	{
		MazeCell cell = this.mazeController.GetCellForPosition( egg.transform.localPosition );
		List<SerpentConsts.Dir> availableDirections = cell.UnblockedDirections;
		int randomIndex = UnityEngine.Random.Range (0, availableDirections.Count);
		SerpentConsts.Dir dir = availableDirections[randomIndex];
		
		Snake newSnake = null;
		int length = SerpentConsts.GetNewlyHatchedSnakeLength( egg.Side );
		if (egg.Side == SerpentConsts.Side.Enemy)
		{
			newSnake = CreateEnemySnake(length);
		}
		else
		{
			newSnake = CreatePlayerSnake(length);
		}
		EggDied( egg );
		
		PlaceSnake(newSnake, cell.X, cell.Y, dir);	
	}
	
	private void EggDied(Creature creature)
	{
		Egg e = creature as Egg;
		if (e == null) { return; }
		
		e.FullyGrown -= this.EggFullyGrown;
		e.Hatched -= this.EggHatched;
		
		SetEgg(e.Side, null);
	}
	
	
	public List<Creature> GetEggs()
	{
		List<Creature> eggs = new List<Creature>();
		
		for (int i = 0; i < this.eggs.Length; ++i)
		{
			eggs.Add( this.eggs[i] );
		}
		return eggs;
	}
	
	public Egg GetEgg( SerpentConsts.Side side )
	{
		return this.eggs[ (int) side ];
	}
	
	private void SetEgg( SerpentConsts.Side side, Egg e )
	{
		this.eggs[ (int) side ] = e;
	}
	
	#endregion Snake Eggs
	
	#region Snake Changes
	
	private void NumSnakeSegmentsChanged(Snake snake)
	{
		// Player snake changed in size, or an enemy did.  We need to check whether any snake colours should change
		// (to indicate whether the player can eat that snake or not).
		this.updateSnakeColours = true;
	}
	
	private void SnakeSegmentEaten(Vector3 position)
	{
		//this.animationManager.PlayRandomAnimation(position);
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
			StartCoroutine(PlayerDeathSequence());
		}
		else
		{
			// Trigger game-over sequence.
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
		this.snakes.Remove(snake);
	}
	
	private IEnumerator PlayerDeathSequence()
	{
		// Remove any eggs?
		
		yield return new WaitForSeconds(3.0f);
		
		Managers.GameState.ExtraSnakes--;		
		
		for( int i = 0; i < this.snakes.Count; ++i )
		{
			this.snakes[i].Reset();
		}
				
		StartCoroutine( PlaceSnakes() );
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
		List<Snake> enemies = this.snakes.FindAll( s => s.Side == SerpentConsts.Side.Enemy );
		return enemies;
	}

	#endregion External accessors for AI
	
}
