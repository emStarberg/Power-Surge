using Godot;
using System;

public interface IPlayerAttack
{
    void Activate(string dir); // Called when attack is to happen
    String AttackName { get; set; }
}