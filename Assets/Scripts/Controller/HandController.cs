using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{

    public bool isLeftHand = false;
    public bool isRightHand = true;

    public GameObject StretchSenseController;
    public GameObject Thumb;
    public GameObject Index;
    public GameObject Middle;
    public GameObject Ring;
    public GameObject Pinky;

    public List<GameObject> ThumbChild;
    public List<GameObject> IndexChild;
    public List<GameObject> MiddleChild;
    public List<GameObject> RingChild;
    public List<GameObject> PinkyChild;

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
        SSL_Circuit controller = null;
        if (isLeftHand)
        {
            controller = StretchSenseController.GetComponent<StretchSenseAPI>().getLeftGloveCircuit();
        }
        else if (isRightHand)
        {
            controller = StretchSenseController.GetComponent<StretchSenseAPI>().getRightGloveCircuit();
        }

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
        //We are generating training data, move character to reference position
        if (Position.Length == 16)
        {
            Thumb.transform.localEulerAngles = new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
            ThumbChild[0].transform.localEulerAngles = new Vector3(0, (float)Position[3], 0);
            ThumbChild[1].transform.localEulerAngles = new Vector3(0, (float)Position[3], 0);

            Index.transform.localEulerAngles = new Vector3((float)Position[12], 0, (float)Position[8]);
            IndexChild[0].transform.localEulerAngles = new Vector3(0, 0, (float)Position[4]);
            IndexChild[1].transform.localEulerAngles = new Vector3((float)Position[4], 0, 0);

            Middle.transform.localEulerAngles = new Vector3((float)Position[13], 0, (float)Position[9]);
            MiddleChild[0].transform.localEulerAngles = new Vector3(0, 0, (float)Position[5]);
            MiddleChild[1].transform.localEulerAngles = new Vector3(0, 0, (float)Position[5]);

            Ring.transform.localEulerAngles = new Vector3((float)Position[14], 0, (float)Position[10]);
            RingChild[0].transform.localEulerAngles = new Vector3(0, 0, (float)Position[6]);
            RingChild[1].transform.localEulerAngles = new Vector3(0, 0, (float)Position[6]);

            Pinky.transform.localEulerAngles = new Vector3((float)Position[15], 0, (float)Position[11]);
            PinkyChild[0].transform.localEulerAngles = new Vector3(0, 0, (float)Position[7]);
            PinkyChild[1].transform.localEulerAngles = new Vector3(0, 0, (float)Position[7]);
        }
        else 
        {
            Thumb.transform.localEulerAngles = new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
            ThumbChild[0].transform.localEulerAngles = new Vector3(0, (float)Position[3] , 0);
            ThumbChild[1].transform.localEulerAngles = new Vector3(0, (float)Position[3], 0);

            Index.transform.localEulerAngles = new Vector3(0, 0, (float)Position[4]);
            IndexChild[0].transform.localEulerAngles = new Vector3(0, 0, (float)Position[4]);
            IndexChild[1].transform.localEulerAngles = new Vector3(0, 0, (float)Position[4]);

            Middle.transform.localEulerAngles = new Vector3(0, 0, (float)Position[5]);
            MiddleChild[0].transform.localEulerAngles = new Vector3(0, 0, (float)Position[5]);
            MiddleChild[1].transform.localEulerAngles = new Vector3(0, 0, (float)Position[5]);

            Ring.transform.localEulerAngles = new Vector3(0, 0, (float)Position[6]);
            RingChild[0].transform.localEulerAngles = new Vector3(0, 0, (float)Position[6]);
            RingChild[1].transform.localEulerAngles = new Vector3(0, 0, (float)Position[6]);

            Pinky.transform.localEulerAngles = new Vector3(0, 0, (float)Position[7]);
            PinkyChild[0].transform.localEulerAngles = new Vector3(0, 0, (float)Position[7]);
            PinkyChild[1].transform.localEulerAngles = new Vector3(0, 0, (float)Position[7]);
        }
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
        if (Input.touchCount > 0 && lastTouch == 0)
        {
            controller.RecordPosition();
        }
        lastTouch = Input.touchCount;
    }


    bool moveThis = false;

    //Rotate character on screen
    void RotateCharacter()
    {

        //Determine where the touch starts (Only apply later movement to that hand)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(0).position.x > Screen.width / 2 && isRightHand) {
            //Hand is right hand and touch started on Right side of screen
            moveThis = true;
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(0).position.x < Screen.width / 2 && isLeftHand)
        {
            //Hand is left hand and touch started on Left side of the screen
            moveThis = true;
        }

        //Rotate selected hand
        if (moveThis && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            // Get movement of the finger since last frame
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;

            // Move object across XY plane
            transform.Rotate(0, 0, touchDeltaPosition.x * 0.1f);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
            moveThis = false;
        }
    }

}
