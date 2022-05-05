using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    public string sceneName;
    private Animator anim;

    void Start(){
        DontDestroyOnLoad(this.gameObject);

        anim = GetComponent<Animator>();
    }

    public void LoadScene(){    //Called by Animation once black bg bars fully down
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void LoadFinished(){     //Other scene lets loader know scene has finished required setup. Some are slower than others due to generation requirements
        anim.SetBool("Loaded", true);
    }

    public void AnimationFinished(){
        Destroy(gameObject);
    }
}
