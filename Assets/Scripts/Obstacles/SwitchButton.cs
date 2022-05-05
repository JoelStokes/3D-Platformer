using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchButton : MonoBehaviour
{
    public Animator[] moveAnims;
    public Animator switchAnim;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateSwitch(){
        switchAnim.SetTrigger("Press");

        foreach (Animator anim in moveAnims){
            anim.SetTrigger("In");
        }
    }

    public void ResetSwitch(){  //Called when Gen re-uses level piece
        switchAnim.SetTrigger("Reset");

        foreach (Animator anim in moveAnims){
            anim.SetTrigger("Reset");
        }
    }
}
