using UnityEngine;
using System;
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
		LoadGameLevel(1);
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
		
		this.playerSnake = CreateSnake(this.playerSnakeConf, 3, 1, 0, SerpentConsts.Dir.E);
		this.playerSnake.ChangeColour(this.theme.PlayerSnakeColour);
		
		this.playerSnake.Controller.StartMoving(SerpentConsts.Dir.E);
		
		// Enemy snake
		CreateEnemySnake(5, 8, 12, SerpentConsts.Dir.W);
		CreateEnemySnake(5, 10, 9, SerpentConsts.Dir.S);
		CreateEnemySnake(5, 0, 9, SerpentConsts.Dir.S);
	}
	
	private void CreateEnemySnake(int length, int x, int y, SerpentConsts.Dir direction)
	{
		Snake enemySnake = CreateSnake(this.enemySnakeConf, length, x, y, direction);
		enemySnake.ChangeColour(this.theme.EnemySnakeColour);
		
		this.enemySnakes.Add(enemySnake);
		// TODO make instruction to start moving go through enemy snake controller
		enemySnake.StartMoving(direction);	
	}
	
	private Snake CreateSnake(SnakeConfig config, int length, int x, int y, SerpentConsts.Dir direction)
	{
		Snake snake = SerpentUtils.SerpentInstantiate<Snake>(this.snakePrefab, this.mazeController.transform);
		snake.SetUp(this.mazeController, config, length);
		Vector3 position = this.mazeController.GetCellCentre(x, y);
		Debug.Log("Adding snake at (" + x + "," + y + "): " + position.x + "," + position.y);
		snake.SetInitialLocation(position, direction);
		snake.SnakeSegmentsChanged += this.NumSnakeSegmentsChanged;
		return snake;
	}

	#region Update
	
	private void Update()
	{
		if (this.playerSnake == null) { return; }
		
		// Test for creature-creature interactions.
		for( int i = 0; i < this.enemySnakes.Count; )
		{
			Snake enemySnake = this.enemySnakes[i];
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
	}

	private void NumSnakeSegmentsChanged()
	{
		this.updateSnakeColours = true;
	}
		
	public void UpdateEnemySnakeColours()
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


