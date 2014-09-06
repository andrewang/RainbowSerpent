using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ScreenShotFileManager : MonoBehaviour
{	
	public ScreenShotFileManager ()
	{
	}
	
	public void Start()
	{
		RemoveOldScreenShots();
	}
	
	private void RemoveOldScreenShots()
	{
		for (int tempVersion = 0; tempVersion < SerpentConsts.ScreenShotVersion; ++tempVersion)
		{
			RemoveOldScreenShots(tempVersion);
		}
		RemoveWrongSizedScreenShots();
	}
	
	private void RemoveOldScreenShots(int version)
	{
		string directory = GetScreenShotDirectory();
		string searchPattern = GetScreenShotFilePattern(version);
		DirectoryInfo dir = new DirectoryInfo(directory);
		FileInfo[] infoArray = dir.GetFiles(searchPattern);
		foreach (FileInfo info in infoArray)
		{
			string fullPath = Path.Combine(directory, info.FullName);
			File.Delete(fullPath);
		}
	}
	
	private void RemoveWrongSizedScreenShots()
	{
		// We only do this for the current version because the removal of old screenshots will work for all else
		int version = SerpentConsts.ScreenShotVersion;
		
		string directory = GetScreenShotDirectory();		
		DirectoryInfo dir = new DirectoryInfo(directory);

		for (int levelNumber = 0; levelNumber < Managers.GameState.NumLevels; ++levelNumber)
		{
			string searchPattern = GetScreenShotFilePattern(version, levelNumber);
			FileInfo[] infoArray = dir.GetFiles(searchPattern);
			foreach (FileInfo info in infoArray)
			{
				// Does this file match the FULL path for this level and current screen size?
				string properName = ScreenShotName(levelNumber, version);
				string infoName = info.Name;
				if (infoName.Equals(properName) == false)
				{
					File.Delete(info.FullName);
				}
			}
		}
	}

	public string ScreenShotPath(int levelNumber, int version = SerpentConsts.ScreenShotVersion)
	{
		string dataPath = Application.persistentDataPath;
		string fileName = ScreenShotName(levelNumber, version);
		dataPath = Path.Combine(dataPath, fileName);
		return dataPath;
	}
	
	private string ScreenShotName(int levelNumber, int version = SerpentConsts.ScreenShotVersion)
	{
		string name;
		if (version >= 6)
		{
			// include resolution in filename.
			name = "Maze" + version + "-" + levelNumber + "-" + Screen.width + "x" + Screen.height + ".png";
		}
		else
		{
			if (version > 0)
			{
				name = "Maze" + levelNumber + "-" + version + ".png";
			}
			else
			{
				// For backwards compatibility purposes
				name = "Maze" + levelNumber + ".png";
			}
		}
		return name;
	}
		
	
	private string GetScreenShotDirectory()
	{
		return Application.persistentDataPath;
	}
	
	private string GetScreenShotFilePattern(int version)
	{
		if (version < 6) { return "Maze*-" + version + ".png"; }
		
		return "Maze" + version + "-*";
	}
	
	private string GetScreenShotFilePattern(int version, int levelNumber)
	{
		if (version < 6) { return ""; } // inapplicable
		
		return "Maze" + version + "-" + levelNumber + "-*";
	}
	
	public bool ScreenShotExists(int levelNumber, int version = SerpentConsts.ScreenShotVersion)
	{
		string path = ScreenShotPath(levelNumber, version);
		return File.Exists(path);		
	}
	
	public Texture2D LoadScreenShot(int levelNumber)		
	{
		string path = ScreenShotPath(levelNumber, SerpentConsts.ScreenShotVersion);
		
		byte[] byteArray = File.ReadAllBytes(path);
		
		// The texture will be resized by reading it in.
		// Make sure not to create any mipmaps - this created fuzziness (low resolution)
		// in the simulator!
		Texture2D screenShotTexture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
		screenShotTexture.LoadImage(byteArray);
		return screenShotTexture;
	}
}

