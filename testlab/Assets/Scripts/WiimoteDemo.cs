
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System;
using WiimoteApi;
using WiimoteApi.Util;


public class WiimoteDemo : MonoBehaviour
{

    public InertialNavigation inertial;

    public GameObject dummyCube;
    public GameObject laser;
    
    public Button optionAButton;
    public ColorBlock optionAColorblock;
    public bool optionAClicked = false;
    private LineRenderer myLineRenderer;
    

    public WiimoteModel model;
    public RectTransform[] ir_dots;
    public RectTransform[] ir_bb;
    public RectTransform ir_pointer;

    private Quaternion initial_rotation;

    private Wiimote wiimote;
    private Vector2 scrollPosition;

    private Vector3 wmpOffset = Vector3.zero;

    float timer = 0.0f;

    public bool inertialActivated = true;

    public float v;
    public float v0 = 0;
    public float a;
    public float t;
    public float s;
    public float s0 = 0;

    
    public float[] mA = new float[3];
    public float[] sVec = new float[3];
    public  float a_x;
    public float a_y;
    public float a_z;
    public float speedFactor = 0.01f; //regular speed
    public float v_x;
    public float v_y;
    public float v_z;

    public float s_x;
    public float s_y;
    public float s_z;
    
    public float[] accs;
    public float testAccelerationValue = 0;

    public float tempTime;
    public float timeInterval = 1;

    public bool firstTime = true;

    private float InertialTest(float acceleration, float velocity0, float time, float distance0)
    {
        if (!inertialActivated) return 0f;
        
        a = acceleration;
        t = time;

        // 1. Ableitung
        v = a * t + v0;
        // 2. Ableitung
        s = (a / 2f) * Mathf.Pow(t, 2f) + v * t + s0;

        // new v0
        v0 = v;
        // new s0
        s0 = s;
        
        return s;
    }

    private float TransformationTest(float alpha, float beta, float gamma)
    {
        float dx1 = Mathf.Cos(gamma) * Mathf.Cos(alpha) - Mathf.Sin(gamma) * Mathf.Sin(beta) * Mathf.Sin(alpha);
        float dx2 = -Mathf.Sin(gamma) * Mathf.Cos(beta);
        float dx3 = -Mathf.Cos(alpha) * Mathf.Sin(beta) * Mathf.Sin(gamma) + Mathf.Sin(alpha) * Mathf.Cos(gamma);

        float dy1 = Mathf.Sin(beta) * Mathf.Sin(alpha) * Mathf.Cos(gamma) - Mathf.Cos(alpha) * Mathf.Sin(gamma);
        float dy2 = Mathf.Cos(gamma) * Mathf.Cos(beta);
        float dy3 = Mathf.Cos(gamma) * Mathf.Sin(beta) * Mathf.Cos(alpha) + Mathf.Sin(gamma) * Mathf.Sin(alpha);

        float dz1 = Mathf.Cos(beta) * Mathf.Sin(alpha);
        float dz2 = -Mathf.Sin(beta);
        float dz3 = -Mathf.Cos(beta) * Mathf.Cos(alpha);

        float[, ] result = new float[3,3]
        {
            {dx1, dx2, dx3},
            {dy1, dy2, dy3},
            {dz1, dz2, dz3}

        };
        
        return 1;
    }

    public float RemoveGravity(float rotX, float rotY, float rotZ, float accelX, float accelY, float accelZ)
    {
        float[] accel = { accelX, accelY, accelZ }; //accelerometer data
        float[] gravity = { 0, 0, 1.0f }; //gravity downwards g = 1.0
        float[] rG = new float[3];
        float[] rA = new float[3];
       
        
        float alpha = rotX; // 90 * Mathf.PI/180; //from gyro converted to rad
        float beta = rotY; //0* Mathf.PI/180; //from gyro converted to rad
        float gamma = rotZ; // 0 * Mathf.PI/180; //from gyro converted to rad
        Debug.Log("Pan: " + alpha + " Tilt: " + beta + " Roll: " + gamma);

        float[,] R = new float[3, 3]
        {
            { Mathf.Cos(alpha)*Mathf.Cos(beta) , Mathf.Cos(alpha)*Mathf.Sin(beta)*Mathf.Sin(gamma) - Mathf.Sin(alpha)*Mathf.Cos(gamma) , Mathf.Cos(alpha)*Mathf.Sin(beta)*Mathf.Cos(gamma) + Mathf.Sin(alpha)*Mathf.Sin(gamma)},
            { Mathf.Sin(alpha)*Mathf.Cos(beta) , Mathf.Sin(alpha)*Mathf.Sin(beta)*Mathf.Sin(gamma) + Mathf.Cos(alpha)*Mathf.Cos(gamma) , Mathf.Sin(alpha)*Mathf.Sin(beta)*Mathf.Cos(gamma) - Mathf.Cos(alpha)*Mathf.Sin(gamma)},
            {     -1* Mathf.Sin(beta)    ,                  Mathf.Cos(beta) * Mathf.Sin(gamma)                 ,               Mathf.Cos(beta) * Mathf.Cos(gamma)                   }
        };
        
        rG[0]= gravity[0]*R[0,0] + gravity[0]*R[0,1] + gravity[0]*R[0,2];
        rG[1]= gravity[1]*R[1,0] + gravity[1]*R[1,1] + gravity[1]*R[1,2];
        rG[2]= gravity[2]*R[2,0] + gravity[2]*R[2,1] + gravity[2]*R[2,2];

        rA[0] = accel[0] * R[0, 0] + accel[0] * R[0, 1] + accel[0] * R[0, 2];
        rA[1] = accel[1] * R[1, 0] + accel[1] * R[1, 1] + accel[1] * R[1, 2];
        rA[2] = accel[2] * R[2, 0] + accel[2] * R[2, 1] + accel[2] * R[2, 2];
        
        mA[0]=rA[0]-rG[0];
        mA[1]=rA[1]-rG[1];
        mA[2]=rA[2]-rG[2];
        
        rA[0] = mA[0] * R[0, 0] + mA[0] * R[1, 0] + mA[0] * R[2, 0];
        rA[1] = mA[1] * R[0, 1] + mA[1] * R[1, 1] + mA[1] * R[2, 1];
        rA[2] = mA[2] * R[0, 2] + mA[2] * R[1, 2] + mA[2] * R[2, 2];

        Debug.Log("A: " + accel[0] + " | " + accel[1] + " | " + accel[2]);
        //Debug.Log("G: " + gravity[0] + " | " + gravity[1] + " | " + gravity[2]);
        Debug.Log("rG: " + rG[0] + " | " + rG[1] + " | " + rG[2]);
        Debug.Log("A-G: " + mA[0] + " | " + mA[1] + " | " + mA[2]);
        Debug.Log("A-G earthframe: " + rA[0] + " | " + rA[1] + " | " + rA[2]);

        return 1;
    } 

    private float RemoveGravity2( float rotX, float rotY, float rotZ, float accX, float accY, float accZ)
    {
        float vecXAxis = accX;
        float vecYAxis = accY;
        float vecZAxis = accZ;
            
        float yawRad = rotX* Mathf.PI/180; //from gyro converted to rad
        float pitchRad = rotY* Mathf.PI/180; //from gyro converted to rad
        float rollRad = rotZ* Mathf.PI/180; //from gyro converted to rad
        
        float x = accX, y = accY, z = 0;
        vecXAxis -= x * Mathf.Cos(yawRad) - y * Mathf.Sin(yawRad) ;
        vecYAxis -= x * Mathf.Sin(yawRad) - y * Mathf.Cos(yawRad) ;

        x = vecXAxis; z = vecZAxis;
        vecXAxis -=  x * Mathf.Cos(pitchRad) + z * Mathf.Sin(pitchRad) ;
        vecZAxis -= - x * Mathf.Sin(pitchRad) + z * Mathf.Cos(pitchRad) ;

        y = vecYAxis; z = vecZAxis;
        vecYAxis -=  y * Mathf.Cos(rollRad) - z * Mathf.Sin(rollRad);
        vecZAxis -=  y * Mathf.Sin(rollRad) + z * Mathf.Cos(rollRad);

        Debug.Log("Vec: "+ vecXAxis+ " | " +vecYAxis+ " | " +vecZAxis);
        
        return 1;
    }
	
	/** Hier beginnt der neue Kram **/
	
    private Quaternion hamiltonProduct(Quaternion a, Quaternion b) {
        Quaternion ret = new Quaternion();

        ret.w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z;
        ret.x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y;
        ret.y = a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x;
        ret.z = a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w;

        return ret;
    }

    private ownQuat hamiltonProduct2(ownQuat a, ownQuat b) {
        ownQuat ret = new ownQuat();

        ret.w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z;
        ret.x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y;
        ret.y = a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x;
        ret.z = a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w;

        return ret;
    }

    // Just a simple data structure to represent a Quaternion - does not do any checks, so handle with care
    struct ownQuat {
        public float w, x, y, z;
    };

    private ownQuat RotToQuat(float yaw, float pitch, float roll) {
        ownQuat ret = new ownQuat();

		// siehe WP: https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        float cy = Convert.ToSingle(Math.Cos(yaw * 0.5 / 180.0 * Math.PI));
        float cp = Convert.ToSingle(Math.Cos(pitch * 0.5 / 180.0 * Math.PI));
        float cr = Convert.ToSingle(Math.Cos(roll * 0.5 / 180.0 * Math.PI));
        float sy = Convert.ToSingle(Math.Sin(yaw * 0.5 / 180.0 * Math.PI));
        float sp = Convert.ToSingle(Math.Sin(pitch * 0.5 / 180.0 * Math.PI));
        float sr = Convert.ToSingle(Math.Sin(roll * 0.5 / 180.0 * Math.PI));

        // Calculate ownQuat values
        ret.w = cy * cp * cr + sy * sp * sr;
        ret.x = cy * cp * sr - sy * sp * cr;
        ret.y = sy * cp * sr + cy * sp * cr;
        ret.z = sy * cp * cr - cy * sp * sr;

        return ret;
    }

    // supposed to remove gravity value from our Wiimote Accelerator... works more or less well
	// Ich habe das ganze zunächst mit Unity-Datenstrukturen (insb. Vector3 und Quaternion) probiert, später dann mit eigenen (insb. ownQuat)
	// Der Code mit den Unity-Strukturen ist noch auskommentiert
	// Wichtig dabei ist, dass Unity's Umwandlung von Vector3 in Quaternion eine andere Reihenfolge benutzt als meine Methode - deshalb ist die Reihenfolge der Rotationswerte
	// bei der initialisierung von v_rot anders als für die ownQuat-Srtuktur q_rot
    private Vector3 RemoveGravity3(float rotX, float rotY, float rotZ, float accelX, float accelY, float accelZ) {
        Vector3 ret = new Vector3(0, 0, 0);
        //Debug.Log("Raw/Input rotation data: (" + rotX + ", " + rotY + ", " + rotZ + ")");
        //Debug.Log("Raw/Input acceleration data: (" + accelX + ", " + accelY + ", " + accelZ + ")");

        // Create a vector for our acceleration and rotation values, this is only utilized when using Unity's Quaternions
        //Vector3 v_accel = new Vector3(accelY, accelX, accelZ);
        //Vector3 v_rot = new Vector3(rotZ, rotX, rotY);

        // Create a quaternion for our current rotation - atm it's an ownQuat
        //Quaternion q_rot = Quaternion.Euler(v_rot);
        //Debug.Log("Rotation Quaternion (w,x,y,z): (" + q_rot.w + ", " + q_rot.x + ", " + q_rot.y + ", " + q_rot.z + ")");
        ownQuat q_rot = RotToQuat(rotY, rotX, rotZ);

        // Create a quaternion for our acceleration - this should not be normalized ... as a matter of fact it's not even a real quaternion, but who cares
        //Quaternion q_accel = new Quaternion(v_accel.x, v_accel.y, v_accel.z, 0);
        //Debug.Log("Acceleration Quaternion (w,x,y,z): (" + q_accel.w + ", " + q_accel.x + ", " + q_accel.y + ", " + q_accel.z + ")");
        ownQuat q_accel = new ownQuat();
        q_accel.w = 0;
        q_accel.x = accelY;
        q_accel.y = accelX;
        q_accel.z = accelZ;

        // Now we need to Multiply our Quaternion with our Vector - this should give us a "normalized" Vector (i.e. Gravity should then always point "down" [in z direction])
        // It could be, that there is an easier (built-in) way to do this, but I haven't found it yet
		// DIe hier erzeugten Quaternions sind in Wahrheit keine - das ganze ist nur dafür da, damit man den Kram leichter "multiplizieren" kann...
        //Quaternion q_temp = hamiltonProduct(q_rot, q_accel);
        ownQuat q_temp = hamiltonProduct2(q_rot, q_accel);
        //Quaternion q_res = hamiltonProduct(q_temp, Quaternion.Inverse(q_rot));
        ownQuat q_rinv = new ownQuat();
        q_rinv.w = q_rot.w;
        q_rinv.x = -q_rot.x;
        q_rinv.y = -q_rot.y;
        q_rinv.z = -q_rot.z;
        ownQuat q_res = hamiltonProduct2(q_temp, q_rinv);
        //Debug.Log("Normalized accel vector: (" + q_res.x + ", " + q_res.y + ", " + q_res.z + ")");

        //Debug.Log("Normalized Accel data: (" + v_accel_norm.x + ", " + v_accel_norm.y + ", " + v_accel_norm.z + ")");

        // All that needs to be done now, is to remove the gravity data

        // Sanity check... this has driven me nuts - I used these values to correctly set up the rotation quaternion
        /*float roll = Mathf.Atan2(2 * q_rot.y * q_rot.w + 2 * q_rot.x * q_rot.z, 1 - 2 * q_rot.y * q_rot.y - 2 * q_rot.z * q_rot.z);
        float pitch = Mathf.Atan2(2 * q_rot.x * q_rot.w + 2 * q_rot.y * q_rot.z, 1 - 2 * q_rot.x * q_rot.x - 2 * q_rot.y * q_rot.y);
        float yaw = Mathf.Asin(2 * q_rot.x * q_rot.y + 2 * q_rot.z * q_rot.w);*/
		// siehe ebenfalls WP
        float roll = Mathf.Atan2(2 * (q_rot.w * q_rot.x + q_rot.y * q_rot.z), 1 - 2 * (q_rot.x *q_rot.x + q_rot.y*q_rot.y));
        float pitch = Mathf.Asin(2 * (q_rot.w * q_rot.y - q_rot.x * q_rot.z));
        float yaw = Mathf.Atan2(2 * (q_rot.w * q_rot.z + q_rot.x * q_rot.y), 1 - 2 * (q_rot.y * q_rot.y + q_rot.z * q_rot.z));
        Debug.Log("Roll: " + roll * 180 / Math.PI + " Pitch: " + pitch * 180 / Math.PI + " Yaw: " + yaw * 180 / Math.PI);

        // Remove about 0.95 from absolute of z value (i.e. shift it towards 0)
        q_res.z += (q_res.z < 0) ? 0.95f : -0.95f;

        //Quaternion q_temp2 = hamiltonProduct(Quaternion.Inverse(q_rot), q_res);
        ownQuat q_temp2 = hamiltonProduct2(q_rinv, q_res);
        //Quaternion q_orig = hamiltonProduct(q_temp2, q_rot);
        ownQuat q_cleared = hamiltonProduct2(q_temp2, q_rot);
        Debug.Log("cleansed vector: (" + q_cleared.x + ", " + q_cleared.y + ", " + q_cleared.z + ")");

        ret.x = q_cleared.x;
        ret.y = q_cleared.y;
        ret.z = q_cleared.z;

        return ret;
    }
	
	/** Hier endet der neue Kram **/

        void Start() { 
            initial_rotation = model.rot.localRotation; 
            optionAColorblock = optionAButton.colors; 
            optionAColorblock.highlightedColor = new Color32(255,100,100,255); 
            myLineRenderer = laser.GetComponent<LineRenderer>();
    }

    void Update ()
    {
        tempTime += Time.deltaTime;
        
        if (!WiimoteManager.HasWiimote()) { return; }

       // initialisierung

        wiimote = WiimoteManager.Wiimotes[0];
        if (firstTime)
        {
            wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
            int[,] caldata = { { 464, 500, 600 }, { 499, 600, 500 }, { 596, 497, 500 } };
            wiimote.Accel.accel_calib = caldata;
            wiimote.RequestIdentifyWiiMotionPlus();
            wiimote.ActivateWiiMotionPlus();
        }
        firstTime = false;

        int ret;
        do
        {
            ret = wiimote.ReadWiimoteData();

            if (ret > 0 && wiimote.current_ext == ExtensionController.MOTIONPLUS) {


                Vector3 offset = new Vector3(  -wiimote.MotionPlus.PitchSpeed,
                                                wiimote.MotionPlus.YawSpeed,
                                                wiimote.MotionPlus.RollSpeed) / 95f; // Divide by 95Hz (average updates per second from wiimote)
                wmpOffset += offset;

                model.rot.Rotate(offset, Space.Self);
            }
        } while (ret > 0);


        // speichere die Accelerometerwerte der wiimote in ein Array

        accs = wiimote.Accel.GetCalibratedAccelData();
        // Debug.Log("Calib Accel " +accs[0] + " | "+ accs[1] + " | "+accs[2]);

        RemoveGravity(
             model.rot.transform.localRotation.eulerAngles.x,
             model.rot.transform.localRotation.eulerAngles.y,
             model.rot.transform.localRotation.eulerAngles.z,
             accs[0],
             accs[1],
             accs[2]
             );
        
        a_x = mA[0];
        a_y = mA[1];
        a_z = mA[2];

        sVec[0] = s_x;
        sVec[1] = s_y;
        sVec[2] = s_z;
        
        v_x += a_x * Time.deltaTime;
        v_y += a_y * Time.deltaTime;
        v_z += a_z * Time.deltaTime;

        s_x += v_x * Time.deltaTime * speedFactor;
        s_y += v_y * Time.deltaTime * speedFactor;
        s_z += v_z * Time.deltaTime * speedFactor;
        
        model.rot.transform.Translate(s_x, s_y, s_z);
       
        /* //Hab mal versucht, die Achsen zu ändern, hat leider nichts gebracht...
		RemoveGravity1(
           model.rot.transform.localRotation.eulerAngles.z,
           model.rot.transform.localRotation.eulerAngles.x,
           model.rot.transform.localRotation.eulerAngles.y,
           accs[1],
           accs[0],
           accs[2]
           );
		   */
        /* 
RemoveGravity2(
    model.rot.transform.localRotation.eulerAngles.x,
    model.rot.transform.localRotation.eulerAngles.y,
    model.rot.transform.localRotation.eulerAngles.z,
    accs[0],
    accs[1],
    accs[2]
    );
    */
/*
        Vector3 v_accel = RemoveGravity3(
            model.rot.transform.localRotation.eulerAngles.x,
            model.rot.transform.localRotation.eulerAngles.y,
            model.rot.transform.localRotation.eulerAngles.z,
            accs[0],
            accs[1],
            accs[2]);
        
*/
        /*
       // schicke die Accelerometerwerte durch die Formel und gib mir die Positionsänderung zurück

       float xA = InertialTest(accs[0], v0, tempTime, s0);
       float yA = InertialTest(accs[1], v0, tempTime, s0);
       float zA = InertialTest(accs[2], v0, tempTime, s0);

       Debug.Log("ACC_CALC: X: "+xA+" Y: "+yA+ " Z: "+zA);

       // addiere Positionsänderung auf den orangen dummyCube

       dummyCube.transform.position += new Vector3(xA,yA,zA);


     // führe functions alle timeInterval Sekunden aus
     if (tempTime >timeInterval)
     {
         tempTime = 0;
         Debug.Log("Zeitintervall: "+ timeInterval);
         // functions...
     }
     */
        
       
        // button handler
        model.a.enabled = wiimote.Button.a;

        if (model.a.enabled)
        {
            Debug.Log("A pressed");
            ZeroOut();
            ResetOffset();
        };
        model.b.enabled = wiimote.Button.b;
        if (!model.b.enabled)
        {
            myLineRenderer.enabled = false;
        }
        if (model.b.enabled)
        {
            Debug.Log("B pressed");
            myLineRenderer.enabled = true;
            RaycastHit hit;
            if (Physics.Raycast(laser.transform.position, laser.transform.forward, out hit))
            {
                if (hit.collider.gameObject.name == "OptionA")
                {
                    print("Hit: "+ hit.collider.gameObject.name);
                    OptionAClick();
                }
            }
        };
        model.one.enabled = wiimote.Button.one;
        model.two.enabled = wiimote.Button.two;
        model.d_up.enabled = wiimote.Button.d_up;
        model.d_down.enabled = wiimote.Button.d_down;
        model.d_left.enabled = wiimote.Button.d_left;
        model.d_right.enabled = wiimote.Button.d_right;
        model.plus.enabled = wiimote.Button.plus;
        model.minus.enabled = wiimote.Button.minus;
        model.home.enabled = wiimote.Button.home;

        if (wiimote.current_ext != ExtensionController.MOTIONPLUS)
            model.rot.localRotation = initial_rotation;

        if (ir_dots.Length < 4) return;

        float[,] ir = wiimote.Ir.GetProbableSensorBarIR();
        for (int i = 0; i < 2; i++)
        {
            float x = (float)ir[i, 0] / 1023f;
            float y = (float)ir[i, 1] / 767f;
            if (x == -1 || y == -1) {
                ir_dots[i].anchorMin = new Vector2(0, 0);
                ir_dots[i].anchorMax = new Vector2(0, 0);
            }

            ir_dots[i].anchorMin = new Vector2(x, y);
            ir_dots[i].anchorMax = new Vector2(x, y);

            if (ir[i, 2] != -1)
            {
                int index = (int)ir[i,2];
                float xmin = (float)wiimote.Ir.ir[index,3] / 127f;
                float ymin = (float)wiimote.Ir.ir[index,4] / 127f;
                float xmax = (float)wiimote.Ir.ir[index,5] / 127f;
                float ymax = (float)wiimote.Ir.ir[index,6] / 127f;
                ir_bb[i].anchorMin = new Vector2(xmin, ymin);
                ir_bb[i].anchorMax = new Vector2(xmax, ymax);
            }
        }

        float[] pointer = wiimote.Ir.GetPointingPosition();
        ir_pointer.anchorMin = new Vector2(pointer[0], pointer[1]);
        ir_pointer.anchorMax = new Vector2(pointer[0], pointer[1]);
        
    }

    void OnGUI()
    {
        GUI.Box(new Rect(0,0,320,Screen.height), "");

        GUILayout.BeginVertical(GUILayout.Width(300));
        GUILayout.Label("Wiimote Found: " + WiimoteManager.HasWiimote());
        if (GUILayout.Button("Find Wiimote"))
            WiimoteManager.FindWiimotes();

        if (GUILayout.Button("Cleanup"))
        {
            WiimoteManager.Cleanup(wiimote);
            wiimote = null;
        }

        if (wiimote == null)
            return;

        GUILayout.Label("Extension: " + wiimote.current_ext.ToString());

        GUILayout.Label("LED Test:");
        GUILayout.BeginHorizontal();
        for (int x = 0; x < 4;x++ )
            if (GUILayout.Button(""+x, GUILayout.Width(300/4)))
                wiimote.SendPlayerLED(x == 0, x == 1, x == 2, x == 3);
        GUILayout.EndHorizontal();

        GUILayout.Label("Set Report:");
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("But/Acc", GUILayout.Width(300/4)))
            wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL);
        if(GUILayout.Button("But/Ext8", GUILayout.Width(300/4)))
            wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_EXT8);
        if (GUILayout.Button("B/A/Ext16", GUILayout.Width(300 / 4)))
        {
            wiimote.SendDataReportMode(InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
            Debug.Log("Test "+InputDataType.REPORT_BUTTONS_ACCEL_EXT16);

        }

        if(GUILayout.Button("Ext21", GUILayout.Width(300/4)))
            wiimote.SendDataReportMode(InputDataType.REPORT_EXT21);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Request Status Report"))
            wiimote.SendStatusInfoRequest();

        GUILayout.Label("IR Setup Sequence:");
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Basic", GUILayout.Width(100)))
            wiimote.SetupIRCamera(IRDataType.BASIC);
        if(GUILayout.Button("Extended", GUILayout.Width(100)))
            wiimote.SetupIRCamera(IRDataType.EXTENDED);
        if(GUILayout.Button("Full", GUILayout.Width(100)))
            wiimote.SetupIRCamera(IRDataType.FULL);
        GUILayout.EndHorizontal();

        GUILayout.Label("WMP Attached: " + wiimote.wmp_attached);
        if (GUILayout.Button("Request Identify WMP"))
            wiimote.RequestIdentifyWiiMotionPlus();
        if ((wiimote.wmp_attached || wiimote.Type == WiimoteType.PROCONTROLLER) && GUILayout.Button("Activate WMP"))
            wiimote.ActivateWiiMotionPlus();
        if ((wiimote.current_ext == ExtensionController.MOTIONPLUS ||
            wiimote.current_ext == ExtensionController.MOTIONPLUS_CLASSIC ||
            wiimote.current_ext == ExtensionController.MOTIONPLUS_NUNCHUCK) && GUILayout.Button("Deactivate WMP"))
            wiimote.DeactivateWiiMotionPlus();

        GUILayout.Label("Calibrate Accelerometer");
        GUILayout.BeginHorizontal();
        for (int x = 0; x < 3; x++)
        {
            AccelCalibrationStep step = (AccelCalibrationStep)x;
            if (GUILayout.Button(step.ToString(), GUILayout.Width(100)))
            {
                wiimote.Accel.CalibrateAccel(step);
            }

        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Print Calibration Data"))
        {
            StringBuilder str = new StringBuilder();
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    str.Append(wiimote.Accel.accel_calib[y, x]).Append(" ");
                }
                str.Append("\n");
            }
            Debug.Log(str.ToString());
        }

        if (wiimote != null && wiimote.current_ext != ExtensionController.NONE)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUIStyle bold = new GUIStyle(GUI.skin.button);
            bold.fontStyle = FontStyle.Bold;
            if (wiimote.current_ext == ExtensionController.CLASSIC) {
                GUILayout.Label("Classic Controller:", bold);
                ClassicControllerData data = wiimote.ClassicController;
                GUILayout.Label("Stick Left: " + data.lstick[0] + ", " + data.lstick[1]);
                GUILayout.Label("Stick Right: " + data.rstick[0] + ", " + data.rstick[1]);
                GUILayout.Label("Trigger Left: " + data.ltrigger_range);
                GUILayout.Label("Trigger Right: " + data.rtrigger_range);
                GUILayout.Label("Trigger Left Button: " + data.ltrigger_switch);
                GUILayout.Label("Trigger Right Button: " + data.rtrigger_switch);
                GUILayout.Label("A: " + data.a);
                GUILayout.Label("B: " + data.b);
                GUILayout.Label("X: " + data.x);
                GUILayout.Label("Y: " + data.y);
                GUILayout.Label("Plus: " + data.plus);
                GUILayout.Label("Minus: " + data.minus);
                GUILayout.Label("Home: " + data.home);
                GUILayout.Label("ZL: " + data.zl);
                GUILayout.Label("ZR: " + data.zr);
                GUILayout.Label("D-Up: " + data.dpad_up);
                GUILayout.Label("D-Down: " + data.dpad_down);
                GUILayout.Label("D-Left: " + data.dpad_left);
                GUILayout.Label("D-Right: " + data.dpad_right);
            }
            else if (wiimote.current_ext == ExtensionController.MOTIONPLUS)
            {
                GUILayout.Label("Wii Motion Plus:", bold);
                MotionPlusData data = wiimote.MotionPlus;
                GUILayout.Label("Pitch Speed: " + data.PitchSpeed);
                GUILayout.Label("Yaw Speed: " + data.YawSpeed);
                GUILayout.Label("Roll Speed: " + data.RollSpeed);
                GUILayout.Label("Pitch Slow: " + data.PitchSlow);
                GUILayout.Label("Yaw Slow: " + data.YawSlow);
                GUILayout.Label("Roll Slow: " + data.RollSlow);
                if (GUILayout.Button("Zero Out WMP"))
                {
                    data.SetZeroValues();
                    model.rot.rotation = Quaternion.FromToRotation(model.rot.rotation*GetAccelVector(), Vector3.up) * model.rot.rotation;
                    model.rot.rotation = Quaternion.FromToRotation(model.rot.forward, Vector3.forward) * model.rot.rotation;
                }
                if(GUILayout.Button("Reset Offset"))
                    wmpOffset = Vector3.zero;
                GUILayout.Label("Offset: " + wmpOffset.ToString());
            }

            GUILayout.EndScrollView();
        } else {
            scrollPosition = Vector2.zero;
        }
        GUILayout.EndVertical();
    }
    
    void ZeroOut()
    {
        MotionPlusData data = wiimote.MotionPlus;
        data.SetZeroValues();
        model.rot.rotation = Quaternion.FromToRotation(model.rot.rotation * GetAccelVector(), Vector3.up) * model.rot.rotation;
        model.rot.rotation = Quaternion.FromToRotation(model.rot.forward, transform.forward) * model.rot.rotation;
        model.rot.transform.position = new Vector3(0,1.6f,1.4f);
    }
    
    void ResetOffset()
    {
        wmpOffset = Vector3.zero;
    }
    void OnDrawGizmos()
    {
        if (wiimote == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(model.rot.position, model.rot.position + model.rot.rotation*GetAccelVector()*2);
    }

    private Vector3 GetAccelVector()
    {
        float accel_x;
        float accel_y;
        float accel_z;

        float[] accel = wiimote.Accel.GetCalibratedAccelData();
        accel_x = accel[0];
        accel_y = -accel[2];
        accel_z = -accel[1];
        
        return new Vector3(accel_x, accel_y, accel_z).normalized;
    }

    [System.Serializable]
    public class WiimoteModel
    {
        public Transform rot;
        public Renderer a;
        public Renderer b;
        public Renderer one;
        public Renderer two;
        public Renderer d_up;
        public Renderer d_down;
        public Renderer d_left;
        public Renderer d_right;
        public Renderer plus;
        public Renderer minus;
        public Renderer home;
    }

    void OnApplicationQuit() {
        if (wiimote != null) {
            WiimoteManager.Cleanup(wiimote);
            wiimote = null;
        }
    }
    public void OptionAClick()
    {
        
        Debug.Log("Option A clicked");

        if (!optionAClicked)
        {
            optionAColorblock.highlightedColor = new Color32(100,255,100,255);
            optionAColorblock.pressedColor = new Color32(100,255,100,255);
            optionAColorblock.normalColor = new Color32(100,255,100,255);
            optionAButton.colors = optionAColorblock;
            
            optionAClicked = true;
        }
        else
        {
            optionAColorblock.pressedColor = new Color32(255,255,255,255);
            optionAColorblock.normalColor = new Color32(255,255,255,255);
            optionAButton.colors = optionAColorblock;
            optionAClicked = false;
        }
    }
}
