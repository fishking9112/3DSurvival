using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    Wandering,
    Attacking
}

public class NPC : MonoBehaviour , IDamagalbe
{
    [Header("Stats")]
    public int health;
    public float walkSpeed;
    public float runSpeed;
    public ItemData[] dropOnDeath;

    [Header("AI")]
    private NavMeshAgent agent;
    public float detectDistance;
    [SerializeField]
    private AIState aiState;
    
    [Header("Wandering")]
    public float minWanderDistance; // 따라서 이동한 최소값
    public float maxWanderDistance; // 따라서 이동할 최대값
    public float minWanderWaitTime; // 얼마나 기다렸다가 이동할지 ?
    public float maxWanderWaitTime;

    [Header("Combat")]
    public int damage;
    public float attackRate;
    private float lastAttackTime;
    public float attackDistance;

    private float playerDistance;
    public float fieldOfView = 120f;    // 공격시 시야 각도

    private Animator animator;
    private SkinnedMeshRenderer[] meshRenderers;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        SetState(AIState.Wandering);
    }

    // Update is called once per frame
    void Update()
    {
        playerDistance = Vector3.Distance(transform.position, CharacterManager.Instance.Player.transform.position);

        animator.SetBool("Moving", aiState != AIState.Idle);

        switch (aiState) 
        {
            case AIState.Idle:
            case AIState.Wandering:
                PassiveUpdate();
                break;
            case AIState.Attacking:
                AttackingUpdate();
                break;

        }
    }
    public void SetState(AIState state)
    {
        aiState = state;

        switch(aiState)
        {
            case AIState.Idle:
                agent.speed = walkSpeed;
                agent.isStopped = true;
                break;
            case AIState.Wandering:
                agent.speed = walkSpeed;
                agent.isStopped = false;
                break;
            case AIState.Attacking:
                agent.speed = runSpeed;
                agent.isStopped = false;
                break;
        }

        animator.speed = agent.speed / walkSpeed;
    }

    void PassiveUpdate()
    {
        if(aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation" , Random.Range(minWanderWaitTime , maxWanderWaitTime)); 
            //Q. 매 프레임마다 Invoke 해줄텐데, 파라미터로 들어오는 시간 동안엔 Invoke가 일어나지 않는가 ?
        }

        if(playerDistance < detectDistance)
        {
            SetState(AIState.Attacking);
        }
    }
    
    void WanderToNewLocation()
    {
        if (aiState != AIState.Idle) return;    // 강의에선 이렇게 나왔는데
        //if (aiState == AIState.Idle) return;    // Idle 일때 안들어오게 하려고 한거 아닐까 ?

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation());
    }

    //목표지점을 정해주는 함수
    Vector3 GetWanderLocation()
    {
        NavMeshHit hit;

        // 일정 영역을 지정해 주면 , 이동할수 있는 경로에 한해 최단 경로를 반환해준다
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance , maxWanderDistance)) , out hit , maxWanderDistance , NavMesh.AllAreas );
        //onUnitSphere == 반지름이 1인 구 형태의 가상의 구



        //do - while 문으로 바꿔보기
        int i = 0;

        while(Vector3.Distance(transform.position , hit.position) < detectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);
            i++;

            if (i == 30) break;
        }

        return hit.position;
    }

    void AttackingUpdate()
    {
        if(playerDistance < attackDistance && IsPlayerInFOV())  // 거리가 가깝고 공격각도 안에 있는 경우
        {
            agent.isStopped = true; // 멈춰서 때린다
            if(Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;

                CharacterManager.Instance.Player.controller.GetComponent<IDamagalbe>().TakePhysicalDamage(damage);
                // 플레이어 컨트롤러 안에 있는 TakePhysicalDamage 함수에 데미지를 넣어 호출만 해준다.
                // 피격에 관한 로직은 저쪽에서 처리해주고,
                // 여기서는 공격을 했다 < 라는것만 알려준다.

                animator.speed = 1;
                animator.SetTrigger("Attack");
            }
        }
        else
        {
            if(playerDistance < detectDistance) // 탐지범위 안에는 있지만 , 공격 범위 밖에 있는 경우
            {
                agent.isStopped = false;
                NavMeshPath path = new NavMeshPath();

                //목표지점을 찾으면
                if(agent.CalculatePath(CharacterManager.Instance.Player.transform.position , path))
                {
                    agent.SetDestination(CharacterManager.Instance.Player.transform.position);

                    //path 를 이용해 이동패턴을 만들 수 있으니 , 활용해 보세요
                }
                //못가요
                else
                {
                    agent.SetDestination(transform.position);
                    agent.isStopped = true;
                    SetState(AIState.Wandering);
                }
            }
            else // 멀어졌을 경우
            {
                agent.SetDestination(transform.position);
                agent.isStopped = true;
                SetState(AIState.Wandering);
            }
        }
    }

    //시야(공격) 각도 안에 player가 있는지 검사하는 함수
    bool IsPlayerInFOV()
    {
        Vector3 directionToPlayer = CharacterManager.Instance.Player.transform.position - transform.position;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        return angle < fieldOfView * 0.5f;
    }

    public void TakePhysicalDamage(int damage)
    {
        health -= damage;

        if(health <= 0)
        {
            Die();
        }

        //피격 효과
        StartCoroutine(DamageFlash());
    }

    void Die()
    {
        for(int i = 0; i < dropOnDeath.Length; i++)
        {
            Instantiate(dropOnDeath[i].dropPrefab , transform.position + Vector3.up * 2.0f , Quaternion.identity);
        }

        Destroy(gameObject);
    }


    IEnumerator DamageFlash()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.color = new Color(1.0f, 0.6f, 0.6f);
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.color = Color.white;
        }
    }
}
