using UnityEngine;
using Serpent;
using MiniJSON;
using System.IO;
using System.Collections.Generic;
using SerpentExtensions;


public class SettingsManager : MonoBehaviour
{
	[SerializeField] private DifficultySettings[] difficultySettings;
	
	private string settingsFileName = "settings.json";
	
	private Difficulty difficulty;
	public Difficulty Difficulty 
	{ 
		get
		{	
			return this.difficulty;
		}
		set
		{
			this.difficulty = value;
			Save();
		}
	}	

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
	
	private void Start()
	{
		// after starting, we don't need any other mono behavior, so set the gameobject to disabled
		Load();
		this.gameObject.SetActive(false);
	}
	
	private string SettingsFilePath()
	{
		return Path.Combine(Application.persistentDataPath, this.settingsFileName);
	}
	
	private void Load()
	{
		string path = SettingsFilePath();
		if (File.Exists(path) == false)
		{
			return;
		}
		
		string text = File.ReadAllText(path);
		Dictionary<string,object> dict = Json.Deserialize(text) as Dictionary<string,object>;
		
		// int version = dict.GetInt ("version");
		// nothing to do with version at the moment.		
		
		int intDiff = dict.GetInt("difficulty");
		this.difficulty = (Difficulty) intDiff;
	}
	
	public void Save()
	{
		Dictionary<string,object> saveDict = new Dictionary<string,object>();
		
		saveDict.Add("version", SerpentConsts.SettingsVersion);
		
		int intDiff = (int) this.Difficulty;
		saveDict.Add("difficulty", intDiff);
		
		string jsonText = Json.Serialize(saveDict);
		File.WriteAllText(SettingsFilePath(), jsonText);
	}
}


