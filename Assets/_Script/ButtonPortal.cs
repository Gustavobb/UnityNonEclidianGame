using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPortal : Interactable
{
    public List<GameObject> portals;
    Animator anim;
    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    public override void Interact()
    {
        base.Interact();
        anim.SetTrigger("Press");
        foreach (GameObject portal in portals)
        {
            portal.gameObject.SetActive(!portal.gameObject.activeSelf);
        }
    }

    void OnCollisionEnter(Collision other) 
    {
        if (other.collider.tag == "Object") Interact();
    }
}
