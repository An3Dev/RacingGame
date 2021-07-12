using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Nameplate : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Transform lookTarget;

    float height = 5;

    [SerializeField] Transform followTarget;
    float lookRate = 0.04f;
    WaitForSeconds waitForSeconds;

    float maxScale = 0.5f;
    float minScale = 0.05f;

    float minDistance = 10;
    float maxDistance = 200;

    bool autoInitialize = false;

    private void Awake()
    {
        if (autoInitialize)
            Initialize(lookTarget, followTarget, name);
    }

    public void Initialize(Transform lookTarget, Transform followTarget, string name)
    {
        this.lookTarget = lookTarget;
        this.followTarget = followTarget;
        nameText.text = name;

        //waitForSeconds = new WaitForSeconds(lookRate);
        //StartCoroutine(nameof(Tick));
    }

    private void FixedUpdate()
    {
        transform.LookAt(lookTarget);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.position = followTarget.position + Vector3.up * height;
        //Debug.Log("Nameplate: " + lookTarget);
        float distance = Vector3.Distance(transform.position, lookTarget.position);
        float scale = Mathf.Lerp(minScale, maxScale, distance / maxDistance);
        transform.localScale = new Vector3(scale, scale, scale);
    }

    //private void OnDisable()
    //{
    //    StopCoroutine(nameof(Tick));
    //}
}
