using UnityEngine;

public class ScaleClipping : Scale
{
	override protected void ApplyScale()
	{		
		UIPanel panel = this.GetComponentInChildren<UIPanel>();
		if (panel == null) { return; }
		
		Vector4 clipRange = panel.clipRange;
		// remember, size of clip range is ZW.
		
		clipRange.z *= this.widthScale;
		clipRange.y *= this.heightScale;
		
		panel.clipRange = clipRange;
	}
	
}


