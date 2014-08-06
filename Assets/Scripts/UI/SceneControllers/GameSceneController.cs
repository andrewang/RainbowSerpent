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
	[SerializeField] private GameObject textContainer = null;	
	[SerializeField] private UISprite background = null;	
	[SerializeField] private UILabel[] labels;
	[SerializeField] private UILabel levelLabel = null;
	[SerializeField] private UILabel scoreLabel = null;
	[SerializeField] private UILabel livesLabel = null;	
	[SerializeField] private GameObject buttonsContainer;

	[SerializeField] private GameObject pauseUIContainer = null;
	[SerializeField] private GameObject gameOverUIContainer = null;
	
	[SerializeField] private UIPanel mazePanel = null; 
	[SerializeField] private UILabel debugInfoLabel = null;
	
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
		if (this.buttonsContainer == null) { Debug.LogError("GameSceneController: buttonsContainer is null/empty"); }
		if (this.gameOverUIContainer == null) { Debug.LogError("GameSceneController: gameOverUIContainer is null"); } 
		
		if (this.mazePanel == null) { Debug.LogError("GameSceneController: mazePanel is null"); }
		if (this.debugInfoLabel == null) { Debug.LogError("GameSceneController: debugInfoLabel is null"); }
	}

	#endregion Verify Serialize Fields

	#region Setup
	
	override public void OnLoad()
	{
		this.gameManager.GameOver += this.GameOver;
		LoadGameLevel(Managers.GameState.Level);
	}

	private void LoadGameLevel(int levelNum)
	{
		this.gameManager.Setup(levelNum);		
		ConfigureUI();
		ConfigureInput();
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
	
	#endregion Setup

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
		
		//debugInfoLabel
		
		string debugInfo = "";
		
		UISprite sprite = this.mazeController.GetFirstWallSprite();
		if (sprite)
		{
			Vector3 scale = sprite.transform.localScale;
			int width = sprite.width;
			int height = sprite.height;
			debugInfo = debugInfo + "Wall: " + scale.x + ", " + scale.y + "  W: " + width + " H: " + height;
		}
		else
		{
			GameObject screenShotContainer = this.mazeController.GetScreenShot();
			if (screenShotContainer) 
			{
				Vector3 scale = screenShotContainer.transform.localScale;
				debugInfo = debugInfo + "Screenshot scale: " + scale.x + ", " + scale.y;
			}
		}
		
		// screen dimensions
		debugInfo = debugInfo + "\n";
		debugInfo = debugInfo + "  Screen W: " + Screen.width + " H: " + Screen.height;
		debugInfo = debugInfo + "  Text W scale: " + this.textContainer.transform.localScale.x;
				
		this.debugInfoLabel.text = debugInfo;
		
	}
	
	#endregion Update
	
	#region Game Over
	
	public void GameOver()
	{
		Managers.GameState.Paused = true;
		Managers.GameClock.Paused = true;
			
		this.buttonsContainer.SetActive(false);
		this.gameOverUIContainer.gameObject.SetActive(true);
	}
	
	#endregion Game Over
	
	#region Button input
	
	private void RestartGame()
	{
		Managers.GameClock.Paused = false;			
		Managers.GameState.Reset();
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Game);		
	}
		
	private void PauseGame()
	{
		this.buttonsContainer.SetActive(false);
		this.pauseUIContainer.gameObject.SetActive(true);	
		Managers.GameState.Paused = true;
		Managers.GameClock.Paused = true;
	}
	
	private void ResumeGame()
	{
		this.buttonsContainer.SetActive(true);
		this.pauseUIContainer.gameObject.SetActive(false);	
		Managers.GameState.Paused = false;
		Managers.GameClock.Paused = false;		
	}
	
	private void ReturnToMainMenu()
	{
		Managers.GameClock.Paused = false;		
		Managers.GameState.Reset();
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Main);
	}
	
	#endregion Button input
	
	
}
