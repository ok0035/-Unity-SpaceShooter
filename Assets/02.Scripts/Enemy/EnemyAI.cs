using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    public enum State
    {
        PATROL,
        TRACE,
        ATTACK,
        DIE
    }

    public State state = State.PATROL;

    private Transform playerTr;
    private Transform enemyTr;
    //Animator 컴포넌트를 저장할 변수
    private Animator animator;

    public float attackDist = 5.0f;
    public float traceDist = 15.0f;

    public bool isDie = false;

    private WaitForSeconds ws;

    //이동을 제어하는 MoveAgent클래스를 저장할 변수
    private MoveAgent moveAgent;

    private EnemyFire enemyFire;

    //시야각 및 추적 반경을 제어하는 EnemyFOV 클래스를 저장할 변수
    private EnemyFOV enemyFOV;

    //애니메이터 컨트롤러에 정의한 파라미터의 해시값을 미리 추출
    private readonly int hashMove = Animator.StringToHash("IsMove");
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly int hashDieIdx = Animator.StringToHash("DieIdx");
    private readonly int hashOffset = Animator.StringToHash("Offset");
    private readonly int hashWalkSpeed = Animator.StringToHash("WalkSpeed");
    private readonly int hashPlayerDie = Animator.StringToHash("PlayerDie");


    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("PLAYER");

        if (player != null) playerTr = player.GetComponent<Transform>();

        enemyTr = GetComponent<Transform>();
        moveAgent = GetComponent<MoveAgent>();
        animator = GetComponent<Animator>();
        enemyFire = GetComponent<EnemyFire>();
        //시야각 및 처적 반경을 제어하는 EnemyFOV 클래스를 추출
        enemyFOV = GetComponent<EnemyFOV>();


        ws = new WaitForSeconds(0.3f);

        animator.SetFloat(hashOffset, Random.Range(0.0f, 1.0f));
        animator.SetFloat(hashWalkSpeed, Random.Range(1.0f, 1.2f));
    }

    private void OnEnable()
    {
        StartCoroutine(CheckState());
        StartCoroutine(Action());

        Damage.OnPlayerDie += this.OnPlayerDie;

    }

    private void OnDisable()
    {
        Damage.OnPlayerDie -= this.OnPlayerDie;
    }

    public void OnPlayerDie()
    {

        moveAgent.Stop();
        enemyFire.isFire = false;

        StopAllCoroutines();

        animator.SetTrigger(hashPlayerDie);

    }

    IEnumerator Action()
    {

        //적 캐릭터가 사망할 때까지 무한루프
        while (!isDie)
        {

            yield return ws;
            //상태에 따라 분기처리
            switch (state)
            {

                case State.PATROL:
                    //총알발사정지
                    enemyFire.isFire = false;
                    moveAgent.patrolling = true;
                    animator.SetBool(hashMove, true);
                    break;

                case State.TRACE:
                    enemyFire.isFire = false;
                    moveAgent.traceTarget = playerTr.position;
                    animator.SetBool(hashMove, true);
                    break;

                case State.ATTACK:
                    moveAgent.Stop();
                    animator.SetBool(hashMove, false);

                    if (enemyFire.isFire == false) enemyFire.isFire = true;

                    break;

                case State.DIE:

                    this.gameObject.tag = "Untagged";

                    isDie = true;
                    enemyFire.isFire = false;
                    moveAgent.Stop();

                    animator.SetInteger(hashDieIdx, Random.Range(0, 3));
                    animator.SetTrigger(hashDie);
                    GetComponent<CapsuleCollider>().enabled = false;
                    break;


            }
        }
    }

    IEnumerator CheckState()
    {
        //오브젝트 풀에 생성 시 다른 스크립트의 초기화를 위해 대기
        yield return new WaitForSeconds(1.0f);

        while (!isDie)
        {
            if (state == State.DIE)
            {
                Debug.Log("죽은 상태");
                yield break;
            }

            float dist = Vector3.Distance(playerTr.position, enemyTr.position);

            if (dist <= attackDist)
            {
                //state = State.ATTACK;

                //주인공과의 거리에 장애물 여부를 판단
                if (enemyFOV.isViewPlayer())
                {
                    Debug.Log("여기는 if");
                    state = State.ATTACK; //장애물이 없으면 공격모드 있으면 추적모드
                }
                else
                {
                    state = State.TRACE;
                    Debug.Log("여기는 else");
                }


            }//추적 반경 및 시야각에 드러왔는지 판단
            else if (enemyFOV.isTracePlayer())
            {
                Debug.Log("여기는 else if");
                state = State.TRACE;
            }
            else
            {
                state = State.PATROL;
            }

            yield return ws;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        

        //Speed 파라미터에 이동 속도를 전달
        animator.SetFloat(hashSpeed, moveAgent.speed);
    }
}
