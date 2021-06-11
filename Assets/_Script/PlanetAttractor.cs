using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetAttractor : MonoBehaviour
{
    public float gravity = -12f;
    [HideInInspector]
    public Vector3 gravityUp;

    public void AttractBody(Rigidbody rb, Transform bodyTransform)
    {
        gravityUp = (bodyTransform.position - transform.position).normalized;
        rb.AddForce(gravityUp * gravity);
        bodyTransform.rotation = Quaternion.FromToRotation(bodyTransform.up, gravityUp) * bodyTransform.rotation;
    }
}
