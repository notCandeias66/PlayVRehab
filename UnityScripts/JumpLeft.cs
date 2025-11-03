using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpLeft : ObstacleBase
{
    public override string ObstacleCode => "JL";

    public override int GetLength() => 4;

    public override void RunGimmick()
    {
        Debug.Log("Jump Left Obstacle");
    }
}
