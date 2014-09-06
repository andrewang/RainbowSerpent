using UnityEngine;

public class ScalePosition : Scale
{
	override protected void ApplyScale()
	{		
		Vector3 position = this.transform.localPosition;
		position.x *= this.widthScale;
		position.y *= this.heightScale;		
		this.transform.localPosition = position;
	}
	
}


