#pragma warning disable 0618,0619
using UnityEngine;

[ExecuteInEditMode]
public class UITiledSprite : UISlicedSprite
{
	public override Type type
	{
		get
		{
			return Type.Tiled;
		}
	}
}
