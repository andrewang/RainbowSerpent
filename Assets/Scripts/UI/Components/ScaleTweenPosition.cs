using UnityEngine;

public class ScaleTweenPosition : Scale
{
	override protected void ApplyScale()
	{		
		TweenPosition tween = this.GetComponent<TweenPosition>();
		if (tween == null) { return; }
		
		Vector3 from = tween.from;
		from.x *= this.widthScale;
		from.y *= this.heightScale;	
		tween.from = from;			

		Vector3 to = tween.to;
		to.x *= this.widthScale;
		to.y *= this.heightScale;		
		tween.to = to;
	}
	
}


