using UnityEngine;
using System;
using System.Collections.Generic;

public class GameSceneController : RSSceneController
{
	#region Serialized Fields
	[SerializeField] private MazeController mazeController = null;
	[SerializeField] private GameObject snakePrefab = null;
	[SerializeField] private GameObject playerSnakeConfig = null;
	[SerializeField] private GameObject enemySnakeConfig = null;
	
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
		this.mazeController.SetUp(mazeTextAsset);
		
		// Create creatures
		this.playerSnakeConf = this.playerSnakeConfig.GetComponent<SnakeConfig>();
		this.enemySnakeConf = this.enemySnakeConfig.GetComponent<SnakeConfig>();	
		
		this.playerSnake = CreateSnake(this.playerSnakeConf, 3, 1, 0, SerpentConsts.Dir.E);
		// If I want the player snake to start moving east, I need to send that information
		// to the player snake *controller*
		this.playerSnake.Controller.StartMoving(SerpentConsts.Dir.E);
		
		// Enemy snake
		Snake enemySnake = CreateSnake(this.enemySnakeConf, 5, 8, 12, SerpentConsts.Dir.W);
		this.enemySnakes.Add(enemySnake);
		enemySnake.StartMoving(SerpentConsts.Dir.W);

		Snake enemySnake2 = CreateSnake(this.enemySnakeConf, 5, 10, 9, SerpentConsts.Dir.S);
		this.enemySnakes.Add(enemySnake2);
		enemySnake2.StartMoving(SerpentConsts.Dir.S);
		
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
				enemySnake.ChangeColour(this.enemySnakeConf.Colour);
			}
			else
			{
				// safe colour
				enemySnake.ChangeColour(this.enemySnakeConf.Colour2);
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


