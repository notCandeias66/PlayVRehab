using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchMiddle : ObstacleBase
{
    public override string ObstacleCode => "CM";

    public override int GetLength() => 8;

    public override void RunGimmick()
    {
        Debug.Log("Crouch Middle Obstacle");
    }
}
