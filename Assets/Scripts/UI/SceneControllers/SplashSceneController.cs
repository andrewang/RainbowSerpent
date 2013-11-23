using UnityEngine;
using System.Collections;

public class SplashSceneController : RSSceneController 
{
	// Use this for initialization
	void Start () 
	{
		Debug.Log("Splash scene controller - start");
	}

	private void OnStartPressed ()
	{
		// begin the game
		Debug.Log("Start play!");
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Game);
	}

	override public void OnUnload()
	{
		Debug.Log("Splash scene Unloaded");
	}
}
