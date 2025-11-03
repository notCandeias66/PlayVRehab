using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchRight : ObstacleBase
{
    public override string ObstacleCode => "CR";

    public override int GetLength() => 8;

    public override void RunGimmick()
    {
        Debug.Log("Crouch Right Obstacle");
    }
}
