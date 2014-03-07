using UnityEngine;
using System;

public class Managers : MonoBehaviour
{
	public static SceneManager SceneManager;
	[SerializeField] GameObject sceneManagerPrefab = null;
		
	public static ScriptCache SnakeBodyCache;
	[SerializeField] GameObject snakeBodyCachePrefab = null;
	
	public static GameState GameState;
	
	void Start()
	{		
		GameObject sceneManagerInst = (GameObject) Instantiate(this.sceneManagerPrefab);
		Managers.SceneManager = sceneManagerInst.GetComponent<SceneManager>();
		
		GameObject snakeBodyCacheInst = (GameObject) Instantiate(this.snakeBodyCachePrefab);
		Managers.SnakeBodyCache = snakeBodyCacheInst.GetComponent<ScriptCache>();
		
		Managers.GameState = new GameState();
	}

}

