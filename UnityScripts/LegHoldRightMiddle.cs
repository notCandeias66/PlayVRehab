using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHoldRightMiddle : ObstacleBase
{
    public override string ObstacleCode => "LHRM";

    public override int GetLength() => 2;

    public override void RunGimmick()
    {
        Debug.Log("Leg Hold Right Middle Obstacle");
    }
}
