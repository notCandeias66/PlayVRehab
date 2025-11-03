using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObstacleBase : MonoBehaviour
{
    public abstract string ObstacleCode { get; }

    public virtual int GetLength() => 1;


    protected int delayValue = 0;
    public int GetDelayValue() => delayValue;
    public virtual void SetDelay(int delay)
    {
        this.delayValue = delay;
    }

    public Dictionary<int, string> attemptsData = new Dictionary<int, string>();

    public abstract void RunGimmick();
}
