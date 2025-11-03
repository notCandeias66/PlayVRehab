using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHoldLeftMiddle : ObstacleBase
{
    public override string ObstacleCode => "LHLM";

    public override int GetLength() => 2;

    public override void RunGimmick()
    {
        Debug.Log("Leg Hold Left Middle Obstacle");
    }
}
