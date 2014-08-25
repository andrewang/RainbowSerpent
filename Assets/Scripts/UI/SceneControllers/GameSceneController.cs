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
	//[SerializeField] private GameObject textContainer = null;	
	[SerializeField] private Camera sceneCamera = null;
	[SerializeField] private UILabel[] labels;
	[SerializeField] private UISprite[] sprites;
	[SerializeField] private UIButton[] buttons;
	[SerializeField] private UILabel levelLabel = null;
	[SerializeField] private UILabel scoreLabel = null;
	[SerializeField] private UILabel livesLabel = null;	
	[SerializeField] private GameObject buttonsContainer;

	[SerializeField] private GameObject pauseUIContainer = null;
	[SerializeField] private GameObject gameOverUIContainer = null;
	
	[SerializeField] private UIPanel mazePanel = null; 
	[SerializeField] private UILabel debugInfoLabel = null;
	
	[SerializeField] private AudioSource musicSource;
	
	private bool paused;
	
	#endregion Serialized Fields
		
	#region Verify Serialize Fields

	override public void VerifySerializeFields()
	{
		if (this.mazeController == null) { Debug.LogError("GameSceneController: mazeController is null"); }
		if (this.inputController == null) { Debug.LogError("GameSceneController: inputController is null"); }
		if (this.gameManager == null) { Debug.LogError("GameSceneController: gameManager is null"); }
		if (this.levelLabel == null) { Debug.LogError("GameSceneController: levelLabel is null"); }
		if (this.scoreLabel == null) { Debug.LogError("GameSceneController: scoreLabel is null"); }
		if (this.livesLabel == null) { Debug.LogError("GameSceneController: livesLabel is null"); }
		if (this.labels == null || this.labels.Length == 0) { Debug.LogError("GameSceneController: labels is null/empty"); }
		if (this.sprites == null || this.sprites.Length == 0) { Debug.LogError("GameSceneController: sprites is null/empty"); }
		if (this.buttons == null || this.buttons.Length == 0) { Debug.LogError("GameSceneController: buttons is null/empty"); }
		if (this.buttonsContainer == null) { Debug.LogError("GameSceneController: buttonsContainer is null/empty"); }
		if (this.gameOverUIContainer == null) { Debug.LogError("GameSceneController: gameOverUIContainer is null"); } 
		
		if (this.mazePanel == null) { Debug.LogError("GameSceneController: mazePanel is null"); }
		if (this.debugInfoLabel == null) { Debug.LogError("GameSceneController: debugInfoLabel is null"); }
		
		if (this.musicSource == null) { Debug.LogError("GameSceneController: musicSource is null"); }
	}

	#endregion Verify Serialize Fields

	#region Setup
	
	override public void OnLoad()
	{
		base.OnLoad();
		Managers.GameClock.Reset();
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
			label.color = theme.UIColour;
		}
		foreach( UISprite sprite in this.sprites )
		{
			sprite.color = theme.UIColour;
		}
		foreach( UIButton button in this.buttons )
		{
			button.defaultColor = theme.UIColour;
			button.pressed = theme.PressedButtonColour;
			// This hover colour really only matters for testing, but it's annoying for it not to be set.
			button.hover = theme.UIColour;
		}
		this.sceneCamera.backgroundColor = theme.BackgroundColour;
	}
	
	private void ConfigureInput()
	{
		// Now that snakes have been created, give the input controller a reference to the player's snake so it can be controlled
		// by player input.
		this.inputController.PlayerSnake = this.gameManager.PlayerSnake;
	}
	
	#endregion Setup

	#region Update
	
	public new void Update()
	{
		if (this.gameManager.PlayerSnake == null) { return; } 
		
		UpdateText();
		
		if (Input.GetKeyDown(KeyCode.Escape)) 
		{
			if (this.paused)
			{
				Application.Quit();
			}
			else
			{		
				PauseGame();				
			}
		}
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
				debugInfo = debugInfo + "Screenshot scale: " + scale.y;
				
				Transform t = screenShotContainer.transform.parent;
				while (t != null)
				{
					debugInfo = debugInfo + ", " + t.localScale.y;
					t = t.parent;
				}
				
				debugInfo = debugInfo + "\n";
				UITexture screenShotTexture = screenShotContainer.GetComponentInChildren<UITexture>();
				if (screenShotTexture != null)
				{
					debugInfo = debugInfo + "SS Size: " + screenShotTexture.width + " by " + screenShotTexture.height;
				}
				
			}
		}
		
		// screen dimensions
		debugInfo = debugInfo + "\n";
		debugInfo = debugInfo + "Screen Size: " + Screen.width + " by " + Screen.height;
				
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
		this.paused = false;
		
		Managers.GameClock.Paused = false;			
		Managers.GameClock.Reset(); 
		
		Managers.GameState.Reset();
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Game);		
	}
		
	private void PauseGame()
	{
		this.paused = true;
		
		this.buttonsContainer.SetActive(false);
		this.pauseUIContainer.gameObject.SetActive(true);	
		Managers.GameState.Paused = true;
		Managers.GameClock.Paused = true;
		
		this.musicSource.Pause();
	}
	
	private void ResumeGame()
	{
		this.paused = false;
		
		this.buttonsContainer.
		SetActive(true);
		this.pauseUIContainer.gameObject.SetActive(false);	
		Managers.GameState.Paused = false;
		Managers.GameClock.Paused = false;	
		
		this.musicSource.Play();	
	}
	
	private void ReturnToMainMenu()
	{
		// Clear the game clock of all events and state
		Managers.GameClock.Paused = false;
		Managers.GameClock.Reset(); 
		
		Managers.GameState.Reset();
		
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Main);
	}
	
	#endregion Button input
	
	
}
