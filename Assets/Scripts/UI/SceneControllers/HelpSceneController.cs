using UnityEngine;
using System.Collections;

public class HelpSceneController : RSSceneController 
{

	private void OnButtonPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Credits);
	}

}
