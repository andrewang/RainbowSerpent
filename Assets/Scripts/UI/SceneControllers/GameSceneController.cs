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
	
	private List<Creature> creatures = new List<Creature>();
		
		
	PlayerSnakeController playerController = null;
	PlayerSnakeController PlayerController
	{
		get
		{
			if (this.playerController != null) 
			{ 
				return this.playerController;
			}
			// Otherwise get the player controller and assign it to the field.	
			foreach( Creature creature in this.creatures )
			{
				if (creature.Controller is PlayerSnakeController)
				{
					this.playerController = creature.Controller as PlayerSnakeController;
					break;
				}				
			}
			return this.playerController;
		}
	}

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
		
		// Empty creature list
		this.creatures.Clear();

		// Load in new map data.
		TextAsset mazeTextAsset = Resources.Load("testMaze") as TextAsset;
		this.mazeController.SetUp(mazeTextAsset);
		
		// Create creatures
		SnakeConfig playerSnakeConf = this.playerSnakeConfig.GetComponent<SnakeConfig>();
		CreateSnake(playerSnakeConf, 3, 1, 0, SerpentConsts.Dir.E);
		// If I want the player snake to start moving east, I need to send that information to the player
		// snake *controller*
		this.PlayerController.StartMoving(SerpentConsts.Dir.E);
		//playerSnake.StartMoving(SerpentConsts.Dir.E);
		
		// Enemy snake
		SnakeConfig enemySnakeConf = this.enemySnakeConfig.GetComponent<SnakeConfig>();	
		Snake enemySnake = CreateSnake(enemySnakeConf, 5, 2, 4, SerpentConsts.Dir.E);
		enemySnake.StartMoving(SerpentConsts.Dir.E);
	}
	
	private Snake CreateSnake(SnakeConfig config, int length, int x, int y, SerpentConsts.Dir direction)
	{
		Snake snake = SerpentUtils.SerpentInstantiate<Snake>(this.snakePrefab, this.mazeController.transform);
		snake.SetUp(this.mazeController, config, length);
		Vector3 position = this.mazeController.GetCellCentre(x, y);
		snake.SetInitialLocation(position, direction);
		this.creatures.Add(snake);
		return snake;
	}


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
		PlayerSnakeController controller = this.PlayerController;
		if (controller == null) { return; }
		
		controller.StartMoving(direction);
	}

	#endregion Input
}


