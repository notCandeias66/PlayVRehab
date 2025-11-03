using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpMiddle : ObstacleBase
{
    public override string ObstacleCode => "JM";

    public override int GetLength() => 4;

    public override void RunGimmick()
    {
        Debug.Log("Jump Middle Obstacle");
    }
}
