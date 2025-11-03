using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHoldLeftLeft : ObstacleBase
{
    public override string ObstacleCode => "LHLL";

    public override int GetLength() => 2;

    public override void RunGimmick()
    {
        Debug.Log("Leg Hold Left Left Obstacle");
    }
}
