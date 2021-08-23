using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(-0.05934695302748846);
        Vector3 newPos = new Vector3((float)-0.05934695302748846, (float)-0.05934695302748846, (float)-0.05934695302748846);
        Debug.Log(newPos.x);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
