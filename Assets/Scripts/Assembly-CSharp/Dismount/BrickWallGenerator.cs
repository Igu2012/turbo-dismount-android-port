#pragma warning disable 0618,0619
using System.Collections.Generic;
using UnityEngine;

namespace Dismount
{
	public class BrickWallGenerator : MonoBehaviour
	{
		public GameObject brickPrefab;

		public Vector3 brickScaling = Vector3.one;

		public Vector3 brickSpacing = Vector3.zero;

		public int rows = 10;

		public int bricksPerRow = 20;

		public bool buildJoints;

		public float jointBreakForce = 100f;

		private Vector3 brickOriginalScale = Vector3.one;

		private Vector3 brickNewScale = Vector3.one;

		private Vector3 brickSize = Vector3.one;

		private Vector3 brickTotalSize = Vector3.one;

		private List<GameObject> bricks = new List<GameObject>();

		private List<GameObject> groundBricks = new List<GameObject>();

		private bool isGenerated;

		private void Start()
		{
			Generate();
		}

		public void Generate()
		{
			if (isGenerated)
			{
				return;
			}
			GameObject gameObject = Object.Instantiate(brickPrefab, new Vector3(1000000f, 1000000f, 1000000f), Quaternion.identity) as GameObject;
			brickOriginalScale = gameObject.transform.localScale;
			brickNewScale = brickOriginalScale;
			brickNewScale.x *= brickScaling.x;
			brickNewScale.y *= brickScaling.y;
			brickNewScale.z *= brickScaling.z;
			gameObject.transform.localScale = brickNewScale;
			brickSize = gameObject.GetComponent<Collider>().bounds.size;
			Object.Destroy(gameObject);
			brickTotalSize = brickSize + brickSpacing;
			float num = (float)(bricksPerRow / 2) * (0f - brickTotalSize.x);
			float num2 = brickTotalSize.y * 0.5f;
			if (buildJoints)
			{
				float num3 = num;
				for (int i = 0; i < bricksPerRow; i++)
				{
					GameObject gameObject2 = Object.Instantiate(brickPrefab, base.transform.position + base.transform.right * num3 - base.transform.up * brickSize.y * 0.5f, base.transform.rotation) as GameObject;
					gameObject2.transform.localScale = brickNewScale;
					gameObject2.name = "GroundBrick";
					gameObject2.transform.parent = base.transform;
					gameObject2.GetComponent<Renderer>().enabled = false;
					gameObject2.GetComponent<Rigidbody>().isKinematic = true;
					gameObject2.GetComponent<Rigidbody>().useGravity = false;
					groundBricks.Add(gameObject2);
					num3 += brickTotalSize.x;
				}
				num3 = num;
			}
			for (int j = 0; j < rows; j++)
			{
				int num4 = bricksPerRow;
				float num5 = num;
				if (bricksPerRow > 1 && (j & 1) == 1)
				{
					num5 += brickTotalSize.x * 0.5f;
					if (!buildJoints)
					{
						num4--;
					}
				}
				for (int k = 0; k < num4; k++)
				{
					GameObject gameObject3 = Object.Instantiate(brickPrefab, base.transform.position + base.transform.right * num5 + base.transform.up * num2, base.transform.rotation) as GameObject;
					gameObject3.transform.localScale = brickNewScale;
					gameObject3.name = "WallBrick";
					gameObject3.transform.parent = base.transform;
					bricks.Add(gameObject3);
					num5 += brickTotalSize.x;
				}
				num2 += brickTotalSize.y;
			}
			if (!buildJoints)
			{
				return;
			}
			for (int l = 0; l < bricksPerRow; l++)
			{
				GameObject gameObject4 = groundBricks[l];
				GameObject gameObject5 = bricks[l];
				FixedJoint fixedJoint = gameObject4.AddComponent<FixedJoint>();
				fixedJoint.anchor = gameObject4.transform.position + gameObject4.transform.up * brickTotalSize.y * 0.4f;
				fixedJoint.connectedBody = gameObject5.GetComponent<Rigidbody>();
				fixedJoint.breakForce = jointBreakForce;
			}
			for (int m = 0; m < rows; m++)
			{
				for (int n = 0; n < bricksPerRow - 1; n++)
				{
					GameObject gameObject6 = bricks[m * bricksPerRow + n];
					GameObject gameObject7 = bricks[m * bricksPerRow + n + 1];
					FixedJoint fixedJoint2 = gameObject6.AddComponent<FixedJoint>();
					fixedJoint2.anchor = gameObject6.transform.position + gameObject6.transform.right * brickTotalSize.x * 0.4f;
					fixedJoint2.connectedBody = gameObject7.GetComponent<Rigidbody>();
					fixedJoint2.breakForce = jointBreakForce;
					if (m < rows - 1)
					{
						GameObject gameObject8 = bricks[(m + 1) * bricksPerRow + n];
						FixedJoint fixedJoint3 = gameObject6.AddComponent<FixedJoint>();
						fixedJoint3.anchor = gameObject6.transform.position + gameObject6.transform.up * brickTotalSize.y * 0.4f;
						fixedJoint3.connectedBody = gameObject8.GetComponent<Rigidbody>();
						fixedJoint3.breakForce = jointBreakForce;
					}
				}
			}
			isGenerated = true;
		}

		private void OnDismountStarted()
		{
		}
	}
}
