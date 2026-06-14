using System;
using UnityEngine;

public abstract class PlayerState
{
    public abstract void OnEnter(PlayerController player);
    public abstract void OnExit(PlayerController player);

    public abstract void Update(PlayerController player);
}
