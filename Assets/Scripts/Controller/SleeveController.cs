using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleeveController : MonoBehaviour
{

    public GameObject StretchSenseController;
    public GameObject RightArm;
    public GameObject RightElbow;
    public GameObject LeftArm;
    public GameObject LeftElbow;

    private int lastTouch = 0;

    // Use this for initialization
    private void Awake()
    {
        //Set and lock frame rate
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

    // Update is called once per frame
    void Update()
    {

        double[] Position = null;
        SSL_Circuit controller = StretchSenseController.GetComponent<SSLBleAPI>().getCircuit();

        if (controller != null)
        {
            ManageTouchEvent(controller);
            Position = GetPositionData(controller);
            UpdateAvatar(Position);
        }

    }

    //Get Calibration or Calibrated positions for avatar
    double[] GetPositionData(SSL_Circuit controller)
    {
        if (controller.isCalibrating())
        {
            return controller.getTrainingTarget();
        }
        else //Running
        {
            return controller.getCalibratedData();
        }
    }

    //Set joint angles
    void UpdateAvatar(double[] Position)
    {
        //Set position
        RightArm.transform.localEulerAngles = new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
        RightElbow.transform.localEulerAngles = new Vector3(0, 0, (float)Position[3]);

        LeftArm.transform.localEulerAngles = new Vector3((float)Position[4], (float)Position[5], (float)Position[6]);
        LeftElbow.transform.localEulerAngles = new Vector3(0, 0, (float)Position[7]);
    }

    //Update UI based on touch feedback
    void ManageTouchEvent(SSL_Circuit controller)
    {
        if (controller.isCalibrating())
        {
            UpdateCalibration(controller);
        }
        else
        {
            RotateCharacter();
        }
    }

    //Increment through calibration positions
    void UpdateCalibration(SSL_Circuit controller)
    {
        //Test that touch has been captured
        if (Input.touchCount > 0 && lastTouch == 0)
        {
            controller.RecordPosition();
        }
        lastTouch = Input.touchCount;
    }

    //Rotate character on screen
    void RotateCharacter()
    {

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            // Get movement of the finger since last frame
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            // Move object across XY plane
            transform.Rotate(0, -touchDeltaPosition.x * 0.1f, 0);
        }
    }

}
