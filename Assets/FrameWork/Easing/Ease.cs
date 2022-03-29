using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public abstract class Ease
{
    public abstract float CalcAmount(float current, float start = 0f, float end = 1f);

}



public class EaseLinear : Ease
{
    public override float CalcAmount(float current, float start = 0f, float end = 1f)
    {
        return current / (end - start);
    }
}
