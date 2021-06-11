using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{   
    protected Animator animator;
    protected StateMachine stateMachine;
    protected float startTime;
    protected PlayerController playerController;
    string animTriggerName;
    
    public State(PlayerController playerController, StateMachine stateMachine, string animTriggerName, Animator animator)
    {
        this.animator = animator;
        this.playerController = playerController;
        this.stateMachine = stateMachine;
        this.animTriggerName = animTriggerName;
    }

    public virtual void EnterState()
    {
        animator.SetTrigger(animTriggerName);
        startTime = Time.time;
    }

    public virtual void ExitState()
    {
        // animator.SetTrigger(animTriggerName);
    }

    public virtual void LogicUpdate() {}

    public virtual void PhysicsUpdate() {}
}
