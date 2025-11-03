using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHoldLeftRight : ObstacleBase
{
    public override string ObstacleCode => "LHLR";

    public override int GetLength() => 2;

    public override void RunGimmick()
    {
        Debug.Log("Leg Hold Left Right Obstacle");
    }
}
