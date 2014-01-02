using UnityEngine;
using System;

public class Managers : MonoBehaviour
{
	public static SceneManager SceneManager;
	[SerializeField] GameObject sceneManagerPrefab = null;
	
	public static GameState GameState;

	void Start()
	{
		GameObject inst = (GameObject) Instantiate(this.sceneManagerPrefab);
		Managers.SceneManager = inst.GetComponent<SceneManager>();
		
		Managers.GameState = new GameState();
	}


}

