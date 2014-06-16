using System;

public class Door : Wall
{
	private SerpentConsts.Dir openableSide;
	private bool open;
	
	public SerpentConsts.LevelState LevelStateRequired { get; set; }
	
	// Doors need a reference to their sprite to do animations.
	public UISprite Sprite
	{
		get;
		set;
	}
	
	public Door (SerpentConsts.Dir dir)
	{
		this.LevelStateRequired = SerpentConsts.LevelState.None;
		this.open = false;
		this.openableSide = dir;
	}
	
	public bool OpenableFrom( SerpentConsts.Dir direction )
	{
		if (this.LevelStateRequired != SerpentConsts.LevelState.None)
		{
			SerpentConsts.LevelState currentState = Managers.GameState.LevelState;
			if (currentState != this.LevelStateRequired)
			{
				return false;
			}
		}
	
		// Check for level state requirement
		return (direction == this.openableSide);
	}
	
	public void Open()
	{
		if (this.open) { return; }
		
		this.open = true;
		
		// do quick opening animation
		TweenScale.Begin(this.Sprite.gameObject, 0.1f, new UnityEngine.Vector3(0.01f, 1.0f));				
	}
	
	public void Close()
	{
		if (this.open == false) { return; }
		
		this.open = false;
		// do quick closing animation
		TweenScale.Begin(this.Sprite.gameObject, 0.1f, new UnityEngine.Vector3(1.0f, 1.0f));				
		
	}
	
	public void Hide()
	{
		if (this.Sprite == null) { return; }
		
		this.Sprite.alpha = 0.0f;
	}
	
	public void Show()
	{
		if (this.Sprite == null) { return; }
		
		this.Sprite.alpha = 1.0f;
	}
}


