using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : ObstacleBase
{
    public override string ObstacleCode => "GND";

    public override int GetLength() => 4;

    public override void RunGimmick()
    {

    }
}
