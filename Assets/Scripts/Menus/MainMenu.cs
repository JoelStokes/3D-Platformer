using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    public GameObject LoadPrefab;

    public void Confirm(InputAction.CallbackContext context){
        if (context.phase == InputActionPhase.Started){
            GameObject LoadObj = Instantiate(LoadPrefab, new Vector3(0,0,0), Quaternion.identity);
            LoadObj.GetComponent<Loading>().sceneName = "VerticalClassic";
            LoadObj.name = "Load Handler";
            
            //Prevent all future selections when done
            Destroy(gameObject);
        }
    }
}
