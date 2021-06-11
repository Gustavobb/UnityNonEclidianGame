using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    PlayerController player;
    protected bool hasInteracted = false;
    
    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }
    public virtual void Interact()
    {
        hasInteracted = !hasInteracted;
    }
}
