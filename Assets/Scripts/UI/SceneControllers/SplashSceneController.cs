using UnityEngine;
using System.Collections;

public class SplashSceneController : RSSceneController 
{
	// Use this for initialization
	void Start () 
	{
	}

	private void OnStartPressed ()
	{
		// begin the game by switching to the game scene		
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Game);
	}

	override public void OnUnload()
	{
	}
}
