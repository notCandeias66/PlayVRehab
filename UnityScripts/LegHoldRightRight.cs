using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHoldRightRight : ObstacleBase
{
    public override string ObstacleCode => "LHRR";

    public override int GetLength() => 2;

    public override void RunGimmick()
    {
        Debug.Log("Leg Hold Right Right Obstacle");
    }
}
