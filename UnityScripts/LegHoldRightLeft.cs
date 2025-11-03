using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHoldRightLeft : ObstacleBase
{
    public override string ObstacleCode => "LHRL";

    public override int GetLength() => 2;

    public override void RunGimmick()
    {
        Debug.Log("Leg Hold Right Left Obstacle");
    }
}
