using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public struct SaveableVector3
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public SaveableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
