using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateTest : MonoBehaviour {

    delegate void CalNumDelegate(int num);

    event CalNumDelegate calNum;

	// Use this for initialization
	void Start () {

        //calNum = OnPlusNum;
        //calNum(4);

        //calNum = PowerNum;
        //calNum(5);

        calNum += OnPlusNum;
        calNum += PowerNum;

        calNum(5);

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnPlusNum(int num)
    {

        int result = num + 1;
        Debug.Log("One Plus = " + result);

    }

    void PowerNum(int num)
    {

        int result = num * num;
        Debug.Log("Power = " + result);

    }
}
