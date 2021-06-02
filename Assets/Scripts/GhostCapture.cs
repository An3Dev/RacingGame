using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public class GhostCapture : MonoBehaviour
{
    public static GhostCapture Instance;
    public bool captureCarPosition = false;
    
    public GhostCar ghostCar;
    public TextAsset[] ghostCapturePositionAndRotationFiles;


    public Difficulty currentDifficulty;

    List<PositionAndRotation> positionAndRotList = new List<PositionAndRotation>();

    bool shouldRecordThisFrame = false;
    CarController car;
    float milliseconds;

    string fileName;

    string dataPath = "";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }

        if (!captureCarPosition)
        {
            currentDifficulty = GhostModeManager.currentDifficulty;
        }
    }

    private void Start()
    {
        car = CarSpawner.Instance.GetCurrentCar().GetComponent<CarController>();
        fileName = SceneManager.GetActiveScene().name + car.GetCarPresetName() + currentDifficulty.ToString() + ".bytes";
        dataPath = Application.dataPath + "\\GhostCarFiles\\" + fileName;
        if (!captureCarPosition)
        {
            positionAndRotList = Deserialize();
            ghostCar.SetPosList(positionAndRotList);

        } else
        {
            ghostCar.gameObject.SetActive(false);
        }
    }

    public void StartGhost()
    {
        if (captureCarPosition)
        {
            StartRecording();
        } else
        {
            // start ghost movement;
            ghostCar.StartMoving();
        }
    }

    private void FixedUpdate()
    {
        if (shouldRecordThisFrame)
        {
            RecordFrame();
        }
    }

    void StartRecording()
    {    
        shouldRecordThisFrame = true;

        //InvokeRepeating(nameof(RecordFrame), 0.0001f, milliseconds);
        Debug.Log("Started recording");
    }

    void RecordFrame()
    {
        positionAndRotList.Add(new PositionAndRotation(car.transform.position, car.transform.rotation.eulerAngles));
    }

    public void FinishedRace()
    {       
        if (captureCarPosition)
        {
            Invoke(nameof(StopRecording), 5);
        }
    }

    void StopRecording()
    {
        shouldRecordThisFrame = false;
        CancelInvoke();
        Debug.Log("Stopped recording");
        Serialize<PositionAndRotation>(positionAndRotList);
    }

    public List<PositionAndRotation> GetPositionAndRotList()
    {
        return Deserialize();
    }

    void Serialize<PositionAndRotation>(List<PositionAndRotation> list)
    {
        Stream stream = new FileStream(dataPath, FileMode.Create, FileAccess.Write, FileShare.None);
        IFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, list);
        stream.Close();
        Debug.Log("Serialized");
    }

    private List<PositionAndRotation> Deserialize()
    {
        TextAsset textAsset = new TextAsset();
        // loop through all files
        for(int i = 0; i < ghostCapturePositionAndRotationFiles.Length; i++)
        {
            // if this text asset corresponds to the current scene, car, and difficulty
            if (fileName.Equals(ghostCapturePositionAndRotationFiles[i].name + ".bytes"))
            {
                textAsset = ghostCapturePositionAndRotationFiles[i];
            }
        }
        Stream stream = new MemoryStream(textAsset.bytes);
        if (textAsset.text.Length <= 0)
        {
            return null;
        }

        IFormatter formatter = new BinaryFormatter();
        List<PositionAndRotation> list = (List<PositionAndRotation>)formatter.Deserialize(stream);
        stream.Close();

        return list;
    }
}
