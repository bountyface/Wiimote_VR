using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    private LineRenderer lr;
    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            Debug.DrawLine(transform.position, hit.point);
            print(hit.transform.name);
            /*
            if (hit.collider)
            {
                lr.SetPosition(1, new Vector3(0,0,hit.distance));
            }
            */
        }
       /* else
        {
            lr.SetPosition(1,new Vector3(0,0,5000));
        }
*/
    }
}
