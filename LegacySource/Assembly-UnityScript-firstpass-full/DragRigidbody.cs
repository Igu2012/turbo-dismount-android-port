using System.Collections;
using UnityEngine;

[AddComponentMenu("Physics/Drag Rigidbody")]
public class DragRigidbody : MonoBehaviour
{
    public float spring = 50f;
    public float damper = 5f;
    public float drag = 10f;
    public float angularDrag = 5f;
    public float distance = 0.2f;
    public bool attachToCenterOfMass;

    private SpringJoint springJoint;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Camera activeCamera = FindCamera();
        if (activeCamera == null)
            return;

        RaycastHit hitInfo;
        if (!Physics.Raycast(activeCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, 100f))
            return;

        Rigidbody hitBody = hitInfo.rigidbody;
        if (hitBody == null || hitBody.isKinematic)
            return;

        if (springJoint == null)
        {
            GameObject dragger = new GameObject("Rigidbody dragger");
            Rigidbody draggerBody = dragger.AddComponent<Rigidbody>();
            draggerBody.isKinematic = true;
            springJoint = dragger.AddComponent<SpringJoint>();
        }

        springJoint.transform.position = hitInfo.point;
        springJoint.anchor = attachToCenterOfMass
            ? springJoint.transform.InverseTransformPoint(hitBody.transform.TransformPoint(hitBody.centerOfMass))
            : Vector3.zero;
        springJoint.spring = spring;
        springJoint.damper = damper;
        springJoint.maxDistance = distance;
        springJoint.connectedBody = hitBody;

        StartCoroutine(DragObject(hitInfo.distance));
    }

    private IEnumerator DragObject(float hitDistance)
    {
        Rigidbody connectedBody = springJoint != null ? springJoint.connectedBody : null;
        if (connectedBody == null)
            yield break;

        float oldDrag = connectedBody.drag;
        float oldAngularDrag = connectedBody.angularDrag;
        connectedBody.drag = drag;
        connectedBody.angularDrag = angularDrag;

        Camera activeCamera = FindCamera();
        while (Input.GetMouseButton(0) && springJoint != null && activeCamera != null)
        {
            Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);
            springJoint.transform.position = ray.GetPoint(hitDistance);
            yield return null;
        }

        if (connectedBody != null)
        {
            connectedBody.drag = oldDrag;
            connectedBody.angularDrag = oldAngularDrag;
        }

        if (springJoint != null)
            springJoint.connectedBody = null;
    }

    private Camera FindCamera()
    {
        return Camera.main;
    }
}
