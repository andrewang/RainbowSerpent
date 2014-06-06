using UnityEngine;
using System.Collections;
using System.IO;

public class ScreenShotTaker : MonoBehaviour 
{
	private int resWidth;
	private int resHeight;
	
	private bool takeShot = false;
	
	public static string ScreenShotName(int width, int height) {
		return string.Format("{0}/screen_{1}x{2}_{3}.png", 
		                     Application.dataPath, 
		                     width, height, 
		                     System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}
	
	public void TakeScreenShot(int mapNumber) 
	{
		this.resWidth = Screen.width; 
		this.resHeight = Screen.height;
		
		this.takeShot = true;
		StartCoroutine(TakeAScreenshot());
	}
	
	IEnumerator TakeAScreenshot()
	{
		yield return new WaitForEndOfFrame();
		string filename = ScreenShotName(resWidth, resHeight);
		
		Texture2D tex = new Texture2D(resWidth, resHeight);
		tex.ReadPixels(new Rect(0,0,resWidth,resHeight),0,0);
		tex.Apply();
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes(filename, bytes);
		yield return null;
	}
}