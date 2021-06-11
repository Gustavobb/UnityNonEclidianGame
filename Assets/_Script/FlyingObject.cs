using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingObject : GravityBody
{
    PlayerController player;
    bool hasInteracted = false;
    
    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void FixedUpdate() 
    {
        float step =  8.5f * Time.deltaTime;
        if (hasInteracted) transform.position = Vector3.MoveTowards(transform.position, player.holdPoint.transform.position, step);
    }

    public void Interact()
    {
        rb.velocity = new Vector3(0, 0, 0);
        if (hasInteracted) rb.AddForce(player.camera.transform.forward * 12, ForceMode.Impulse);

        hasInteracted = !hasInteracted;
    }

    void OnCollisionEnter(Collision other) 
    {
        if (hasInteracted && other.collider.tag != "Player" && other.collider.tag != "Portal") Interact();
    }
}
