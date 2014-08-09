using UnityEngine;
using Serpent;

public class DifficultyManager : MonoBehaviour
{
	[SerializeField] private DifficultySettings[] difficultySettings;

	public DifficultySettings GetDifficultySettings(Difficulty difficulty)
	{
		int index = (int)difficulty;
		if (index < this.difficultySettings.Length)
		{
			return this.difficultySettings[index];
		}
		
		return null;
	}
	
	public DifficultySettings GetCurrentSettings()
	{
		GameState gameState = Managers.GameState;
		if (gameState == null) 
		{ 
			return GetDifficultySettings(Difficulty.Easy); 
		}
		Difficulty difficulty = Managers.GameState.Difficulty;
		return GetDifficultySettings(difficulty);
	}
}


