using Panadoll;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    private string key = "ddd";
    void Start()
    {
        Debug.LogError("121");
        //e1 = Event1;
        e2 = Event2;
    }

    private DelegateCallback e1 = () =>
    {
        //Debug.LogError(key);
    };
    private DelegateCallback e2;

    void Event1()
    {
        //Debug.LogError(112);
    }

    void Event2()
    {
        //Debug.LogError("!!!!!333");
    }

    // Update is called once per frame
    void Update()
    {
       EventManager.Add(GameEventType.EnterGame, e2);
        //EventManager.Add(GameEventType.EnterGame, e1);
        //EventManager.Add(GameEventType.EnterGame, e2);
        EventManager.Send(GameEventType.EnterGame);
    }


}

    
    // Start is called before the first frame update
    
    
// Update is called once per frame



