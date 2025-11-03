using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchLeft : ObstacleBase
{
    public override string ObstacleCode => "CL";

    public override int GetLength() => 8;

    public override void RunGimmick()
    {
        Debug.Log("Crouch Left Obstacle");
    }
}

