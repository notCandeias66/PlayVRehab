using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinnishLine : ObstacleBase
{
    public override string ObstacleCode => "END";

    public override int GetLength() => 4;

    public override void RunGimmick()
    {
        Debug.Log("Finnish Line Obstacle");
    }
}
