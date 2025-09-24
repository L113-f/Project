using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using Unity.VisualScripting;

public class NpcControll : MonoBehaviour
{

    [Header("npc名字，需与Block名字一致")]
    public string npcName;

    public Flowchart flowchart;
    private bool canSay;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Say();

        }
    }

    void Say()
    {
        if (canSay)
        {
            if (flowchart.HasBlock(npcName))
            {
                flowchart.ExecuteBlock(npcName);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            canSay = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag.Equals("Player"))
        {
            canSay = false;
        }
    }
}
