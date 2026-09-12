#pragma warning disable 0618,0619
using UnityEngine;

[ExecuteInEditMode]
public class UISlicedSprite : UISprite
{
	public override Type type
	{
		get
		{
			return Type.Sliced;
		}
	}
}
