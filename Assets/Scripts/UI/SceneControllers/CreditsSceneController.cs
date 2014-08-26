using UnityEngine;
using System.Collections;

public class CreditsSceneController : RSSceneController 
{		
	static string musicUrl = "https://www.youtube.com/watch?v=lb_WiUn2LXg";
	
	public void OnBackgroundButtonPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Main);
	}
	
	public void OnMusicButtonPressed()
	{
		Application.OpenURL(musicUrl);
	}
	
}
