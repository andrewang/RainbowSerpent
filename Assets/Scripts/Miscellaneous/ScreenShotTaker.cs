using UnityEngine;
using System.Collections;
using System.IO;

public class ScreenShotTaker : MonoBehaviour 
{
	public void TakeScreenShot(string fileName, int width, int height, int centreX, int centreY)
	{
		// incoming coordinates are based on a centre origin and need to be converted to pixel space
		// where 0,0 is in the lower left
		Debug.Log("Take screenshot called with params" + fileName + " " + width + " " 
			+ height + " " + centreX + " " + centreY);
		
		int screenWidth = Screen.width;
		int screenHeight = Screen.height;
		int leftX = centreX + (screenWidth - width) / 2;
		int bottomY = centreY + (screenHeight - height) / 2;
		StartCoroutine(TakeAScreenshot(fileName, width, height, leftX, bottomY));
	}
	
	IEnumerator TakeAScreenshot(string fileName, int width, int height, int leftX, int bottomY)
	{
		yield return new WaitForEndOfFrame();
		
		Texture2D tex = new Texture2D(width, height);
		tex.ReadPixels(new Rect(leftX, bottomY, width, height), 0, 0);
		tex.Apply();
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes(fileName, bytes);
		yield return null;
	}
}