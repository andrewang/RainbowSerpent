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
		//this.soundSlider.value = Managers.SettingsManager.SoundVolume;
		
	}
	
	override public void OnLoad()
	{
		Debug.Log("Setting music slider volume to " + Managers.SettingsManager.MusicVolume);	
		this.musicSlider.value = Managers.SettingsManager.MusicVolume;		
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
		if (newValue < 0.0f)
		{
			this.soundSlider.value = 0.0f;
			return;
		}
		else if (newValue > 1.0f)
		{
			this.soundSlider.value = 1.0f;
			return;
		}
		Managers.SettingsManager.SoundVolume = newValue;
	}
	
	public void OnMusicSliderChange()
	{
		float newValue = this.musicSlider.value;
		/*
		if (newValue < 0.0f)
		{
			this.musicSlider.value = 0.0f;
			return;
		}
		else if (newValue > 1.0f)
		{
			this.musicSlider.value = 1.0f;
			return;
		}
		*/
		Managers.SettingsManager.MusicVolume = newValue;
	}
	
	#endregion Sliders
}
