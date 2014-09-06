using UnityEngine;

public class ScaleCollider : Scale
{
	override protected void ApplyScale()
	{
		// Set (box) collider size
		BoxCollider collider = this.collider as BoxCollider;
		if (collider == null) { return; }
				
		Vector3 centre = collider.center;
		Vector3 size = collider.size;
		centre.x *= this.widthScale;
		centre.y *= this.heightScale;
		size.x *= this.widthScale;
		size.y *= this.heightScale;
		
		collider.center = centre;
		collider.size = size;
		
		UISlider slider = this.GetComponent<UISlider>();
		if (slider != null)			
		{
			slider.fullSize = size;
		}
		
	}
	
}


