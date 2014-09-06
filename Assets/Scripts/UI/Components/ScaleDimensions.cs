using UnityEngine;

public class ScaleDimensions : Scale
{
	override protected void ApplyScale()
	{		
		Vector3 scale = this.transform.localScale;
		scale.x *= this.widthScale;
		scale.y *= this.heightScale;
		this.transform.localScale = scale;
	}
	
}


