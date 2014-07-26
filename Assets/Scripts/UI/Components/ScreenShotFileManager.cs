using System;
using UnityEngine;
using System.IO;

public class ScreenShotFileManager : MonoBehaviour
{
	private const int Version = 2;
	
	public ScreenShotFileManager ()
	{
	}
	
	public void Start()
	{
		RemoveOldScreenShots();
	}
	
	private void RemoveOldScreenShots()
	{
		for (int tempVersion = 0; tempVersion < ScreenShotFileManager.Version; ++tempVersion)
		{
			for (int levelNumber = 0; levelNumber < Managers.GameState.NumLevels; ++levelNumber)
			{
				if (ScreenShotExists(levelNumber, tempVersion))
				{
					string path = ScreenShotPath (levelNumber, tempVersion);
					File.Delete(path);
				}
			}
		}
	}

	public string ScreenShotPath(int levelNumber, int version = ScreenShotFileManager.Version)
	{
		string dataPath = Application.persistentDataPath;
		if (version > 0)
		{
			dataPath = Path.Combine(dataPath, "Maze" + levelNumber + "-" + version + ".png");
		}
		else
		{
			// For backwards compatibility purposes
			dataPath = Path.Combine(dataPath, "Maze" + levelNumber + ".png");			
		}
		return dataPath;
	}
	
	public bool ScreenShotExists(int levelNumber, int version = ScreenShotFileManager.Version)
	{
		string path = ScreenShotPath(levelNumber, version);
		return File.Exists(path);		
	}
	
	public Texture2D LoadScreenShot(int levelNumber)		
	{
		string path = ScreenShotPath(levelNumber, ScreenShotFileManager.Version);
		
		byte[] byteArray = File.ReadAllBytes(path);
		
		// The texture will be resized by reading it in.
		Texture2D screenShotTexture = new Texture2D(4, 4);
		screenShotTexture.LoadImage(byteArray);
		return screenShotTexture;
	}
}

