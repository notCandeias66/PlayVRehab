using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpRight : ObstacleBase
{
    public override string ObstacleCode => "JR";

    public override int GetLength() => 4;

    public override void RunGimmick()
    {
        Debug.Log("Jump Right Obstacle");
    }
}
