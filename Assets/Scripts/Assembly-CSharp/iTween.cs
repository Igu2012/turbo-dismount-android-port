#pragma warning disable 0618,0619
using System.Collections;
using UnityEngine;

public class iTween : MonoBehaviour
{
    public enum EaseType { linear }

    public static Hashtable Hash(params object[] values)
    {
        Hashtable table = new Hashtable();
        for (int i = 0; i + 1 < values.Length; i += 2)
            table[values[i]] = values[i + 1];
        return table;
    }

    public static void MoveTo(GameObject target, Hashtable args)
    {
        if (target == null || args == null) return;
        if (args.ContainsKey("x"))
        {
            Vector3 position = target.transform.position;
            position.x = System.Convert.ToSingle(args["x"]);
            target.transform.position = position;
        }
        else if (args.ContainsKey("position"))
        {
            object value = args["position"];
            if (value is Vector3) target.transform.position = (Vector3)value;
            else if (value is Transform) target.transform.position = ((Transform)value).position;
        }
    }
}
