using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodResult : MonoBehaviour
{
    private const string IS_SHOW = "IsShow";
    [SerializeField]private Animator right_Animator;//动画效果绑定
    [SerializeField]private Animator wrong_Animator;
    [SerializeField] private Item success;//展示用
    [SerializeField] private Item failure;
    [SerializeField] private GameObject successParent; 
    [SerializeField] private GameObject failureParent;
    [SerializeField]private GameObject backgroundParent;
    [SerializeField] private float animationDuration = 1.5f;
    private void Start()
    {
        successParent?.SetActive(false);
        failureParent?.SetActive(false);
        backgroundParent?.SetActive(false);
        CraftingMananger.Instance.OnCraftSuccess += Instance_OnCraftSuccess;
        CraftingMananger.Instance.OnCraftFailure += Instance_OnCraftFailure;
    }

    private void Instance_OnCraftFailure(object sender, System.EventArgs e)
    {
        //wrong_Animator.gameObject.SetActive(true);
        //wrong_Animator.SetTrigger(IS_SHOW);
        //failure.gameObject.SetActive(true);//展示用，后删即可
        failureParent.SetActive(true);
        backgroundParent?.SetActive(true);
        StartCoroutine(HideParentAfterAnimation(failureParent));
    }

    private void Instance_OnCraftSuccess(object sender, System.EventArgs e)
    {
        //right_Animator.gameObject.SetActive(true);
        //right_Animator.SetTrigger(IS_SHOW);
        //success.gameObject.SetActive(true);
        successParent.SetActive(true);
        backgroundParent?.SetActive(true);
        StartCoroutine(HideParentAfterAnimation(successParent));
    }
    private IEnumerator HideParentAfterAnimation(GameObject parent)
    {
        
        yield return new WaitForSeconds(animationDuration);
        parent.SetActive(false);
        backgroundParent?.SetActive(false);
    }
    private void Update()
    {
        
    }
}
