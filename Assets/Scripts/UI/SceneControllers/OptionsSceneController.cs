using UnityEngine;
using System.Collections;
using Serpent;

public class OptionsSceneController : RSSceneController, CyclicSettingsDataSource
{

	private void OnMainMenuButtonPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Main);
	}

	#region Methods for cyclic labels
	
	public int GetValue(string valueName)
	{
		if (valueName == "Difficulty")
		{
			return (int) Managers.GameState.Difficulty;
		}
		return 0;	
	}
	
	public void SetValue(string valueName, int difficulty)
	{
		if (valueName == "Difficulty")
		{
			Managers.GameState.Difficulty = (Difficulty) difficulty;
		}
	}
	
	#endregion Methods for difficulty cyclic label
}
