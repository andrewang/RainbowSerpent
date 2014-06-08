using UnityEngine;
using System;

public class Managers : MonoBehaviour
{
	public static GameObject SceneManagerInstance;
	public static SceneManager SceneManager;
	[SerializeField] GameObject sceneManagerPrefab = null;
		
	public static ScriptCache SnakeBodyCache;
	[SerializeField] GameObject snakeBodyCachePrefab = null;
	
	public static GameState GameState;
	
	void Start()
	{		
		Managers.SceneManagerInstance = (GameObject) Instantiate(this.sceneManagerPrefab);
		Managers.SceneManager = Managers.SceneManagerInstance.GetComponent<SceneManager>();
		DontDestroyOnLoad(Managers.SceneManagerInstance);
		
		GameObject snakeBodyCacheInst = (GameObject) Instantiate(this.snakeBodyCachePrefab);
		Managers.SnakeBodyCache = snakeBodyCacheInst.GetComponent<ScriptCache>();
		DontDestroyOnLoad(snakeBodyCacheInst);
		
		Managers.GameState = new GameState();
	}

}

