using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    public float Mass = 30; // | kg | not being used 
    public float Radius = 0.34f; // | m |


    public Vector3 Weight;
    public float AngularVelocity; // | rad/s |

    public Transform Child;

    private void Start()
    {
        Child = transform.GetChild(0);
    }

    public void Update()
    {
        // TODO: clamp the angular velocty
        Child.rotation *= Quaternion.AngleAxis(AngularVelocity, Vector3.right);
        //var rotation = transform.rotation.eulerAngles;
        //rotation.x += AngularVelocity;
        //transform.rotation = Quaternion.Euler(rotation);
    }

    public void Rotate(float steeringAngle)
    {
        //transform.rotation *= Quaternion.AngleAxis(steeringAngle, Vector3.up);
        var localRotation = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(localRotation.x, steeringAngle * Mathf.Rad2Deg, localRotation.z);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, Radius);
    }
}
