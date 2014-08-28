using UnityEngine;
using System.Collections;

public class HelpSceneController : RSSceneController 
{
	[SerializeField] private GameObject[] helpContents;
	
	private int currentHelp = 0;

	public void OnHelpButtonPressed()
	{
		// turn off the current help, turn on the next help... or if there is no help left, go to the main menu
		
		this.helpContents[this.currentHelp].SetActive(false);
		
		this.currentHelp++;
		if (this.currentHelp < this.helpContents.Length)
		{
			this.helpContents[this.currentHelp].SetActive(true);
			return;
		}
		
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Main);
	}

}
