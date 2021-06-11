using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Animator animator;
    enum InitialState {Opened, Closed};
    [SerializeField]
    InitialState initialState;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (initialState == InitialState.Opened) animator.SetTrigger("Open");
        else animator.SetTrigger("Close");
    }

    public void DoorAction(bool open)
    {
        if (open) animator.SetTrigger("Open");
        else animator.SetTrigger("Close");
    }
}
