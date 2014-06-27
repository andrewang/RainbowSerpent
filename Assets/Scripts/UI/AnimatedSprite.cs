using System;
using UnityEngine;

public class AnimatedSprite : MonoBehaviour
{
	public event Action<AnimatedSprite> TranslationFinished = null;
	
	public UISprite Sprite = null;
	
	private void Done()
	{
		if (this.TranslationFinished != null)
		{
			this.TranslationFinished(this);
		}
	}
	
}

