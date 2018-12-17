using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Monitor the Left and Right foot and adjust body to keep one foot on the ground. (Allow squatting down)

public class KeepGrounded : MonoBehaviour {

    public GameObject FootRight;
    public GameObject FootLeft;
    public GameObject WholeBody;

    public Vector3 LeftFootPosition;
    public Vector3 RightFootPosition;
    public Vector3 BodyPosition;

	// Use this for initialization
	void Start () {

        float Ymin = 0;

        LeftFootPosition    = FootLeft.transform.position;
        RightFootPosition   = FootRight.transform.position;
        BodyPosition        = WholeBody.transform.position;

        //Get current (global) foot position
        LeftFootPosition = FootLeft.transform.position;
        RightFootPosition = FootRight.transform.position;
        //Determine lowest foot position
        if (LeftFootPosition[1] < RightFootPosition[1])
        {
            Ymin = LeftFootPosition[1];
        }
        else
        {
            Ymin = RightFootPosition[1];
        }

        //Update body position
        BodyPosition = WholeBody.transform.position;
        BodyPosition[1] = Ymin+0.05778f;
        WholeBody.transform.position = BodyPosition;

    }

    // Update is called once per frame
    void Update () {

        float Ymin = 0;
        //Get current (global) foot position
        LeftFootPosition    = FootLeft.transform.position;
        RightFootPosition   = FootRight.transform.position;
        //Determine lowest foot position
        if (LeftFootPosition[1] < RightFootPosition[1])
        {
            Ymin = LeftFootPosition[1];
        }
        else
        {
            Ymin = RightFootPosition[1];
        }

        //Update body position
        BodyPosition        = WholeBody.transform.position;
        BodyPosition[1]     = BodyPosition[1]-Ymin + 0.05778f;
        WholeBody.transform.position = BodyPosition;
        
    }
}
