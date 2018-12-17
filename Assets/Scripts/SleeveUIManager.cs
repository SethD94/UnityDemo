using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class SleeveUIManager : MonoBehaviour
    {
        
        public GameObject StretchSenseController;
        
        public SSLBleAPI SleeveBleApi;

        public Button btnScanSleeve;
        
        public Sprite OffSprite;
        public Sprite OnSprite;
        public Camera mOrthographicCamera;
        public float perspectiveZoomSpeed = 0.5f; // The rate of change of the field of view in perspective mode.
        public float orthoZoomSpeed = 0.5f;

        public Camera topCamera;
        public Button btnZoomIn;
        public Button btnZoomOut;
        public Button btnMoveUp;
        public Button btnMoveDown;
        public Button btnMoveLeft;
        public Button btnMoveRight;

        private float maxPercentage = 100f;

        private float minPercentage = -4f;

        public Camera mainCamera;

        public Text txtSteps;
        SSL_Circuit sleeveCircuitController;

        // Use this for initialization
        void Start()
        {
            mainCamera.rect = new Rect(0f, 0.0f, 1f, 1.0f);
            topCamera.enabled = false;
            QualitySettings.antiAliasing = 4;

            txtSteps.gameObject.SetActive(false);
        }

        void Update()
        {
            sleeveCircuitController = SleeveBleApi.getCircuit();
            if (sleeveCircuitController.isCalibrating())
            {
                txtSteps.gameObject.SetActive(true);
                txtSteps.text = " Step Completed:  " + "x" + " / ";
            }
            else {
                mainCamera.rect = new Rect(0f, 0.0f, 1f, 1.0f);
                topCamera.enabled = false;
                txtSteps.gameObject.SetActive(false);
            }
        }

        public void startRightCalibration()
        {
            
            topCamera.enabled = true;
            mOrthographicCamera.rect = new Rect(0f, 0.0f, 0.5f, 1.0f);

                
            BluetoothLEHardwareInterface.Log(" Start Sleeve Calibration");
            if (SleeveBleApi != null && SleeveBleApi.getCircuit()!=null)
            {
                SleeveBleApi.getCircuit().startCalibrating();
            }

        }
        
        public void scanSleeve()
        {
            
            Debug.Log("Debug Scan Sleve");

            if (SleeveBleApi == null)
            {
                Debug.Log("Debug Get API");
                SleeveBleApi = StretchSenseController.GetComponent<SSLBleAPI>();
            }

            if (ChangeImage(btnScanSleeve))
            {
                Debug.Log("Debug Scan Sleve");
                SleeveBleApi.StartProcess();
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
            if (SleeveBleApi != null)
            {
                SleeveBleApi.disconnect();
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
            if (SleeveBleApi != null && SleeveBleApi.getCircuit()!=null)
            {
                SleeveBleApi.getCircuit().restoreCalibration();
            } 
        }
        
        
    }
}