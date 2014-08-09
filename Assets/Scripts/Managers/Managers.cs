using UnityEngine;
using System;

public class Managers : MonoBehaviour
{
	public static SceneManager SceneManager;
	[SerializeField] GameObject sceneManagerPrefab = null;
	
	public static ScreenManager ScreenManager;
	[SerializeField] GameObject screenManagerPrefab = null; 
	
	public static GameClock GameClock;
	[SerializeField] GameObject gameClockPrefab = null;
		
	public static ScriptCache SnakeBodyCache;
	[SerializeField] GameObject snakeBodyCachePrefab = null;
	
	public static DifficultyManager DifficultyManager;
	[SerializeField] GameObject difficultyManagerPrefab = null;
	
	public static GameState GameState;
	
	void Start()
	{
		GameObject sceneManagerInstance = InstantiateManager(this.sceneManagerPrefab);
		Managers.SceneManager = sceneManagerInstance.GetComponent<SceneManager>();

		GameObject screenManagerInstance = InstantiateManager(this.screenManagerPrefab);
		Managers.ScreenManager = screenManagerInstance.GetComponent<ScreenManager>();
		
		GameObject gameClockInstance = InstantiateManager(this.gameClockPrefab);
		Managers.GameClock = gameClockInstance.GetComponent<GameClock>();
		
		GameObject snakeBodyCacheInst = InstantiateManager(this.snakeBodyCachePrefab);
		Managers.SnakeBodyCache = snakeBodyCacheInst.GetComponent<ScriptCache>();
		
		GameObject difficultyManagerInst = InstantiateManager(this.difficultyManagerPrefab);
		Managers.DifficultyManager = difficultyManagerInst.GetComponent<DifficultyManager>();
		
		Managers.GameState = new GameState();
	}
	
	private GameObject InstantiateManager(GameObject prefab)
	{
		GameObject instance = (GameObject) Instantiate(prefab);
		DontDestroyOnLoad(instance);
		return instance;
	}

}

