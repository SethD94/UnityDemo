using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Utility
{
    public class KneeUIManager : MonoBehaviour
    {
        public GameObject StretchSenseController;

        public SSLBleAPI KneeBleApi;
        
        public Button btnScanKnee;
        
        public Sprite OffSprite;
        public Sprite OnSprite;
        public Camera mOrthographicCamera;
        public float perspectiveZoomSpeed = 0.5f; // The rate of change of the field of view in perspective mode.
        public float orthoZoomSpeed = 0.5f;

       public void startCalibration()
        {
            
            
            BluetoothLEHardwareInterface.Log(" Start Knee Calibration");
            if (KneeBleApi != null && KneeBleApi.getCircuit()!=null)
            {
                KneeBleApi.getCircuit().startCalibrating();
            }

        }
        
        public void scanKnee()
        {
            
            BluetoothLEHardwareInterface.Log(" Scan Knee");

            if (KneeBleApi == null)
            {
                KneeBleApi = StretchSenseController.GetComponent<SSLBleAPI>();
            }

            if (ChangeImage(btnScanKnee))
            {
                BluetoothLEHardwareInterface.Log(" Start Process");

                KneeBleApi.StartProcess();
            }
            else
            {
                disconnect();
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
        
        public void disconnect()
        {
            if (KneeBleApi != null)
            {
                KneeBleApi.disconnect();
            }
        }
        
         public void moveLeft()
    {
        mOrthographicCamera.transform.Rotate(Vector3.up, 20.0f * Time.deltaTime);
    }
    
    
    public void moveRight()
    {
        BluetoothLEHardwareInterface.Log(" Move Right");
        mOrthographicCamera.transform.Rotate(Vector3.down, 20.0f * Time.deltaTime);
    }

    public void moveUp()
    {
        mOrthographicCamera.transform.Rotate(Vector3.left, 20.0f * Time.deltaTime);
    }
    
    public void moveDown()
    {
        mOrthographicCamera.transform.Rotate(Vector3.right, 20.0f * Time.deltaTime);
    }
    
    public void zoomIn()
    {
        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = - 6f;

        // If the camera is orthographic...
        if (mOrthographicCamera.orthographic)
        {
            // ... change the orthographic size based on the change in distance between the touches.
            mOrthographicCamera.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

            // Make sure the orthographic size never drops below zero.
            mOrthographicCamera.orthographicSize = Mathf.Max(mOrthographicCamera.orthographicSize, 0.1f);
        } else
        {
            // Otherwise change the field of view based on the change in distance between the touches.
            mOrthographicCamera.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;

            // Clamp the field of view to make sure it's between 0 and 180.
            mOrthographicCamera.fieldOfView = Mathf.Clamp(mOrthographicCamera.fieldOfView, 0.1f, 179.9f);
        }
    }

    public void zoomOut()
    {
        // Find the difference in the distances between each frame.
        float deltaMagnitudeDiff = 6f;

        // If the camera is orthographic...
        if (mOrthographicCamera.orthographic)
        {
            // ... change the orthographic size based on the change in distance between the touches.
            mOrthographicCamera.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;

            // Make sure the orthographic size never drops below zero.
            mOrthographicCamera.orthographicSize = Mathf.Max(mOrthographicCamera.orthographicSize, 0.1f);
        } else
        {
            // Otherwise change the field of view based on the change in distance between the touches.
            mOrthographicCamera.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;

            // Clamp the field of view to make sure it's between 0 and 180.
            mOrthographicCamera.fieldOfView = Mathf.Clamp(mOrthographicCamera.fieldOfView, 0.1f, 179.9f);
        }
    }

        
        public void ChangeScene(String sceneName) { SceneManager.LoadScene(sceneName); } 
        
        public void restoreCalibration()
        {
            if (KneeBleApi != null && KneeBleApi.getCircuit()!=null)
            {
                KneeBleApi.getCircuit().restoreCalibration();
            } 
        }
        
    }
    
}
