using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameSceneController : RSSceneController
{
	#region Serialized Fields
	[SerializeField] private GameManager gameManager = null;
	
	// Main component systems
	[SerializeField] private MazeController mazeController = null;
	[SerializeField] private InputController inputController = null; 
	
	// UI elements
	[SerializeField] private UISprite background = null;	
	[SerializeField] private UILabel[] labels;
	[SerializeField] private UILabel levelLabel = null;
	[SerializeField] private UILabel scoreLabel = null;
	[SerializeField] private UILabel livesLabel = null;	

	#endregion Serialized Fields
		
	#region Verify Serialize Fields

	override public void VerifySerializeFields()
	{
		if (this.mazeController == null) { Debug.LogError("GameSceneController: mazeController is null"); }
		if (this.inputController == null) { Debug.LogError("GameSceneController: inputController is null"); }
		if (this.gameManager == null) { Debug.LogError("GameSceneController: gameManager is null"); }
		if (this.background == null) { Debug.LogError("GameSceneController: background is null"); }
		if (this.levelLabel == null) { Debug.LogError("GameSceneController: levelLabel is null"); }
		if (this.scoreLabel == null) { Debug.LogError("GameSceneController: scoreLabel is null"); }
		if (this.livesLabel == null) { Debug.LogError("GameSceneController: livesLabel is null"); }
		if (this.labels == null || this.labels.Length == 0) { Debug.LogError("GameSceneController: labels is null/empty"); }
	}

	#endregion Verify Serialize Fields

	override public void OnLoad()
	{
		LoadGameLevel(Managers.GameState.Level);
	}

	private void LoadGameLevel(int levelNum)
	{
		// TODO Clear stuff?
		// TODO load theme based on level number.

		this.gameManager.Setup(levelNum);		
		ConfigureUI();
		ConfigureInput();
		this.gameManager.Begin();
	}

	private void ConfigureUI()
	{
		LevelTheme theme = this.gameManager.Theme;
		
		// Set initial colours
		foreach( UILabel label in this.labels )
		{
			label.color = theme.TextColour;
		}
		this.background.color = theme.BackgroundColour;
	}
	
	private void ConfigureInput()
	{
		// Now that snakes have been created, give the input controller a reference to the player's snake so it can be controlled
		// by player input.
		this.inputController.PlayerSnake = this.gameManager.PlayerSnake;
	}

	#region Update
	
	private void Update()
	{
		if (this.gameManager.PlayerSnake == null) { return; } 
		
		UpdateText();
	}
	
	private void UpdateText()
	{
		this.levelLabel.text = Managers.GameState.Level.ToString();
		this.scoreLabel.text = Managers.GameState.Score.ToString();
		this.livesLabel.text = Managers.GameState.ExtraSnakes.ToString();
	}
	
	#endregion Update
	
	
}
