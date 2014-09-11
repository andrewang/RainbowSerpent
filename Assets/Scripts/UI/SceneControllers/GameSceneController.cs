using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SerpentExtensions;

public class GameSceneController : RSSceneController
{
	#region Serialized Fields
	[SerializeField] private GameManager gameManager = null;
	
	// Main component systems
	[SerializeField] private MazeController mazeController = null;
	[SerializeField] private InputController inputController = null; 
	
	// UI elements	
	[SerializeField] private Camera sceneCamera = null;
	[SerializeField] private UILabel[] labels;
	[SerializeField] private UISprite[] sprites;
	[SerializeField] private UIButton[] buttons;
	[SerializeField] private UILabel levelLabel = null;
	[SerializeField] private UILabel scoreLabel = null;
	[SerializeField] private UILabel livesLabel = null;	
	[SerializeField] private GameObject buttonsContainer;
	
	[SerializeField] private GameObject[] mapMaskSets = null;

	[SerializeField] private GameObject pauseUIContainer = null;
	[SerializeField] private GameObject gameOverUIContainer = null;
	
	[SerializeField] private UIPanel mazePanel = null; 
	[SerializeField] private UILabel debugInfoLabel = null;
	
	[SerializeField] private AudioSource musicSource;
	
	private GameObject mapMaskSet;
	private List<UISprite> mapMaskSprites = new List<UISprite>();	
	private UITweener maskTweener = null;
	
	private int levelLoaded = -1;
	private int levelToLoad = -1;
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
		
		if (this.mapMaskSets == null || this.mapMaskSets.Length == 0) { Debug.LogError("GameSceneController: mapMask is null"); }
		
		if (this.mazePanel == null) { Debug.LogError("GameSceneController: mazePanel is null"); }
		if (this.debugInfoLabel == null) { Debug.LogError("GameSceneController: debugInfoLabel is null"); }
		
		if (this.musicSource == null) { Debug.LogError("GameSceneController: musicSource is null"); }
	}

	#endregion Verify Serialize Fields

	#region Setup
	
	override public void OnLoad()
	{
		base.OnLoad();
		StartCoroutine(LoadFirstLevel());
				
		this.gameManager.GameOver += this.GameOver;
		this.mazeController.OnCreateScreenShot += this.OnCreateScreenShot;
		this.mazeController.OnLoadScreenShot += this.OnLoadScreenShot;
		this.mazeController.OnMapLoadComplete += this.OnMapLoadComplete;
	}
	
	private IEnumerator LoadFirstLevel()
	{
		yield return new WaitForEndOfFrame();
		ResizeMazePanel();
		LoadGameLevel(Managers.GameState.Level);
	}

	public void LoadGameLevel(int levelNum)
	{
		if (this.levelLoaded >= 1)
		{
			// Throw out old stuff
			ResetForNewLevel();
		}
		
		this.levelLoaded = levelNum;
		
		Managers.GameClock.Reset();

		// Speed needs to be rechecked here because it may have otherwise been calculated before 
		// SettingsManager was loaded.
		Managers.GameState.RecalculateSpeed();
		
		this.gameManager.Setup(levelNum);		
		ConfigureUI();
		ConfigureInput();
		
		// Ensure one of the mask sets is selected at the start so we can enable it to hide the map
		if (this.mapMaskSet == null)
		{
			SelectMaskAnimation();
		}
		
		this.mazeController.Blah();
		
	}
	
	public void TransitionToLevel(int levelNum)
	{
		this.levelToLoad = levelNum;
		HideMap();
	}
	
	private void ResetForNewLevel()
	{
		// Everything that should need to be reset ought to be stuff in the maze.
		this.mazeController.Reset();
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
	
	private void ResizeMazePanel()
	{
		ResizePanel resizeScript = this.mazePanel.GetComponent<ResizePanel>();
		if (resizeScript != null)
		{
			resizeScript.Execute();
		}
		
		foreach (UISprite mask in this.mapMaskSprites)
		{
			StretchToFitPanel stretch = mask.gameObject.GetComponent<StretchToFitPanel>();
			stretch.Stretch();			
		}
	}
	
	private void OnCreateScreenShot()
	{
		// The mask has to be disabled to create a screenshot.
		// TODO investigate using another camera not displayed to the screen or something.
		SetMaskEnabled(false);
	}
	
	private void OnLoadScreenShot()
	{
		SetMaskEnabled(true);
	}
	
	private void OnMapLoadComplete()
	{
		RevealMap();		
	}
	
	private void RevealMap()
	{
		SelectMaskAnimation();
		// Delay a moment (via coroutine) before doing the reveal.
		StartCoroutine(RevealMapCoroutine());
	}	
	
	IEnumerator RevealMapCoroutine()
	{
		yield return new WaitForSeconds(0.1f);
		PlayMaskAnimation("OnMapRevealed", true);		
	}
	
	private void OnMapRevealed()
	{
		SetMaskEnabled(false);
		this.maskTweener.onFinished.Clear();
		this.gameManager.Begin();
	}
	
	private void HideMap()
	{
		PlayMaskAnimation("OnMapHidden", false);	
	}
	
	private void OnMapHidden()
	{
		this.maskTweener.onFinished.Clear();
		LoadGameLevel(this.levelToLoad);
	}
	
	private void SelectMaskAnimation()
	{
		// Hide any previous mask
		SetMaskEnabled(false);
		this.mapMaskSprites.Clear();
		
		// Different sets of masks and animations exist.  For instance, zoom in/out from
		// the centre, pan down, pan right.
		int numSets = this.mapMaskSets.Length;
		int setNum = UnityEngine.Random.Range(0, numSets);
		GameObject set = this.mapMaskSets[setNum];
		
		this.mapMaskSet = set;
		
		// Recolor the sprites and put them in a list to trigger their animations.
		LevelTheme theme = this.gameManager.Theme;		
		UISprite[] maskSprites = set.GetComponentsInChildren<UISprite>(true);
		foreach( UISprite mask in maskSprites )
		{
			mask.color = theme.BackgroundColour;			
			this.mapMaskSprites.Add(mask);
			
			TweenColor colourTween = mask.GetComponent<TweenColor>();
			if (colourTween != null)
			{
				colourTween.Recolour(theme.BackgroundColour);				
			}
		}
		
		// Set the new mask to be displayed
		SetMaskEnabled(true);
	}
	
	private void PlayMaskAnimation(string callback, bool forward)
	{
		SetMaskEnabled(true);
		
		// Reset which tweener we track, then find a new one when looping.
		this.maskTweener = null;
		
		foreach (UISprite mask in this.mapMaskSprites)
		{
			UITweener scaler = mask.gameObject.GetComponent<UITweener>();
			if (scaler != null)
			{
				if (this.maskTweener == null)
				{
					this.maskTweener = scaler;
					EventDelegate onMapRevealedDelegate = new EventDelegate(this, callback);
					this.maskTweener.onFinished.Add( onMapRevealedDelegate );
				}
				scaler.enabled = true;
				if (forward)
				{
					scaler.PlayForward();
				}
				else
				{
					scaler.PlayReverse();
				}
			}
		}
	}
	
	private void SetMaskEnabled(bool enabled)
	{
		if (this.mapMaskSet != null)
		{
			this.mapMaskSet.SetActive(enabled);
		}
		/*
		foreach (UISprite mask in this.mapMaskSprites)
		{
			mask.gameObject.SetActive(enabled);
		}
		*/
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
		
		// Debug Info Label
				
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
