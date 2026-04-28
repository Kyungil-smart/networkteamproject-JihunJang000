using System;
using UnityEngine;

//input Systemを VContainerと一緒に使う為の規格

// 다양한 인풋 시스템을 넣기 위해 구현 틀 마련. 
public interface IInputProvider
{
    // 連続的な入力値
    Vector2 MoveDirection { get; }
    Vector2 LookDirection { get; }
    bool IsSprinting { get; }

    // 単発的な event
    event Action OnJumpEvent;
    event Action OnBasicAttackEvent;
    event Action OnSkillQEvent;
    event Action OnSkillEEvent;
    event Action OnSkillREvent;
    event Action OnDashEvent;
}


