using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointNormalCheck : MonoBehaviour
{
    public float Length = 100;

    public Vector3 HitPoint;

    public Vector3 HitNormal;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Length))
        {
            HitPoint = hit.point;
            HitNormal = hit.normal;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * Vector3.Distance(transform.position, HitPoint));
        Gizmos.DrawSphere(transform.position, 0.1f);
        
        Gizmos.color = Color.yellow;
        // Gizmos.DrawRay(HitPoint, HitNormal * 100);
        Gizmos.DrawSphere(HitPoint, 0.1f);

        Gizmos.color = Color.red;

        // var angle = Vector3.Angle(transform.forward, HitNormal);
        // var scalar = HitPoint.magnitude * Mathf.Cos(angle * Mathf.PI / 180);
        // var vector = (HitPoint / HitPoint.magnitude) * scalar;
        //
        // Gizmos.DrawRay(HitPoint, vector * 100);

        // var p = HitPoint - HitNormal;
        // Gizmos.DrawSphere(p, 0.1f);        
        var p = HitPoint.normalized;
        var dot = Vector3.Dot(transform.forward, HitNormal);
        // var magSquare = Mathf.Sqrt(HitNormal.x * HitNormal.x + HitNormal.y * HitNormal.y  + HitNormal.z * HitNormal.z );
        var magSquare = Vector3.Dot(HitNormal, HitNormal);
        var vector = transform.forward - (dot / magSquare) * HitNormal;

        Gizmos.DrawRay(HitPoint, (dot / magSquare) * HitNormal * 100);
        Gizmos.DrawRay(HitPoint, vector * 100);

        // Gizmos.DrawRay(HitPoint, -HitNormal * 100);
        // Gizmos.DrawRay(HitPoint, (transform.forward  - HitNormal).normalized * 100);



        // Vector3 planeVector = new Vector3();
        //
        // planeVector = Vector3.Cross(HitPoint, HitNormal);

        // Gizmos.DrawRay(HitPoint, Vector3.Cross(HitNormal, HitPoint) * 100);
        // Gizmos.color = Color.black;
        // Gizmos.DrawRay(HitPoint, GetProjection(HitPoint, HitNormal) * 100);
        // Gizmos.color = Color.blue;   
        // Gizmos.DrawRay(HitPoint, GetProjection(HitPoint, Vector3.Cross(HitNormal, HitPoint)) * 100);
    }

    
    Vector3 GetProjection(Vector3 point, Vector3 normal)
    {
        var dot = Vector3.Dot(point, normal);
        var mag = normal.magnitude * normal.magnitude;
        var proj = (dot / mag) * normal;
        return proj;
    }
}

// wulcat — Today at 1:24 AM
// Hello i am trying to learn some vector maths for game development. I am currently stuck with this problem. I need to get vector in same direction but projected on the plane.
// Image
// Bot
//  pinned 
// a message
//  to this channel. See all 
// pinned messages
// .
//  — Today at 1:24 AM
// themateo713 — Today at 1:27 AM
// that's not vector projection. That's some sort of collision with the plane and you want to remove the component orthogonal to the plane, so that the ray continues "sliding on the plane"
// also the p you drew doesn't really describe a vector, more so a direction
// if you draw the vector with the same direction but starting from the origin (thus going below to the right), then the red vector would be the projection of p on q
// and then you can use all the vector projection theory you want
// wulcat — Today at 1:29 AM
// Let me quick search the removal component part.
// themateo713 — Today at 1:29 AM
// that would work if you're already in a basis created from vectors that define the plane
// otherwise just use a regular projection formula
// wulcat — Today at 1:30 AM
// Alright
// themateo713 — Today at 1:30 AM
// otherwise finding the orthogonal component to a random plane is definitely not the simplest solution
// wulcat — Today at 1:30 AM
// ayiii
// themateo713 — Today at 1:30 AM
// if you were to project on the x-y plane specifically, then just remove the z component
// but on a random plane there are better methods
// wulcat — Today at 1:32 AM
// the problem is the plane can be in any rotation
// Image
// that's what i initially had though to remove one component but as the rotations were preset, i was jumbled on it
// themateo713 — Today at 1:34 AM
// there's quite a few formulas
// Since this appears to be programming based, I'll say just pick the formula that's easiest for you to implement / is most efficient based on your implementation
// https://en.wikipedia.org/wiki/Vector_projection
// Vector projection
// The vector projection of a vector a on (or onto) a nonzero vector b, sometimes denoted
//
//
//
//
// proj
//
//
// b
//
//
//
// ⁡
//
// a
//
//
//
// {\displaystyle \operatorname {proj} _{\mathbf {b} }\mathbf {a} }
// (also known as the vect...
// Vector projection
// wulcat — Today at 1:42 AM
// So basically i have to find the angle using the direction vector and plane normal and then use that angle further to find for the projectiong
// right?
// themateo713 — Today at 1:43 AM
// just the start of the page gives you a formula that uses only scalar products (or dot product). No need to use angles here. Finding angles in 3D is pretty hard. And I don't know any way of doing so without dot products, but I'm no 3D expert
// you can do the scalar product easily since you probably represent the vectors in the usual x-y-z coordinate system
// wulcat — Today at 1:51 AM
// It says b is a non zero vector, so in our case will that be planes normal?
// Image
// themateo713 — Today at 1:55 AM
// yes so technically, I didn't specify that, but yes. You actually do the "component removal" part but without finding what you remove. If you take p and subtract the component orthogonal to the plane, you're left with a vector in the plane. So you can do p - (projection of p onto the plane's normal vector)
// Bot
// BOT
//  — Today at 2:06 AM
// @wulcat Has your question been resolved?
// wulcat — Today at 2:09 AM
// So here's my understanding,
//
// p1 = p - plane's normal
// proj (p) on q plane = (p1 . n / n . n) * n
// themateo713 — Today at 2:16 AM
// if we name n the vector normal to the plane, then proj(p) = p - (p . n) / (n . n) * n
// wulcat — Today at 2:17 AM
// omg >.<
// i misread that last in your last state ment hehe
// thanks alot!
// ❤️
// Image
// .close
// Bot
// BOT
//  — Today at 2:23 AM
// Channel closed
// Closed by @wulcat
//
// Use .reopen if this was a mistake.
//
// Please take a minute to participate in our survey if you haven't already!
// ﻿