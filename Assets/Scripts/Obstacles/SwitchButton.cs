using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchButton : MonoBehaviour
{
    public Animator[] moveAnims;
    public Animator switchAnim;
    public Animator[] gearAnims;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateSwitch(){
        switchAnim.SetTrigger("Activate");

        foreach (Animator anim in moveAnims){
            anim.SetTrigger("Activate");
        }

        if (gearAnims != null && gearAnims.Length != 0){
            foreach (Animator anim in gearAnims){
                anim.SetTrigger("Activate");
            }
        }
    }

    public void ResetSwitch(){  //Called when Gen re-uses level piece
        switchAnim.SetTrigger("Reset");

        foreach (Animator anim in moveAnims){
            anim.SetTrigger("Reset");
        }

        if (gearAnims != null && gearAnims.Length == 0){
            foreach (Animator anim in gearAnims){
                anim.SetTrigger("Reset");
            }
        }
    }
}
