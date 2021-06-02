using System.Collections.Generic;
using UnityEngine;

public class GhostCar : MonoBehaviour
{
    Rigidbody rb;
    bool isMoving = false;
    List<PositionAndRotation> positionAndRotList = new List<PositionAndRotation>();

    int currentPosAndRotIndex = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetPosList(List<PositionAndRotation> list)
    {
        this.positionAndRotList = list;
        if (list == null || list.Count == 0)
        {
            gameObject.SetActive(false);
        }
    }
    public void StartMoving()
    {
        isMoving = true;
    }

    // this is called on a fixed time step of 0.02 milliseconds(50 fps)
    private void FixedUpdate()
    {
        if (isMoving)
        {
            MoveToNextPosition();
        }
    }

    void MoveToNextPosition()
    {
        // if this is the last index of the position and rotation list, then set isMoving to false;
        if (currentPosAndRotIndex >= positionAndRotList.Count)
        {
            isMoving = false;
            return;
        }
        PositionAndRotation pr = positionAndRotList[currentPosAndRotIndex];

        // use move position so that the rigidbody interpolates the movement.
        // For interpolation to work with Move Position, the rigidbody must be kinematic.
        rb.MovePosition(new Vector3(pr.Position.x, pr.Position.y, pr.Position.z));
        rb.MoveRotation(Quaternion.Euler(pr.Rotation.x, pr.Rotation.y, pr.Rotation.z));
        currentPosAndRotIndex++;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Checkpoint>() != null && other.GetComponent<Checkpoint>().GetIsFinishCheckpoint())
        {
            RaceManager.Instance.OnGhostFinished();
        }
    }
}
