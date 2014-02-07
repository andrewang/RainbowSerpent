using UnityEngine;
using System.Collections;

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
	}
	
	void PostRender() {
		if (this.takeShot) 
		{
			RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
			camera.targetTexture = rt;
			Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
			camera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
			camera.targetTexture = null;
			RenderTexture.active = null;
			Destroy(rt);
			byte[] bytes = screenShot.EncodeToPNG();
			string filename = ScreenShotName(resWidth, resHeight);
			System.IO.File.WriteAllBytes(filename, bytes);
			Debug.Log(string.Format("Took screenshot to: {0}", filename));
			this.takeShot = false;
		}
	}
}