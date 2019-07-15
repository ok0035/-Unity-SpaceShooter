using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour {

    public Transform target;
    public float moveDamping    = 15.0f;
    public float rotateDamping  = 10.0f;
    public float distance       = 5.0f;
    public float height         = 4.0f;
    public float targetOffset   = 2.0f;

    [Header("Wall Obstacle Setting")]
    public float heightAboveWall = 10.0f;    // 카메라가 올라갈 높이
    public float colliderRadius = 1.8f;     // 충돌체의 반지름
    public float overDamping = 5.0f;        // 이동속도 계수
    private float originHeight;             // 최초 높이를 보관할 변수

    [Header("Etc Obstacle Setting")]
    //카메라가 올라갈 높이
    public float heightAboveObstacle = 12.0f;
    //주인공에 투사할 레이캐스트의 높이 오프셋
    public float castOffset = 1.0f;

    private Transform tr;

    // Use this for initialization
    void Start () {
        tr = GetComponent<Transform>();
        originHeight = height;
	}

    // Update is called once per frame
    void Update()
    {

        //구체 형태의 충돌체로 충돌 여부를 검사
        if (Physics.CheckSphere(tr.position, colliderRadius))
        {
            //보간함수를 사용하여 카메라의 높이를 부드럽게 상승시킴
            height = Mathf.Lerp(height
                                , heightAboveWall
                                , Time.deltaTime * overDamping);
        }
        else
        {
            //보간함수를 이용하여 카메라의 높이를 부드럽게 하강시킨다.
            height = Mathf.Lerp(height
                                , originHeight
                                , Time.deltaTime * overDamping);
        }

        //주인공이 장애물에 가려졌는지를 판단할 레이캐스트의 높낮이를 설정
        Vector3 castTarget = target.position + (target.up * castOffset);
        //castTarget 좌표로의 방향벡터를 계산
        Vector3 castDir = (castTarget - tr.position).normalized;
        //충돌 정보를 반활받을 변수
        RaycastHit hit;

        //레이캐스트를 투사해 장애물 여부를 검사
        if(Physics.Raycast(tr.position, castDir, out hit, Mathf.Infinity))
        {
            //주인공이 레이캐스트에 맞지 않았을 경우
            if(!hit.collider.CompareTag("PLAYER"))
            {
                //보간함수를 사용해 카메라의 높이를 부드럽게 상승시킴
                height = Mathf.Lerp(height
                    , heightAboveObstacle
                    , Time.deltaTime * overDamping);
            } else
            {
                //보간함수를 이용하여 카메라의 높이를 부드럽게 하강시킨다.
                height = Mathf.Lerp(height
                                    , originHeight
                                    , Time.deltaTime * overDamping);
            }
        }

    }

    private void LateUpdate()
    {
        var camPos = target.position - (target.forward * distance) + (target.up * height);
        tr.position = Vector3.Slerp(tr.position, camPos, Time.deltaTime * rotateDamping);
        tr.rotation = Quaternion.Slerp(tr.rotation, target.rotation, Time.deltaTime * rotateDamping);
        tr.LookAt(target.position + (target.up * targetOffset));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position + (target.up * targetOffset), 0.1f);
        Gizmos.DrawLine(target.position + (target.up * targetOffset), transform.position);

        //카메라의 충돌체를 표현하기 위한 구체를 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, colliderRadius);

        //주인공 캐릭터가 장애물에 가려졌는지를 판단할 레이를 표시
        Gizmos.color = Color.red;
        Gizmos.DrawLine(target.position + (target.up * castOffset), transform.position);
    }

}
