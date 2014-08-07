using UnityEngine;
using System.Collections;

public class MainMenuSceneController : RSSceneController 
{
	[SerializeField] private UILabel versionLabel;
	
	void Start()
	{
		if (this.versionLabel == null) { return; }
		this.versionLabel.text = SerpentConsts.Version;
	}

	private void OnStartPressed()
	{
		// begin the game by switching to the game scene		
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Game);
	}
	
	private void OnHelpPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Help);
	}

	private void OnOptionsPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Options);
	}
	
}
