using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态机
/// </summary>
public class PlayerStateMachine:MonoBehaviour
{
    public State currentState = null;
    protected PlayerController playerController;

    public void Initialize(PlayerController _player, State intializtionState)
    {
        currentState = intializtionState;
        playerController = _player;
        currentState.Enter();
    }

    public void ChangeState()
    {
        var nextStateEnum = this.currentState.ChangeState();
        if (nextStateEnum == PlayerState.None) return;

        //TODO 进行状态的转换
        if (!this.playerController.playerStates.ContainsKey(nextStateEnum)) return;
     

        var nextState = this.playerController.playerStates[nextStateEnum];
        this.currentState.Exit();
        this.currentState = nextState;
        this.currentState.Enter();

    }

}
