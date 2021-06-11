using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    [SerializeField]
    bool isTraveller = false;
    public bool hasGravity = true;
    [HideInInspector]
    public Traveller traveller;
    [HideInInspector]
    public Vector3 gravityVelocity, velocity;
    protected PlanetAttractor atrractor;
    protected bool isGrounded = false;
    public float distanceToTheGround;
    public delegate void OnTravelled();
    
    public OnTravelled onTravelled;

    [HideInInspector]
    public Rigidbody rb;
    public float speed = 5f, gravity = -10f, runSpeed = 10f, jumpHeigth = 5f;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        distanceToTheGround = GetComponent<Collider>().bounds.extents.y;

        if (!hasGravity) 
            rb.velocity = new Vector3(Random.Range(-.3f, .3f), Random.Range(-.3f, .3f), Random.Range(-.3f, .3f));

        if (isTraveller)
            traveller = new Traveller(this);        
    }

    protected virtual void ApplyGravity() 
    {
        if (atrractor != null) atrractor.AttractBody(rb, transform);
        else if (hasGravity) 
        {
            rb.AddForce(Vector3.up * gravity);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, 1.5f*runSpeed);
        }
    }

    protected virtual void LateUpdate() 
    {
        if (isTraveller) traveller.LateUpdateTraveller();
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.GetComponent<Collider>().tag == "Planet")
            atrractor = other.transform.parent.GetComponent<Collider>().GetComponent<PlanetAttractor>();
        
        else if (other.GetComponent<Collider>().tag == "Portal" && isTraveller)
            traveller.OnEnterPortal(other.GetComponent<Collider>().gameObject.GetComponent<Portal>());
    }

    void OnTriggerExit(Collider other) 
    {
        if (other.GetComponent<Collider>().tag == "Planet")
            atrractor = null;
        
        else if (other.GetComponent<Collider>().tag == "Portal" && isTraveller)
            traveller.OnExitPortal();
    }
}
