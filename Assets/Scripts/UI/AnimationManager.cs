using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
	[SerializeField] ScriptCache spriteCache = null;
	
	private List<Color> randomColours = new List<Color> {Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.white, Color.yellow};
		
	public AnimationManager ()
	{
	
	}

	public void PlayRandomAnimation(Vector3 position)
	{
		List<AnimatedSprite> sprites = GenerateRandomAnimationSprites();
		
		AssignRandomColour(sprites);
		AssignRandomRotation(sprites);
		SetTranslation(sprites, position);
				
		// animate the sprites?  or should that be in the GenerateRandomAnimation?
		
		// should the sprites change alpha?
		// should the sprites change colour?
		// are the two compatible?
	}
	
	private List<AnimatedSprite> GenerateRandomAnimationSprites()
	{
		List<AnimatedSprite> sprites = new List<AnimatedSprite>();
		
		int randomChoice = 0; 
		
		if (randomChoice == 0)
		{
			for( int i = 0; i < 5; ++i)
			{
				AnimatedSprite sprite = this.spriteCache.GetObject<AnimatedSprite>();
				sprites.Add( sprite );
			}
		}
		
		return sprites;
	}
	
	private void AssignRandomRotation(List<AnimatedSprite> sprites)
	{
		// Assumption: the sprites are all supposed to be radially aligned equally around the circle
		float numSprites = (float) sprites.Count;
		float degreesRotationPerSprite = 360.0f / numSprites;
		float currRotation = (float)UnityEngine.Random.Range( 0, 360 );
		foreach( AnimatedSprite sprite in sprites )
		{
			sprite.transform.Rotate( 0.0f, 0.0f, currRotation );
			currRotation += degreesRotationPerSprite;
			if (currRotation > 360.0f) { currRotation -= 360.0f; }
		}
	}
	
	private void SetTranslation(List<AnimatedSprite> sprites, Vector3 initialPosition)
	{
		// Set sprites to be above everything else
		initialPosition.z -= 5.0f;
		// And loop...
		foreach( AnimatedSprite sprite in sprites )
		{
			sprite.transform.localPosition = initialPosition;
			
			Vector3 eulerAngles = sprite.transform.localRotation.eulerAngles;
			// NOTE: this radians conversion is required because the animation system expects rotation counter-clockwise from horizontal,
			// while eulerAngles gives us a rotation clockwise from vertical.
			float radians = (90.0f - eulerAngles.z) * Mathf.Deg2Rad;
			Vector3 unitVector = new Vector3( Mathf.Sin(radians), Mathf.Cos(radians), 0.0f);
			Vector3 displacement = unitVector * Screen.width;
			TweenPosition tween = TweenPosition.Begin( sprite.gameObject, 3.0f, sprite.transform.localPosition + displacement );
			// This rather complicated setup is required in order to get a callback which contains a reference to the animated
			// sprite.  Note the first line here clears any previously set event.
			sprite.TranslationFinished -= AnimatedSpriteDone;			
			sprite.TranslationFinished += AnimatedSpriteDone;
			EventDelegate tweenFinished = new EventDelegate(sprite, "Done");
			tween.onFinished.Add( tweenFinished );		
		}
	}
	
	private void AnimatedSpriteDone( AnimatedSprite sprite )
	{
		this.spriteCache.ReturnObject<AnimatedSprite>( sprite.gameObject );
	}
	
	private void AssignRandomColour(List<AnimatedSprite> sprites)
	{
		int randomChoice = UnityEngine.Random.Range(0, this.randomColours.Count);
		Color colour = this.randomColours[randomChoice];
		foreach( AnimatedSprite sprite in sprites )
		{
			sprite.Sprite.color = colour;
		}
	}
}

