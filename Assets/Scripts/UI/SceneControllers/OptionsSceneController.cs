using UnityEngine;
using System.Collections;
using Serpent;

public class OptionsSceneController : RSSceneController, CyclicSettingsDataSource
{
	[SerializeField] private UISlider soundSlider;
	[SerializeField] private UISlider musicSlider;
	
	override public void Start()
	{
		base.Start();		
	}
	
	override public void OnLoad()
	{
		this.musicSlider.value = Managers.SettingsManager.MusicVolume;		
		this.soundSlider.value = Managers.SettingsManager.SoundVolume;		
	}

	private void OnMainMenuButtonPressed()
	{
		Managers.SceneManager.LoadScene(SerpentConsts.SceneNames.Main);
	}

	#region Cyclic labels
	
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
	
	#endregion Cyclic labels
	
	#region Sliders
	
	public void OnSoundSliderChange()
	{
		float newValue = this.soundSlider.value;
		Managers.SettingsManager.SoundVolume = newValue;
	}
	
	public void OnMusicSliderChange()
	{
		float newValue = this.musicSlider.value;
		Managers.SettingsManager.MusicVolume = newValue;
	}
	
	#endregion Sliders
}
