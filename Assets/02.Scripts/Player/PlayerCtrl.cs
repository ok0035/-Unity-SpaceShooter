using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAnim
{
    public AnimationClip idle;
    public AnimationClip runF;
    public AnimationClip runB;
    public AnimationClip runL;
    public AnimationClip runR;
}

public class PlayerCtrl : MonoBehaviour {

    /*
     *
     * Awake() : 스크립트가 실행될 때 한번만 호출
     * Start() : Update 함수가 호출되기 전에 한 번 호출
     *          (다른 스크립트의 Awake 함수가 모두 실행된 후 실행)
     * Update() : 프레임마다 호출
     * LateUpdate() : 모든 Update 함수가 호출되고 나서 한 번씩 호출
     *                  (카메라 이동로직에 주로 사용)
     * FixedUpdate() : 물리엔진의 시뮬레이션 계산주기로 기본값은 0.02초
     *                  (발생하는 주기가 일정)
     * OnEnable : 게임 오브젝트 또는 스크립트가 활성화 됐을 때 호출
     * OnDisable : 게임 오브젝트 또는 스크립트가 비활성화 됐을 때 호출
     * OnGUI : 레거시 GUI 관련 함수를 사용할 때 사용
    */

    private float h = 0.0f;
    private float v = 0.0f;
    private float r = 0.0f;

    //접근해야 하는 컴포넌트는 반드시 변수에 할당한 후 사용
    private Transform tr;

    //이동속도 변수
    public float moveSpeed = 10.0f;

    //회전속도 변수
    public float rotSpeed = 80.0f;

    public PlayerAnim playerAnim;

    [HideInInspector]
    public Animation anim;

    private void OnEnable()
    {
        GameManager.OnItemChange += UpdateSetup;
    }

    private void UpdateSetup()
    {
        moveSpeed = GameManager.instance.gameData.speed;
    }

    // Use this for initialization
    void Start () {

        tr = GetComponent<Transform>();
        anim = GetComponent<Animation>();
        anim.clip = playerAnim.idle;
        anim.Play();

        //불러온 데이터 값을 moveSpeed 에 적용
        moveSpeed = GameManager.instance.gameData.speed;
	}

    // Update is called once per frame
    void Update () {

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        r = Input.GetAxis("Mouse X");

        //Debug.Log("h = " + h.ToString());
        //Debug.Log("v = " + v.ToString());

        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        //float vec = Vector3.Magnitude(moveDir.normalized);
        //Debug.Log("moveDir의 크기 : " + vec);

        tr.Translate(moveDir.normalized * moveSpeed * Time.deltaTime, Space.Self);
        //Debug.Log("델타타임 : " + Time.deltaTime + "");

        //Vector3.up(y축)을 기준으로 rotSpeed 만큼의 속도로 회전
        tr.Rotate(Vector3.up * rotSpeed * Time.deltaTime * r);

        if (v >= 0.1f)
        {
            anim.CrossFade(playerAnim.runF.name, 0.3f); // 전진 애니메이션
        }
        else if (v <= -0.1f)
        {
            anim.CrossFade(playerAnim.runB.name, 0.3f); // 후진
        }
        else if (h >= 0.1f)
        {
            anim.CrossFade(playerAnim.runR.name, 0.3f); //오른쪽
        }
        else if (h <= -0.1f)
        {
            anim.CrossFade(playerAnim.runL.name, 0.3f); // 왼쪽
        }
        else {
            anim.CrossFade(playerAnim.idle.name, 0.3f); // 정지시
        }

        /*
        
        Vector3.forward = Vector3(0,0,1);
        Vector3.back = Vector3(0,0,-1);
        Vector3.left = Vector3(-1,0,0);
        Vector3.right = Vector3(1,0,0);
        Vector3.up = Vector3(0,1,0);
        Vector3.down = Vector3(0,-1,0);
        Vector3.one = Vector3(1,1,1);
        Vector3.zero = Vector3(0,0,0);

        회전

        Rotate(회전할 기준 좌표축 * Time.deltaTime * 회전 속도 * 변위 입력값);

        */
    }
}
