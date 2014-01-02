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
	private bool updateSnakeColours = false;

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
		
		this.playerSnake = CreateSnake(this.playerSnakeConf, 3);
		this.playerSnake.ChangeColour(this.theme.PlayerSnakeColour);
		PlaceSnake(this.playerSnake, 1, 0, SerpentConsts.Dir.E);
				
		// Enemy snakes
		CreateEnemySnake(5);
		CreateEnemySnake(5);
		CreateEnemySnake(5);
		StartCoroutine( PlaceEnemySnakes() );
	}
	
	private IEnumerator PlaceEnemySnakes()
	{
		PlaceSnake(this.enemySnakes[0], 8, 12, SerpentConsts.Dir.W);
		yield return new WaitForSeconds(5.0f);
		PlaceSnake(this.enemySnakes[1], 8, 12, SerpentConsts.Dir.W);
		yield return new WaitForSeconds(5.0f);
		PlaceSnake(this.enemySnakes[2], 8, 12, SerpentConsts.Dir.W);
	}
	
	private void CreateEnemySnake(int length)
	{
		Snake enemySnake = CreateSnake(this.enemySnakeConf, length);
		enemySnake.ChangeColour(this.theme.EnemySnakeColour);
		this.enemySnakes.Add(enemySnake);
		// place enemy snake offscreen?
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

	#region Update
	
	private void Update()
	{
		if (this.playerSnake == null) { return; }
		
		// Test for creature-creature interactions.
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
				this.enemySnakes.RemoveAt(i);
				enemySnake.SnakeSegmentsChanged -= this.NumSnakeSegmentsChanged;
				
				Destroy(enemySnake);
				// By removing a snake from enemySnakes, we move all the snakes after it up one in the list
				// So we continue the loop by reiterating with the same 'i' value as before.
				continue;
			}
			bool playerDies = enemySnake.TestForInteraction(playerSnake);
			if (playerDies)
			{
				this.playerSnake.SnakeSegmentsChanged -= this.NumSnakeSegmentsChanged;
				
				Destroy (this.playerSnake);
				this.playerSnake = null;
				break;
			}
			++i;
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
		
		UpdateText();		
	}

	private void NumSnakeSegmentsChanged()
	{
		this.updateSnakeColours = true;
	}
		
	private void UpdateEnemySnakeColours()
	{
		if (this.playerSnake == null) { return; }
		
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
		if (this.playerSnake == null) { return; }
		PlayerSnakeController controller = this.playerSnake.Controller as PlayerSnakeController;
		if (controller == null) { return; }
		
		controller.StartMoving(direction);
	}

	#endregion Input
}


