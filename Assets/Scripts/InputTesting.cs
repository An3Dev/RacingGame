using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class InputTesting : MonoBehaviour
{
    CarController carController;

    private void Awake()
    {
        carController = GetComponent<CarController>();

        StartCoroutine(FullGas(60, 0));
        StartCoroutine(Turn(1, -1, 0));
        StartCoroutine(Turn(1, 1, 1));
        StartCoroutine(Turn(1, -1, 2));
        StartCoroutine(Turn(1, 1, 3));
        StartCoroutine(Turn(3, -1, 4));
        StartCoroutine(Turn(5, 1, 7));
        StartCoroutine(Turn(60, -1, 12));
        Invoke(nameof(TakeScreenshot), 23);
    }

    void TakeScreenshot()
    {
        ScreenCapture.CaptureScreenshot("Cybertrueno" + TestingManager.frameRate+".png");
        //TestingManager.frameRate += 10;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator Turn(float time, float direction, float delay)
    {
        yield return new WaitForSeconds(delay);
        float currentTime = 0;
        while(currentTime < time)
        {
            carController.ReceiveSteeringInput(direction);
            currentTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FullGas(float time, float delay)
    {
        yield return new WaitForSeconds(delay);
        float currentTime = 0;
        while (currentTime < time)
        {
            carController.ReceiveGasInput(1);
            currentTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private void Update()
    {
        
    }
}
