using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public IdleState(PlayerController playerController, StateMachine stateMachine, string animTriggerName, Animator animator) : base(playerController, stateMachine, animTriggerName, animator) { }

    public override void EnterState() => base.EnterState();

    public override void ExitState() => base.ExitState();

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (playerController.velocity.x != 0 || playerController.velocity.z != 0) stateMachine.ChangeState(playerController.walkState);
    }
}