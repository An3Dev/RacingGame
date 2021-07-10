using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Nameplate : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;

    [SerializeField] Transform lookTarget;

    float height = 5;

    Transform followTarget;
    float lookRate = 0.04f;
    WaitForSeconds waitForSeconds;

    float maxScale = 5;
    float minScale = 0.5f;

    float minDistance = 10;
    float maxDistance = 500;

    private void Awake()
    {
        waitForSeconds = new WaitForSeconds(lookRate);
        //followTarget = transform.parent;
        // unparent 
        transform.parent = null;    
    }

    public void SetLookTarget(Transform target)
    {
        this.lookTarget = target;
        StartCoroutine(nameof(Tick));
    }

    public void SetFollowTarget(Transform target)
    {
        this.followTarget = target;
    }

    public void SetText(string text)
    {
        nameText.text = text;
    }

    IEnumerator Tick()
    {
        while(true)
        {
            if (lookTarget = null)
            {
                yield return null;
            }
            transform.LookAt(lookTarget);
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
            transform.position = followTarget.position + Vector3.up * height;
            Debug.Log("Nameplate: " + lookTarget);
            float distance = Vector3.Distance(transform.position, lookTarget.position);
            float scale = Mathf.Lerp(minScale, maxScale, distance / maxDistance);
            transform.localScale = new Vector3(scale, scale, scale);
            yield return waitForSeconds;
        }
    }
}
