#pragma warning disable 0618,0619
using UnityEngine;

namespace Dismount
{
	public class AdjustGuiScroll : MonoBehaviour
	{
		public enum Direction
		{
			Left = 0,
			Right = 1
		}

		public Camera selectCamera;

		public Direction scrollDirection;

		private DragCameraCenterOnObject dragCamera;

		private void Start()
		{
			dragCamera = selectCamera.GetComponent<DragCameraCenterOnObject>();
			if (dragCamera == null)
			{
				Debug.LogError("No DragCameraCenterOnObject script found in Camera");
			}
		}

		private void OnPress(bool isPressed)
		{
			if (isPressed)
			{
				switch (scrollDirection)
				{
				case Direction.Left:
					dragCamera.ScrollToNextItemLeft();
					break;
				case Direction.Right:
					dragCamera.ScrollToNextItemRight();
					break;
				}
			}
		}
	}
}
