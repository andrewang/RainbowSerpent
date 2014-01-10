using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameSceneController : RSSceneController
{
	#region Serialized Fields
	[SerializeField] private MazeController mazeController = null;
	[SerializeField] private UISprite background = null;
	[SerializeField] private GameObject snakePrefab = null;
	[SerializeField] private GameObject eggPrefab = null;
	[SerializeField] private GameObject playerSnakeConfig = null;
	[SerializeField] private GameObject enemySnakeConfig = null;
	[SerializeField] private LevelTheme theme = null;
	[SerializeField] private UILabel[] labels;
	[SerializeField] private UILabel levelLabel;
	[SerializeField] private UILabel scoreLabel;
	[SerializeField] private UILabel livesLabel;
	
	private SnakeConfig playerSnakeConf;
	private SnakeConfig enemySnakeConf;
	
	#endregion Serialized Fields
	
	private Snake playerSnake = null; 
	private List<Snake> enemySnakes = new List<Snake>();
	private int maxNumEnemySnakes;	
	private bool updateSnakeColours = false;

	private DateTime playerEggTimer;
	private Egg playerEgg;
	private DateTime enemyEggTimer;
	private Egg enemyEgg;

	#region Verify Serialize Fields

	override public void VerifySerializeFields()
	{
		if (this.mazeController == null) { Debug.LogError("GameSceneController: mazeController is null"); }
	}

	#endregion Verify Serialize Fields

	override public void OnLoad()
	{
		Debug.Log("Game scene finally loaded");
		LoadGameLevel(Managers.GameState.Level);
	}

	private void LoadGameLevel(int levelNum)
	{
		// TODO Clear the map
		
		// Empty creature dict

		// Load in new map data.
		TextAsset mazeTextAsset = Resources.Load("level1") as TextAsset;
		this.mazeController.SetUp(mazeTextAsset, this.theme.WallColour);
		
		// Set some initial colours
		foreach( UILabel label in this.labels )
		{
			label.color = this.theme.TextColour;
		}
		this.background.color = this.theme.BackgroundColour;
		
		// Create creatures
		this.playerSnakeConf = this.playerSnakeConfig.GetComponent<SnakeConfig>();
		this.enemySnakeConf = this.enemySnakeConfig.GetComponent<SnakeConfig>();	
		
		CreatePlayerSnake(SerpentConsts.PlayerSnakeLength);
		
		this.maxNumEnemySnakes = SerpentConsts.MaxNumEnemySnakes;
		
		for (int i = 0; i < this.maxNumEnemySnakes; ++i)
		{
			CreateEnemySnake(SerpentConsts.NormalEnemySnakeLength);
		}
		
		this.enemyEggTimer = new DateTime(0);
		this.playerEggTimer = new DateTime(0);
		
		StartCoroutine( PlaceSnakes() );
	}
	
	private IEnumerator PlaceSnakes()
	{
		PlaceSnake(this.playerSnake, 1, 0, SerpentConsts.Dir.E);
		
		for (int i = 0; i < this.maxNumEnemySnakes; ++i)
		{		
			PlaceSnake(this.enemySnakes[i], 8, 12, SerpentConsts.Dir.W);
			yield return new WaitForSeconds(5.0f);
		}	
	}
	
	private void CreatePlayerSnake(int length)
	{
		this.playerSnake = CreateSnake(this.playerSnakeConf, length);
		this.playerSnake.ChangeColour(this.theme.PlayerSnakeColour);
		
	}
		
	private Snake CreateEnemySnake(int length)
	{
		Snake enemySnake = CreateSnake(this.enemySnakeConf, length);
		enemySnake.ChangeColour(this.theme.EnemySnakeColour);
		this.enemySnakes.Add(enemySnake);
		return enemySnake;
	}
	
	private Snake CreateSnake(SnakeConfig config, int length)
	{
		Snake snake = SerpentUtils.SerpentInstantiate<Snake>(this.snakePrefab, this.mazeController.transform);
		snake.SetUp(this.mazeController, config, length);
		snake.SnakeSegmentsChanged += this.NumSnakeSegmentsChanged;
		return snake;
	}
	
	private void PlaceSnake(Snake snake, int x, int y, SerpentConsts.Dir direction)
	{
		Vector3 position = this.mazeController.GetCellCentre(x, y);
		Debug.Log("Adding snake at (" + x + "," + y + "): " + position.x + "," + position.y);
		snake.SetInitialLocation(position, direction);
		snake.Visible = true;
		snake.Controller.StartMoving(direction);		
	}
	
	private Egg CreateEgg()
	{
		Egg egg = SerpentUtils.SerpentInstantiate<Egg>(this.eggPrefab, this.mazeController.transform);
		egg.Hatched += EggHatched;
		return egg;
	}
	
	/*
	private Egg CreateEgg(int x, int y, SerpentConsts.Dir direction)
	{
		Egg egg = SerpentUtils.SerpentInstantiate<Egg>(this.eggPrefab, this.mazeController.transform);
		egg.Hatched += EggHatched;
		PlaceCreature(egg, x, y, direction);
		return egg;
	}
	*/
	
	private void EggFullyGrown( SnakeSegment segment, Egg egg )
	{
		// place egg in the map cell of the snake segment
		egg.transform.parent = this.mazeController.transform;
		egg.transform.localPosition = segment.transform.localPosition;
	}
	
	private void EggHatched( Egg egg )
	{
		MazeCell cell = this.mazeController.GetCellForPosition( egg.transform.localPosition );
		List<SerpentConsts.Dir> availableDirections = cell.UnblockedDirections;
		int randomIndex = UnityEngine.Random.Range (0, availableDirections.Count);
		SerpentConsts.Dir dir = availableDirections[randomIndex];
		
		Snake newSnake = null;
		if (egg == this.enemyEgg)
		{
			newSnake = CreateEnemySnake(SerpentConsts.SmallEnemySnakeLength);
			
			Destroy (egg.gameObject);
			this.enemyEgg = null;
		}
		
		PlaceSnake(newSnake, cell.X, cell.Y, dir);
		
	}
	
	private void PlaceCreature(Creature creature, int x, int y, SerpentConsts.Dir direction)
	{		
		Vector3 position = this.mazeController.GetCellCentre(x, y);
		creature.SetInitialLocation(position, direction);		
	}

	#region Update
	
	private void Update()
	{
		if (this.playerSnake.Dead) { return; }
		
		// Test for snake interactions, based on player first
		for( int i = 0; i < this.enemySnakes.Count; )
		{		
			Snake enemySnake = this.enemySnakes[i];
			if (enemySnake.Visible == false) 
			{ 
				++i;
				continue; 
			}
			
			bool enemyDies = this.playerSnake.TestForInteraction(enemySnake);
			if (enemyDies)
			{
				EnemySnakeDied(enemySnake);
				// By removing a snake from enemySnakes, we move all the snakes after it up one in the list
				// So we continue the loop by reiterating with the same 'i' value as before.
				continue;
			}
			bool playerDies = enemySnake.TestForInteraction(playerSnake);
			if (playerDies)
			{
				PlayerSnakeDied(playerSnake);
				break;
			}
			++i;
		}
		
		// Check for player eating enemy egg
		if (this.enemyEgg != null)
		{
			bool ateEgg = this.playerSnake.TestForInteraction(this.enemyEgg);
			if (ateEgg)
			{
				Destroy(this.enemyEgg.gameObject);
				this.enemyEgg = null;
			}
		}
		
		/*
		foreach( Creature.CreatureCategory category in this.creaturesDict.Keys )
		{
			List<Creature> creatures = this.creaturesDict[category];
			for (Creature creature in creatures)
			{
			}
		}
		*/
		
		if (this.updateSnakeColours)
		{
			UpdateEnemySnakeColours();
			this.updateSnakeColours = false;
		}
		
		if (this.enemyEgg == null && this.enemySnakes.Count < this.maxNumEnemySnakes)
		{
			HandleEnemyEggs();
		}
		/*
		if (this.playerEgg == null && this.playerSnake != null)
		{		
			HandlePlayerEggs();
		}
		*/
		UpdateText();
	}
	
	private void HandleEnemyEggs()
	{
		if (this.enemyEggTimer.Ticks == 0)
		{
			// set the timer.
			this.enemyEggTimer = DateTime.Now + SerpentConsts.EnemyEggFrequency;				
		}
		else if (DateTime.Now > this.enemyEggTimer)
		{
			// time for a random enemy snake to start laying an egg.  But we need a snake with 3+ segments.
			List<Snake> qualifiedSnakes = this.enemySnakes.FindAll( s => s.NumSegments >= 3 );
			if (qualifiedSnakes.Count == 0)
			{
				// Can't spawn an egg.  Reset the timer.
				this.enemyEggTimer = DateTime.Now + SerpentConsts.EnemyEggFrequency;								
				return;
			}
			
			int i = UnityEngine.Random.Range( 0, qualifiedSnakes.Count );
			Snake enemySnake = qualifiedSnakes[i];	
			SnakeSegment lastSegment = enemySnake.LastSegment;
			Egg e = CreateEgg();
			this.enemyEgg = e;			
			lastSegment.BeginToCreateEgg(e, EggFullyGrown, EggDestroyed);			
			
			// reset the timer for next time.
			this.enemyEggTimer = new DateTime(0);
		}	
	}
	
	private void EggDestroyed(Egg e)
	{
		if (e == this.enemyEgg)
		{
			this.enemyEgg = null;
		}
	}
	
	private void HandlePlayerEggs()
	{
		if (this.playerEggTimer.Ticks == 0)
		{
			// set the timer.
			this.playerEggTimer = DateTime.Now + SerpentConsts.PlayerEggFrequency;				
		}
		else if (DateTime.Now > this.playerEggTimer)
		{
			// initial test: just put the egg at the centre of the location of the snake's tail.
			/*
			MazeCell cell = this.mazeController.GetCellForPosition( this.playerSnake.LastSegment.transform.localPosition );
			int x = cell.X;
			int y = cell.Y;
			Egg e = CreateEgg(x, y, SerpentConsts.Dir.N);
			this.playerEgg = e;
			
			// reset the timer for next time.
			this.playerEggTimer = new DateTime(0);
			*/
		}		
	}

	private void NumSnakeSegmentsChanged(Snake snake)
	{
		this.updateSnakeColours = true;
	}
	
	private void PlayerSnakeDied(Snake snake)
	{		
		snake.Dead = true;
		
		if (Managers.GameState.ExtraSnakes > 0)
		{			
			// Trigger post-death sequence
			StartCoroutine(PostDeathSequence());
		}
		else
		{
			// Trigger game-over sequence.
		}
	}
		
	private void EnemySnakeDied(Snake snake)
	{
		// Note: don't bother to set Dead to true, just remove it from the list of snakes and destroy it.
		snake.SnakeSegmentsChanged -= this.NumSnakeSegmentsChanged;
		this.enemySnakes.Remove(snake);
		Destroy(snake);
	}
	
	private IEnumerator PostDeathSequence()
	{
		yield return new WaitForSeconds(3.0f);
		
		Managers.GameState.ExtraSnakes--;		
		
		for( int i = 0; i < this.enemySnakes.Count; ++i )
		{
			this.enemySnakes[i].Reset();
		}
		
		this.playerSnake.Reset();
		
		StartCoroutine( PlaceSnakes() );
	}

	private void UpdateEnemySnakeColours()
	{
		if (this.playerSnake.Dead) { return; }
		
		int playerLength = this.playerSnake.NumSegments;
		
		for( int i = 0; i < this.enemySnakes.Count; ++i)
		{
			Snake enemySnake = this.enemySnakes[i];
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
	
	private void UpdateText()
	{
		this.levelLabel.text = Managers.GameState.Level.ToString();
		this.scoreLabel.text = Managers.GameState.Score.ToString();
		this.livesLabel.text = Managers.GameState.ExtraSnakes.ToString();
	}
	
	#endregion Update
	
	
	#region Input

	private void OnPressUp()
	{
		OnPressDirection(SerpentConsts.Dir.N);
	}
		
	private void OnPressDown()
	{
		OnPressDirection(SerpentConsts.Dir.S);
	}
	
	private void OnPressLeft()
	{
		OnPressDirection(SerpentConsts.Dir.W);
	}
	
	private void OnPressRight()
	{
		OnPressDirection(SerpentConsts.Dir.E);
	}
		
	private void OnPressDirection(SerpentConsts.Dir direction)
	{
		if (this.playerSnake.Dead) { return; }
		PlayerSnakeController controller = this.playerSnake.Controller as PlayerSnakeController;
		if (controller == null) { return; }
		
		controller.StartMoving(direction);
	}

	#endregion Input
}
