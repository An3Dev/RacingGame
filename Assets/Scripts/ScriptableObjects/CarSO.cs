using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "UniqueObject")]
public class UniqueObject : ScriptableObject
{
    public int uniqueID;
    public new string name;
    public Texture2D image;
    public GameObject prefab;
}
