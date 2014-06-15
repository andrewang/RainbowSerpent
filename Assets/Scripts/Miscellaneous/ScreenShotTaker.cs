using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class ScreenShotTaker : MonoBehaviour 
{
	public void TakeScreenShot(string fileName, int width, int height, int centreX, int centreY, Action<Texture2D> completed)
	{		
		// incoming coordinates are based on a centre origin and need to be converted to pixel space
		// where 0,0 is in the lower left
		int leftX = centreX + (Screen.width - width) / 2;
		int bottomY = centreY + (Screen.height - height) / 2;
		
		StartCoroutine(TakeScreenShotCoroutine(fileName, width, height, leftX, bottomY, completed));
	}
	
	IEnumerator TakeScreenShotCoroutine(string fileName, int width, int height, int leftX, int bottomY, Action<Texture2D> completed)
	{
		// This yield is necessary to make sure that it is safe to read the screen buffer.
		yield return new WaitForEndOfFrame();
		
		Texture2D tex = new Texture2D(width, height);
		tex.ReadPixels(new Rect(leftX, bottomY, width, height), 0, 0);
		tex.Apply();
		
		// wait till end of next frame to apply the texture to anything.
		yield return new WaitForEndOfFrame();		
		completed(tex);
		
		// and now save the texture to a file.
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes(fileName, bytes);
		yield return null;
	}
}