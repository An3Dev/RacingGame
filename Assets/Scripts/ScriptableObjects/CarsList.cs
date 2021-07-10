using UnityEngine;

[CreateAssetMenu(fileName = "CarsList", menuName = "ScriptableObjects/CarsList")]
public class CarsList : ScriptableObject
{
    public GameObject[] carPrefabsList;
}
