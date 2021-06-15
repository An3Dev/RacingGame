using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public RectTransform playerInMap;
    public RectTransform UITop, UILeft;
    public Transform worldTop, worldLeft;
    public Transform mapWorldCenter;
    private Vector3 normalized, mapped;
    private Transform car;
    private float worldToUIRatioY, worldToUIRatioX;

    private void Start()
    {
        worldToUIRatioY = worldTop.localPosition.z / UITop.localPosition.y;
        worldToUIRatioX = worldLeft.localPosition.x / UILeft.localPosition.x;

        car = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        transform.position = car.position;
        transform.rotation = car.rotation;
        
        Vector3 relativePosition = mapWorldCenter.InverseTransformPoint(this.transform.position);

        playerInMap.localPosition = new Vector3(relativePosition.x / worldToUIRatioX, relativePosition.z / worldToUIRatioY);

        playerInMap.localRotation = Quaternion.Euler(new Vector3(0, 0, -transform.rotation.eulerAngles.y + 180));
    }
}