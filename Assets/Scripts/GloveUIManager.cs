using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GloveUIManager : MonoBehaviour
{
    public GameObject StretchSenseController;

    private SSL_Circuit controller;
    public StretchSenseAPI StretchSenseApi;
    private Boolean left = false;
    private Boolean isRecord = false;

    public Sprite OffSprite;
    public Sprite OnSprite;
    public Button buttonLeftGlove;
    public Button buttonRightGlove;
    public Button buttonRecord;
    
    
    public Sprite OffRecordSprite;
    public Sprite OnRecordSprite;

    private Boolean isBlinking;
    public Text textRecording;
    
//    [DllImport("__Internal")]
//    private static extern void Hello();
// s
//
//    [DllImport("__Internal")]
//    private static extern void getPermission();
    
    private void Start()
    {
         textRecording.gameObject.SetActive(false);
    }

    public void scanLeftGlove()
    {
        if (StretchSenseApi == null)
        {
            StretchSenseApi = StretchSenseController.GetComponent<StretchSenseAPI>();
        }

        if (ChangeImage(buttonLeftGlove))
        {
            StretchSenseApi.setIsLeftGLove(true);
            StretchSenseApi.StartProcess();
        }
        else
        {
            disconnectLeft();
        }
    }

    public void scanRightGlove()
    {
        if (StretchSenseApi == null)
        {
            StretchSenseApi = StretchSenseController.GetComponent<StretchSenseAPI>();
        }

        if (ChangeImage(buttonRightGlove))
        {
            StretchSenseApi.setIsLeftGLove(false);
            StretchSenseApi.StartProcess();
        }
        else
        {
            disconnectRight();
        }
    }

    public void disconnectLeft()
    {
        if (StretchSenseApi != null)
        {
            StretchSenseApi.disconnectLeft();
        }
    }

    public void disconnectRight()
    {
        if (StretchSenseApi != null)
        {
            StretchSenseApi.disconnectRight();
        }
    }

    public void startCalibration()
    {
        controller = StretchSenseController.GetComponent<SSLBleAPI>().getCircuit();
        if (controller != null)
        {
        }
    }

    public void setMin()
    {
        controller = StretchSenseController.GetComponent<SSLBleAPI>().getCircuit();
        if (controller != null)
        {
        }
    }

    public void setMax()
    {
        controller = StretchSenseController.GetComponent<SSLBleAPI>().getCircuit();
        if (controller != null)
        {
        }
    }

    public void OnValueChanged(float newValue)
    {
        int filterValue = (int) newValue;
        controller = StretchSenseController.GetComponent<SSLBleAPI>().getCircuit();
        if (controller != null)
        {
            SSLBleAPI stretchSenseApi = StretchSenseController.GetComponent<SSLBleAPI>();
            byte filter = (byte) filterValue;
            stretchSenseApi.setFilterCharacteristic(filter);
        }
    }

    public void record()
    {
        if (StretchSenseApi != null)
        {
            isRecord = !isRecord;

            if (isRecord)
            {
                StopAllCoroutines();
                isBlinking = true;
                StartCoroutine(StartBlinking());
                StretchSenseApi.recordData();
                BluetoothLEHardwareInterface.Log("Started Recording");
            }
            else
            {
                isBlinking = false;
                StopBlinking();
                StretchSenseApi.stopRecordData();
                BluetoothLEHardwareInterface.Log("Stop Recording");
            }
        }
    }

    public void shareLogs()
    {
        if (StretchSenseApi != null && StretchSenseApi.getSensorFilePath() != null)
        {
            new NativeShare().AddFile(StretchSenseApi.getSensorFilePath())
                .SetSubject(" Sensor Logs")
                .SetText("Please find attached sensor logs. ")
                .Share();
        }
    }

    public Boolean ChangeImage(Button button)
    {
        Boolean isConnected = false;

        if (button.image.sprite == OnSprite)
        {
            button.image.sprite = OffSprite;
        }
        else
        {
            button.image.sprite = OnSprite;
            isConnected = true;
        }

        return isConnected;
    }

    public void setTurnOnImage(Button button)
    {
        button.image.sprite = OnSprite;
 
    }

    public void setTurnOffImage(Button button)
    {
        button.image.sprite = OffSprite;

    }
    
    public void ChangeRecordImage(Button button)
    {

        if (button.image.sprite == OnRecordSprite)
        {
            button.image.sprite = OffRecordSprite;
        }
        else
        {
            button.image.sprite = OnRecordSprite;
        }

    }
    
    public IEnumerator StartBlinking(){
        
        //blink it forever. You can set a terminating condition depending upon your requirement. Here you can just set the isBlinking flag to false whenever you want the blinking to be stopped.
        while(isBlinking){
            buttonRecord.image.sprite = OnRecordSprite;

            //set the Text's text to blank
            buttonRecord.gameObject.SetActive(false);
            textRecording.gameObject.SetActive(false);


            //display blank text for 0.5 seconds
            yield return new WaitForSeconds(.5f);
            buttonRecord.gameObject.SetActive(true);
            textRecording.gameObject.SetActive(true);

            //display “I AM FLASHING TEXT” for the next 0.5 seconds
            yield return new WaitForSeconds(.5f); 
        }
    }
        
    //your logic here. I have set the isBlinking flag to false after 5 seconds
    void StopBlinking(){

        StopAllCoroutines();
        
        buttonRecord.image.sprite = OffRecordSprite;
        textRecording.gameObject.SetActive(false);


    }

    public void swapGlove()
    {
        if (StretchSenseApi != null)
        {
            StretchSenseApi.swapGLove();
        }
        
    }

    public void startLeftCalibration()
    {
        if (StretchSenseApi != null)
        {
            StretchSenseApi.getLeftGloveCircuit().startCalibrating();
        }

    }

    public void startRightCalibration()
    {
        if (StretchSenseApi != null)
        {
            StretchSenseApi.getRightGloveCircuit().startCalibrating();
        }

    }


    public void restoreLeftGloveCalibration()
    {
        if (StretchSenseApi != null)
        {
            StretchSenseApi.getLeftGloveCircuit().restoreCalibration();
        }
    }

    public void restoreRightGloveCalibration()
    {
        if (StretchSenseApi != null)
        {
            StretchSenseApi.getRightGloveCircuit().restoreCalibration();
        } 
    }
    
    public void ChangeScene(String sceneName) { SceneManager.LoadScene(sceneName); } 

    
    
}