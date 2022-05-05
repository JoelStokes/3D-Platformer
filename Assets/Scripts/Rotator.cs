using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationPerSecond = 0;

    void Update(){
        transform.Rotate(0, 0, rotationPerSecond * Time.deltaTime / 1f, Space.Self);
    }
}
