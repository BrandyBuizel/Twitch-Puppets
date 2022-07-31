using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouthControl : MonoBehaviour
{
    // Start is called before the first frame update

    public Quaternion rotation1, rotation2;
    public GameObject jaw;
    public float openAm = 0;


    void Update()
    {

        Quaternion newRot = Quaternion.Lerp(rotation1, rotation2, openAm);
        jaw.transform.localRotation = newRot;

    }
}
