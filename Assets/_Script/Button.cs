using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : Interactable
{
    Animator animator;
    Door linkedDoorScript;

    public GameObject panelOff, panelOn, linkedDoor;

    
    enum Type {Door, PlayerMove};
    [SerializeField]
    Type type;
    public PlayerMover playerMover;

    protected override void Start()
    {
        base.Start();
        if (linkedDoor != null) linkedDoorScript = linkedDoor.GetComponent<Door>();
        animator = GetComponent<Animator>();
    }

    public override void Interact()
    {
        base.Interact();
        animator.SetTrigger("Press");

        if (type == Type.PlayerMove) playerMover.MovePlayer();
        else HandleDoor();
    }

    void HandleDoor()
    {
        panelOff.SetActive(!hasInteracted);
        panelOn.SetActive(hasInteracted);
        linkedDoorScript.DoorAction(hasInteracted);
    }

    void OnCollisionEnter(Collision other) 
    {
        if (other.collider.tag == "Object") Interact();
    }
}
