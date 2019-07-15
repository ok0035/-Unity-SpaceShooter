using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveBullet : MonoBehaviour {

    public GameObject sparkEffect;

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.tag == "BULLET")
        {
            Debug.Log("Bullet Collision Test");
            ShowEffect(coll);
            //Destroy(coll.gameObject);
            coll.gameObject.SetActive(false);
        }

    }

    private void ShowEffect(Collision coll)
    {
        //충돌 지점의 정보를 추출
        ContactPoint contact = coll.contacts[0];

        //법선 벡터가 이루는 회전각도 추출
        Quaternion rot = Quaternion.FromToRotation(Vector3.back, contact.normal);

        //스파크 효과 생성
        GameObject spark = Instantiate(sparkEffect, 
            contact.point + (-contact.normal * 0.03f), rot);

        //스파크 효과의 부모를 드럼통 또는 벽으로 설정
        spark.transform.SetParent(this.transform);
    
    }
}
