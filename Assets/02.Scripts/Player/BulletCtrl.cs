using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 * isTrigger 옵션 X
 * void OnCollisionEnter    두 물체 간의 충돌이 일어나기 시작 했을 때
 * void OnCollisionStay     두 물체 간의 충돌이 지속될 때
 * void onCollisionExit     두 물체가 서로 떨어졌을 때
 * 
 * isTrigger 옵션 O
 * void OnTriggerEnter      두 물체 간의 충돌이 일어나기 시작했을 때
 * void OnTriggerStay       두 물체 간의 충돌이 지속 될 때
 * void OnTriggerExit       두 물체가 서로 떨어졌을 때
 * 
 * 
 */

public class BulletCtrl : MonoBehaviour {

    public float damage = 10.0f;
    public float speed = 5000.0f;

    private Transform tr;
    private Rigidbody rb;
    private TrailRenderer trail;

    private void Awake()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
        damage = GameManager.instance.gameData.damage;
    }

    private void OnEnable()
    {
        rb.AddForce(transform.forward * speed);
        GameManager.OnItemChange += UpdateSetup;
    }

    private void UpdateSetup()
    {
        damage = GameManager.instance.gameData.damage;
    }

    private void OnDisable()
    {
        trail.Clear();
        tr.position = Vector3.zero;
        tr.rotation = Quaternion.identity;
        rb.Sleep();
    }

 //   // Use this for initialization
 //   void Start () {
 //       GetComponent<Rigidbody>().AddForce(transform.forward * speed);
	//}
	
	// Update is called once per frame
	void Update () {
		
	}
}
