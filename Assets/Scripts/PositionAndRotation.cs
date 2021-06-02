using System;
using UnityEngine;

[Serializable]
public struct PositionAndRotation
{
    public SaveableVector3 Position { get; }
    public SaveableVector3 Rotation { get; }

    public PositionAndRotation(Vector3 position, Vector3 rotation)
    {
        Position = new SaveableVector3(position.x, position.y, position.z);
        Rotation = new SaveableVector3(rotation.x, rotation.y, rotation.z);
    }

    public override string ToString()
    {
        return Position.ToString() + " " + Rotation.ToString();
    }

}
