using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public Transform carTarget;

    [SerializeField] Vector3 defaultDistanceFromCar = new Vector3(0, 0, -10);
    [SerializeField] Vector3 rotationOffset = new Vector3(30, 0, 0);

    [SerializeField] float lag = 1;

    Rigidbody carRb;

    private void Awake()
    {
        carRb = carTarget.GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 point = carTarget.TransformPoint(defaultDistanceFromCar);
        point.y = carTarget.position.y + defaultDistanceFromCar.y;
        Vector3 velocity = Vector3.zero;
        transform.position = Vector3.SmoothDamp(transform.position, point, ref velocity, lag);

        transform.LookAt(carTarget);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rotationOffset);
    }
}
