using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : ObstacleBase
{
    public override string ObstacleCode => "!";

    public override int GetLength() => 4;

    public override void RunGimmick()
    {

    }
}
