using UnityEngine;
using System.Collections;

public class HelpSceneController : RSSceneController 
{
	// Use this for initialization
	void Start () 
	{
	}

	private void OnButtonPressed()
	{
		// return to the main menu
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Main);
	}

}
