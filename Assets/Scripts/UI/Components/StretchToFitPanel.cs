using UnityEngine;
using System.Collections;

//
// This component can be attached to a sprite to make it stretch (scale?) to be the same size as a panel the horizontal
// or vertical axes, or both.
//

public class StretchToFitPanel : MonoBehaviour 
{
	[SerializeField] private UIPanel panel;
	[SerializeField] private UISprite sprite;
	[SerializeField] private bool horizontal;
	[SerializeField] private float horizontalScale = 1.0f;
	[SerializeField] private bool vertical;
	[SerializeField] private float verticalScale = 1.0f;
	

	// Use this for initialization
	void Start () 
	{
		if (this.sprite == null)
		{
			this.sprite = this.gameObject.GetComponent<UISprite>();
		}
		
		Stretch();		
	}
	
	public void Stretch()
	{
		if (this.horizontal)
		{
			float width = panel.clipRange.z * this.horizontalScale;
			this.sprite.width = (int)width;
		}
		
		if (this.vertical)
		{		
			float height = panel.clipRange.w * this.verticalScale;
			this.sprite.height = (int)height;
		}
	}
}
