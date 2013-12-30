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
	
	#endregion Serialized Fields
	
	private Snake playerSnake = null; 
	private List<Snake> enemySnakes = new List<Snake>();
	

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
		SnakeConfig playerSnakeConf = this.playerSnakeConfig.GetComponent<SnakeConfig>();
		this.playerSnake = CreateSnake(playerSnakeConf, 3, 1, 0, SerpentConsts.Dir.E);
		// If I want the player snake to start moving east, I need to send that information
		// to the player snake *controller*
		this.playerSnake.Controller.StartMoving(SerpentConsts.Dir.E);
		
		// Enemy snake
		SnakeConfig enemySnakeConf = this.enemySnakeConfig.GetComponent<SnakeConfig>();	
		Snake enemySnake = CreateSnake(enemySnakeConf, 5, 2, 4, SerpentConsts.Dir.E);
		this.enemySnakes.Add(enemySnake);
		enemySnake.StartMoving(SerpentConsts.Dir.E);
	}
	
	private Snake CreateSnake(SnakeConfig config, int length, int x, int y, SerpentConsts.Dir direction)
	{
		Snake snake = SerpentUtils.SerpentInstantiate<Snake>(this.snakePrefab, this.mazeController.transform);
		snake.SetUp(this.mazeController, config, length);
		Vector3 position = this.mazeController.GetCellCentre(x, y);
		snake.SetInitialLocation(position, direction);
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
				Destroy(enemySnake);
				// By removing a snake from enemySnakes, we move all the snakes after it up one in the list
				// So we continue the loop by reiterating with the same 'i' value as before.
				continue;
			}
			bool playerDies = enemySnake.TestForInteraction(playerSnake);
			if (playerDies)
			{
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


