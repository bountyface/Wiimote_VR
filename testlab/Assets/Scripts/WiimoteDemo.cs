
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
    public float s0=0;

    public float[] accs;
    public float testAccelerationValue = 0;

    public float tempTime;
    public float timeInterval = 1;

    public bool firstTime = true;

    private float InertialTest(float acceleration, float velocity0, float time, float distance0)
    {
        // Berechnung der Inertial Navigation laut Formel

        if (!inertialActivated) return 0f;

        Debug.Log("InertialTest running...");

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
       // Debug.Log("Rotation: "+alpha+" | "+beta+" | "+gamma+" | ");

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
        //Debug.Log("Matrix: " + result);
        return 1;
    }

    private float RemoveGravity(float rotX, float rotY, float rotZ, float accelX, float accelY, float accelZ)
    {

        float[] accel = { accelX, accelY, accelZ }; //accelerometer data
        float[] gravity = { 0, 0, 1.0f }; //gravity downwards g = 1.0
        float[] rG = new float[3];
        float[] rA = new float[3];
        float[] mA = new float[3];


        Debug.Log("RotX: " + rotX + " RotY: " + rotY + " RotZ: " + rotZ);
        Debug.Log("AccX: " + accelX + " AccY: " + accelY + " AccZ: " + accelZ);

        float alpha = rotX* Mathf.PI/180; //from gyro converted to rad
        float beta = rotY* Mathf.PI/180; //from gyro converted to rad
        float theta = rotZ* Mathf.PI/180; //from gyro converted to rad
        //Debug.Log("Pan: " + alpha + " Tilt: " + beta + " Roll: " + theta);







        float[,] R = new float[3, 3]
        {
            { Mathf.Cos(alpha)*Mathf.Cos(beta) , Mathf.Cos(alpha)*Mathf.Sin(beta)*Mathf.Sin(theta) - Mathf.Sin(alpha)*Mathf.Cos(theta) , Mathf.Cos(alpha)*Mathf.Sin(beta)*Mathf.Cos(theta) + Mathf.Sin(alpha)*Mathf.Sin(theta)},
            { Mathf.Sin(alpha)*Mathf.Cos(beta) , Mathf.Sin(alpha)*Mathf.Sin(beta)*Mathf.Sin(theta) + Mathf.Cos(alpha)*Mathf.Cos(theta) , Mathf.Sin(alpha)*Mathf.Sin(beta)*Mathf.Cos(theta) - Mathf.Cos(alpha)*Mathf.Sin(theta)},
            {     -1* Mathf.Sin(beta)    ,                  Mathf.Cos(beta) * Mathf.Sin(theta)                 ,               Mathf.Cos(beta) * Mathf.Cos(theta)                   }
        };
        //Debug.Log("Rotation Matrix: "+ rotationMatrix[0,0] +" | " + rotationMatrix[0,1] +" | " + rotationMatrix[0,2]);


        float det =  R[0,0]*(R[1,1]*R[2, 2]-R[1, 2]*R[2, 1])/
                    -R[0,1]*(R[1,0]*R[2, 2]-R[1, 2]*R[2, 0])/
                    +R[0,2]*(R[1,0]*R[2, 1]-R[1, 1]*R[2, 0]);


        Debug.Log("Determinante: " +det);



        rG[0]= gravity[0]*R[0,0] + gravity[1]*R[0,1] + gravity[2]*R[0,2] ;
        rG[1]= gravity[0]*R[1,0] + gravity[1]*R[1,1] + gravity[2]*R[1,2] ;
        rG[2]= gravity[0]*R[2,0] + gravity[1]*R[2,1] + gravity[2]*R[2,2] ;


        rA[0] = accel[0] * R[0, 0] + accel[1] * R[0, 1] + accel[2] * R[0, 2];
        rA[1] = accel[0] * R[1, 0] + accel[1] * R[1, 1] + accel[2] * R[1, 2];
        rA[2] = accel[0] * R[2, 0] + accel[1] * R[2, 1] + accel[2] * R[2, 2];


        Debug.Log("rA: " + rA[0] + " | " + rA[1] + " | " + rA[2]);


        //Debug.Log("Rotated Gravity: " +rotatedGravity[0]);

        mA[0]=rA[0]-rG[0];
        mA[1]=rA[1]-rG[1];
        mA[2]=rA[2]-rG[2];


       // Debug.Log("rA: " +rA[0]+ " | " +rA[1]+ " | " +rA[2]);


        rA[0] = mA[0] * R[0, 0] + mA[1] * R[1, 0] + mA[2] * R[2, 0];
        rA[1] = mA[0] * R[0, 1] + mA[1] * R[1, 1] + mA[2] * R[2, 1];
        rA[2] = mA[0] * R[0, 2] + mA[1] * R[1, 2] + mA[2] * R[2, 2];




        Debug.Log("A: " + accel[0] + " | " + accel[1] + " | " + accel[2]);
        Debug.Log("G: " + gravity[0] + " | " + gravity[1] + " | " + gravity[2]);
        Debug.Log("rG: " + rG[0] + " | " + rG[1] + " | " + rG[2]);
        Debug.Log("A-G: " + mA[0] + " | " + mA[1] + " | " + mA[2]);
        Debug.Log("A-G earthframe: " + rA[0] + " | " + rA[1] + " | " + rA[2]);


        return 1;
    }
    void Start() {
        //inertial=new InertialNavigation();
        //InvokeRepeating("InertialTestTest",1.0f, 1.0f);
        initial_rotation = model.rot.localRotation;
    }

    void Update ()
    {
        tempTime += Time.deltaTime;
        //Debug.Log(transform.forward);
        //TransformationTest(model.rot.transform.localRotation.eulerAngles.x, model.rot.transform.localRotation.eulerAngles.y, model.rot.transform.localRotation.eulerAngles.z);



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
                //Debug.Log(-wiimote.MotionPlus.PitchSpeed /95f);

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
        //ReadOnlyArray<int> accs_raw = wiimote.Accel.accel;
        //Debug.Log(accs_raw[0]+ " | " + accs_raw[1] + " | "+ accs_raw[2]);

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
        Debug.Log("Accel: "+wiimote.Accel.GetCalibratedAccelData());
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
    // Micha
    void ZeroOut()
    {
        MotionPlusData data = wiimote.MotionPlus;
        data.SetZeroValues();
        model.rot.rotation = Quaternion.FromToRotation(model.rot.rotation * GetAccelVector(), Vector3.up) * model.rot.rotation;
        model.rot.rotation = Quaternion.FromToRotation(model.rot.forward, transform.forward) * model.rot.rotation;
    }

    // Micha
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

        // could be wrong, normalisation should take min and max values and calculate them into 0...1
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
}
