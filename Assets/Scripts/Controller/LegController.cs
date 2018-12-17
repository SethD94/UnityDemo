using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegController : MonoBehaviour 
{

    public GameObject StretchSenseController;
    public GameObject RightHip;
    public GameObject RightKnee;
    public GameObject LeftHip;
    public GameObject LeftKnee;

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
    double[] GetPositionData(SSL_Circuit controller) {
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
    void UpdateAvatar(double[] Position) {
       
        //Set position
        RightHip.transform.localEulerAngles = new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
        RightKnee.transform.localEulerAngles = new Vector3((float)Position[3], 0, 0);
        LeftHip.transform.localEulerAngles = new Vector3((float)Position[4], (float)Position[5], (float)Position[6]);
        LeftKnee.transform.localEulerAngles = new Vector3((float)Position[7], 0, 0);
    }

    //Update UI based on touch feedback
    void ManageTouchEvent(SSL_Circuit controller) {

        if (controller.isCalibrating())
        {
            UpdateCalibration(controller);
        }
        else {
            RotateCharacter();
        }
    }

    //Increment through calibration positions
    void UpdateCalibration(SSL_Circuit controller) {

        if (Input.touchCount > 0 && lastTouch == 0)
        {
            controller.RecordPosition();
        }
        lastTouch = Input.touchCount;
    }

    //Rotate character on screen
    void RotateCharacter() {
        
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            // Get movement of the finger since last frame
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            // Move object across XY plane
            transform.Rotate(0, -touchDeltaPosition.x * 0.1f, 0);
        }
    }

}
