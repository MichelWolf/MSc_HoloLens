using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStart : MonoBehaviour
{

    public AnimationClip anim;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animation>().clip = anim;
        GetComponent<Animation>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
