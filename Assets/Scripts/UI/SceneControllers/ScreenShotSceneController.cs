using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ScreenShotSceneController : RSSceneController
{
	#region Serialized Fields
	
	// Main component systems
	[SerializeField] private MazeController mazeController = null;
	[SerializeField] private ScreenShotTaker shotTaker = null;

	#endregion Serialized Fields
	
	#region Verify Serialize Fields
	
	override public void VerifySerializeFields()
	{
		if (this.mazeController == null) { Debug.LogError("GameSceneController: mazeController is null"); }
	}
	
	#endregion Verify Serialize Fields
	
	private bool screenshotTaken = false;
	private LevelTheme theme;
	
	public void Start()
	{
		LoadGameLevel(1);
	}
	
	override public void OnLoad()
	{
		LoadGameLevel(1);
	}
	
	private void LoadGameLevel(int levelNum)
	{
		LoadTheme(levelNum);
		LoadMapData(levelNum);
	}
		
	public void LoadTheme(int levelNum)
	{
		UnityEngine.Object prefab = Resources.Load("theme" + levelNum.ToString());
		GameObject obj = Instantiate(prefab) as GameObject;
		this.theme = obj.GetComponent<LevelTheme>();
	}
	
	public void LoadMapData(int levelNum)
	{
		TextAsset mazeTextAsset = Resources.Load("level" + levelNum.ToString()) as TextAsset;
		this.mazeController.SetUp(mazeTextAsset, this.theme.WallColour);		
		// but then hide the doors
		this.mazeController.Maze.HideDoors();
		Application.CaptureScreenshot("Maze" + levelNum + ".png");
		
	}
	
	public void LateUpdate()
	{
		if (screenshotTaken == false)
		{
			this.shotTaker.TakeScreenShot(1);
//			HandScreenshot();
		}
	}
	
	/*
	private IEnumerator HandScreenshot()
	{
		yield return new WaitForEndOfFrame();
		Texture2D tex = new Texture2D(Screen.width, Screen.height);
		tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
		tex.Apply();
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes("DiffMaze.png", bytes);
		yield return null;
		
	}
	*/
}
