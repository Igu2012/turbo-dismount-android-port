#pragma warning disable 0618,0619
using Dismount;
using Dismount.Vehicular;
using UnityEngine;

namespace Vehicles
{
	public class TestDrop : MonoBehaviour
	{
		private Vehicle vehicle;

		private MrDismount character;

		private string characterName;

		private void Start()
		{
			if ((bool)DismountGame.instance)
			{
				return;
			}
			Vehicle[] array = Object.FindObjectsOfType(typeof(Vehicle)) as Vehicle[];
			MrDismount[] array2 = Object.FindObjectsOfType(typeof(MrDismount)) as MrDismount[];
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					array[i].gameObject.SetActive(false);
				}
			}
			vehicle = array[0];
			Transform transform = vehicle.transform;
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j].transform.parent.parent == transform)
				{
					if (!character)
					{
						character = array2[j];
						characterName = character.transform.parent.name + " / " + character.name;
					}
					else
					{
						array2[j].gameObject.SetActive(false);
					}
				}
				else
				{
					array2[j].gameObject.SetActive(false);
				}
			}
			vehicle.gameObject.SendMessage("WakeUp");
			character.AttachToVehicle(vehicle.gameObject);
			if (!character.kinematicDismountSetup)
			{
				character.SetKinematic(false);
				character.transform.parent = null;
			}
			Invoke("WakeCharacter", 1f);
		}

		private void WakeCharacter()
		{
			if (character.kinematicDismountSetup)
			{
				character.transform.parent = null;
			}
			character.SendMessage("OnDismountStarted");
		}

		private void OnGUI()
		{
			if ((bool)vehicle)
			{
				GUI.Label(new Rect((float)Screen.width * 0.5f - 100f, 10f, 200f, 40f), vehicle.name + " / " + characterName);
			}
			if (GUI.Button(new Rect(Screen.width - 110, 10f, 100f, 40f), "Reset"))
			{
				Application.LoadLevel(Application.loadedLevel);
			}
		}

		private void OnDrawGizmos()
		{
		}
	}
}
