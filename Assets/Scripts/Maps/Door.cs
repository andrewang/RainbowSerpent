using System;

public class Door : Wall
{
	private SerpentConsts.Dir openableSide;
	private bool open;
	
	// Doors need a reference to their sprite to do animations.
	public UISprite Sprite
	{
		get;
		set;
	}
	
	public Door (SerpentConsts.Dir dir)
	{
		this.open = false;
		this.openableSide = dir;
	}
	
	public bool OpenableFrom( SerpentConsts.Dir direction )
	{
		return (direction == this.openableSide);
	}
	
	public void Open()
	{
		if (this.open) { return; }
		
		this.open = true;
		// do quick opening animation? 
	}
	
	public void Close()
	{
		if (this.open == false) { return; }
		
		this.open = false;
		// do quick closing animation?
	}
}


