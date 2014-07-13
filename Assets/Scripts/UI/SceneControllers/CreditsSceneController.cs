using UnityEngine;
using System.Collections;

public class CreditsSceneController : RSSceneController 
{
	
	private void OnButtonPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Main);
	}
	
}
