using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class TorchController : MonoBehaviour {
    public Light2D LightSource;
    public Vector2 intensityLimits;
    public Vector2 radiusLimits;
    public Vector2 changeRateLimits;

    private float timer = 0;
    private float changeValue;

    void Start(){
        SetChangeValue();
    }

    void Update(){
        timer += Time.deltaTime;

        if (timer > changeValue){
            LightSource.intensity = Random.Range(intensityLimits.x, intensityLimits.y);
            LightSource.pointLightOuterRadius = Random.Range(radiusLimits.x, radiusLimits.y);

            timer = 0;
            SetChangeValue();
        }
    }

    private void SetChangeValue(){
        changeValue = Random.Range(changeRateLimits.x, changeRateLimits.y);
    }
}