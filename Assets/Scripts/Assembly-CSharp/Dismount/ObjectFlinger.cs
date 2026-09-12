#pragma warning disable 0618,0619
using UnityEngine;
using reLive;

namespace Dismount
{
	public class ObjectFlinger : MonoBehaviour
	{
		public Rigidbody flingee;

		private void Update()
		{
			if ((bool)flingee && SXInputManager.GetMouseButtonDown(1))
			{
				Transform transform = DismountGame.cameraManager.currentCamera.transform;
				GameObject gameObject = Object.Instantiate(flingee.gameObject) as GameObject;
				gameObject.transform.position = transform.position;
				gameObject.transform.rotation = transform.rotation;
				GameObject gameObject2 = GameObject.Find("SceneDynamic");
				if ((bool)gameObject2)
				{
					gameObject.transform.parent = gameObject2.transform;
					gameObject.AddComponent<RecordMeInReplay>();
				}
				Vector3 direction = transform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition).direction;
				gameObject.GetComponent<Rigidbody>().AddForce(30f * direction, ForceMode.VelocityChange);
			}
		}
	}
}
