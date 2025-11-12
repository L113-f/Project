using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotVisual : MonoBehaviour
{
    private const string Make_Food = "make food";
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void PlayFoodMaking() 
    {
        animator.SetTrigger(Make_Food);
    }
}
