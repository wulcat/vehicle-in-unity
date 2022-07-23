using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public float Mass = 30; // | kg | not being used 
    public float Radius = 0.34f; // | m |


    public Vector3 Weight;
    public float AngularVelocity; // | rad/s |

    public void Update()
    {
        // TODO: clamp the angular velocty
        transform.rotation *= Quaternion.AngleAxis(AngularVelocity, Vector3.right);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, Radius);
    }
}
