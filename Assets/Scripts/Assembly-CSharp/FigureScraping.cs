#pragma warning disable 0618,0619
using Dismount;
using Dismount.Vehicular;
using UnityEngine;

public class FigureScraping : MonoBehaviour
{
	public AudioClip snowSkid;

	public GameObject parcels;

	private Transform parcelTransform;

	public void OnLevelLoaded()
	{
		if (parcels != null && DismountGame.playerState.currentVehicleItemId == "vehicle.splitvan")
		{
			Transform parent = DismountGame.playerState.currentVehicleInstance.transform;
			GameObject gameObject = Object.Instantiate(parcels) as GameObject;
			parcelTransform = gameObject.transform;
			parcelTransform.parent = parent;
			parcelTransform.localPosition = Vector3.zero;
			parcelTransform.localRotation = Quaternion.identity;
			parcelTransform.localScale = Vector3.one;
			MeshRenderer[] componentsInChildren = parcelTransform.GetComponentsInChildren<MeshRenderer>();
			MeshRenderer[] array = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array)
			{
				Material material = meshRenderer.material;
				material.color = HSVToRGB(Random.Range(0f, 1f), Random.Range(0.6f, 0.85f), Random.Range(0.6f, 0.9f));
			}
		}
	}

	public void OnDismountStarted()
	{
		VehicleAudio vehicleAudio = Object.FindObjectOfType<VehicleAudio>();
		if ((bool)vehicleAudio && (bool)vehicleAudio.skidSound && (bool)snowSkid)
		{
			if (snowSkid != null)
			{
				vehicleAudio.skidSound.clip = snowSkid;
				vehicleAudio.skidSound.Play();
			}
			else
			{
				vehicleAudio.skidSound.enabled = false;
			}
		}
		if (parcelTransform != null)
		{
			Transform parent = GameObject.Find("SceneDynamic").transform;
			Rigidbody connectedBody = DismountGame.playerState.currentVehicleInstance.GetComponent<Rigidbody>();
			Rigidbody[] componentsInChildren = parcelTransform.GetComponentsInChildren<Rigidbody>();
			Rigidbody[] array = componentsInChildren;
			foreach (Rigidbody rigidbody in array)
			{
				FixedJoint fixedJoint = rigidbody.gameObject.AddComponent<FixedJoint>();
				fixedJoint.connectedBody = connectedBody;
				fixedJoint.breakForce = 10f;
				fixedJoint.breakTorque = 5f;
				rigidbody.transform.parent = parent;
			}
		}
	}

	public void OnDismountReset()
	{
	}

	private Color HSVToRGB(float H, float S, float V)
	{
		if (S == 0f)
		{
			return new Color(V, V, V);
		}
		if (V == 0f)
		{
			return Color.black;
		}
		Color black = Color.black;
		float num = H * 6f;
		int num2 = Mathf.FloorToInt(num);
		float num3 = num - (float)num2;
		float num4 = V * (1f - S);
		float num5 = V * (1f - S * num3);
		float num6 = V * (1f - S * (1f - num3));
		switch (num2)
		{
		case -1:
			black.r = V;
			black.g = num4;
			black.b = num5;
			break;
		case 0:
			black.r = V;
			black.g = num6;
			black.b = num4;
			break;
		case 1:
			black.r = num5;
			black.g = V;
			black.b = num4;
			break;
		case 2:
			black.r = num4;
			black.g = V;
			black.b = num6;
			break;
		case 3:
			black.r = num4;
			black.g = num5;
			black.b = V;
			break;
		case 4:
			black.r = num6;
			black.g = num4;
			black.b = V;
			break;
		case 5:
			black.r = V;
			black.g = num4;
			black.b = num5;
			break;
		case 6:
			black.r = V;
			black.g = num6;
			black.b = num4;
			break;
		}
		black.r = Mathf.Clamp(black.r, 0f, 1f);
		black.g = Mathf.Clamp(black.g, 0f, 1f);
		black.b = Mathf.Clamp(black.b, 0f, 1f);
		return black;
	}
}
