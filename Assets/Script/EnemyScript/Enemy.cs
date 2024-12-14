using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    protected Rigidbody2D rigid;
    protected SpriteRenderer spriteRenderer;
    protected Transform player;
    protected Animator anim;

    private RectTransform hpBar;
    private Image nowHpbar;
    public GameObject prfHpBar;
    public GameObject canvas;

    public GameObject markPrefab;
    public float markYOffset = 1f;
    public float height = 1.7f;

    [Header("Enemy Stats")]
    public string enemyName = "Enemy";
    public float maxHp = 100f;
    public float nowHp = 100f;
    public float atkDmg = 10;
    public float moveSpeed = 3f;
    public float detectionRange = 5f;

    protected Vector3 patrolTarget;  // 배회
    public float patrolRange = 2f;
    protected float patrolTimer = 0f;  // 목표 지점에 도달한 후 시간 측정
    public float maxPatrolTime = 3f;  // 목표 지점에 도달하지 못한 상태에서 시간을 얼마나 기다릴지 설정 (초 단위)



    private bool isInDamageState = false;
    protected bool isChasing = false;
    protected bool isEnemyDead = false;
    protected bool isTakingDamage = false;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void SetEnemyStatus
        (string _enemyName, float _maxHp, float _atkDmg, float _moveSpeed )
    {
        enemyName = _enemyName;
        maxHp = _maxHp;
        nowHp = _maxHp;
        atkDmg = _atkDmg;
        moveSpeed = _moveSpeed;
    }

    protected virtual void Start()
    {
        // 체력 바 초기화
        hpBar = Instantiate(prfHpBar, canvas.transform).GetComponent<RectTransform>();
        nowHpbar = hpBar.transform.GetChild(0).GetComponent<Image>();

        SetEnemyStatus("enemyName", maxHp, atkDmg, moveSpeed) ; // 적 초기화

        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) Debug.LogError("Player object not found!");
    }

    protected virtual void Update()
    {
        // 체력 바 위치 갱신
        // 피격 중이라면 추격을 하지 않도록
        if (!isInDamageState && nowHp > 0)  // 피격 상태가 아니고 살아있다면
        {
            if (player != null) DetectAndChasePlayer();
        }

        // 체력 바 위치 갱신
        Vector3 _hpBarPos = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + height, 0));
        hpBar.position = _hpBarPos;

        // 체력 바 상태 갱신
        if (nowHpbar != null) nowHpbar.fillAmount = nowHp / maxHp;
    }

    public virtual void TakeDamage(ParameterPlayerAttack argument)
    {
        if (isTakingDamage || anim.GetBool("isDead")) return;

        isTakingDamage = true;
        nowHp -= argument.damage;

        if (nowHp <= 0)
        {
            HandleWhenDead();
            return;
        }

        // 피격 시 추격 중지
        isInDamageState = true;  // 피격 상태로 전환
        anim.SetBool("isHunt", true);
        Vector2 knockbackDirection = (transform.position - player.position).normalized;
        rigid.velocity = Vector2.zero;  // 넉백 효과가 제대로 적용되도록 초기화
        rigid.AddForce(knockbackDirection * argument.knockback, ForceMode2D.Impulse);  // 넉백

        // 0.5초 후 추격을 재개
        Invoke("ResumeChase", 0.5f);

        StartCoroutine(EndDamage());
    }

    private void ResumeChase()
    {
        isInDamageState = false;  // 피격 상태 해제
    }

    protected virtual IEnumerator EndDamage()
    {
        yield return new WaitForSeconds(0.5f);
        isTakingDamage = false;
        anim.SetBool("isHunt", false);
    }

    protected virtual void HandleWhenDead()
    {
        isEnemyDead = true;
        nowHp = 0;
        anim.SetBool("isDead", true);

        if (hpBar != null) Destroy(hpBar.gameObject); //체력 바 UI 삭제

        StartCoroutine(HandleDeath());

        Debug.Log($"[{GetType().Name}] {enemyName} is dead."); 
    }

    protected virtual IEnumerator HandleDeath()
    {
        // 적이 죽었을 때 처리할 로직
        Debug.Log($"[{GetType().Name}] {enemyName} has died.");

        // 적 오브젝트 비활성화
        gameObject.SetActive(false);

        yield return null; // 필요시 대기
    }

    protected bool IsOnPlatform()
    {
        int platformLayer = LayerMask.GetMask("Platform"); 
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position + new Vector3(0.5f, -1f, 0), Vector2.down, 10f, platformLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position + new Vector3(-0.5f, -1f, 0), Vector2.down, 10f, platformLayer);
        Debug.DrawRay(transform.position + new Vector3(0.5f, -1f, 0), Vector2.down * 2f, Color.red);
        Debug.DrawRay(transform.position + new Vector3(-0.5f, -1f, 0), Vector2.down * 2f, Color.red);

        if (hitRight.collider == null || !hitRight.collider.CompareTag("Platform") ||
            hitLeft.collider == null || !hitLeft.collider.CompareTag("Platform"))
        {
            return false;  // 오른쪽 또는 왼쪽에 플랫폼이 없으면 false
        }
        // 두 곳 모두 플랫폼이 있을 경우 true 반환
        return true;
    }

    protected virtual void Patrol()
    {
        // 목표 지점에 도달했는지 확인하고, 도달하지 못하면 타이머 시작
        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
        {
            patrolTimer = 0f;  // 목표 지점에 도달하면 타이머를 초기화
            SetPatrolTarget();  // 새로운 목표 설정
        }
        else
        {
            patrolTimer += Time.deltaTime;  // 목표 지점에 도달하지 못하면 타이머 증가
        }

        // 목표 지점에 일정 시간 동안 도달하지 못하면 새로운 목표 지점 설정
        if (patrolTimer >= maxPatrolTime)
        {
            patrolTimer = 0f;  // 타이머 초기화
            SetPatrolTarget();  // 새로운 목표 설정
        }

        if (!IsOnPlatform())  // 플랫폼이 없으면 반대 방향으로 이동{
        {
            Vector3 reverseDirection = (transform.position - patrolTarget).normalized;
            patrolTarget = transform.position + reverseDirection * patrolRange;
            rigid.velocity = Vector2.zero;  // 이동 멈춤
        }
        // 목표 지점으로 이동 (배회 시 이동 속도를 0.7배로 설정)
        float adjustedMoveSpeed = moveSpeed * 0.7f;
        transform.position = Vector2.MoveTowards(transform.position, patrolTarget, adjustedMoveSpeed * Time.deltaTime);
        anim.SetBool("isWalk", true);
        LookAtPatrolTarget();  // 이동 방향에 따라 회전
    }

    // 배회할 목표 위치를 랜덤하게 설정
    protected virtual void SetPatrolTarget()
    {
        float randomX = Random.Range(-patrolRange, patrolRange); //x축으로만 움직임
        patrolTarget = new Vector2(transform.position.x + randomX, transform.position.y);
    }

    // 이동 방향을 기준으로 애너미 회전 설정
    protected virtual void LookAtPatrolTarget()
    {
        Vector3 direction = patrolTarget - transform.position;

        if (direction.x > 0)
            transform.rotation = Quaternion.Euler(0, 0, 0); // 오른쪽 방향
        else if (direction.x < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0); // 왼쪽 방향
    }

    protected virtual void DetectAndChasePlayer()
    {
        if (player == null || isInDamageState) return;  // 피격 상태일 경우 추격하지 않음

        HeroKnightUsing playerScript = player.GetComponent<HeroKnightUsing>();
        if (playerScript != null && playerScript.isDead) // 플레이어 사망시 추격해제
        {
            isChasing = false;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            if (!isChasing) SpawnMark();
            isChasing = true;

            if (!IsOnPlatform())  // 절벽을 만나면 더 이상 추격하지 않음
            {
                rigid.velocity = Vector2.zero;  // 이동 멈춤
                Debug.Log("절벽 우회");
            }
            else  // 절벽이 없으면 추격 계속
            {
                anim.SetBool("isWalk", true);
                Vector3 direction = (player.position - transform.position).normalized;  // 플레이어를 추격
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                
                Debug.Log("플레이어 추적시작");
            }

            LookAtPlayer();
        }
        else
        {
            Patrol();  // 플레이어가 없을 때 배회
            isChasing = false;
            anim.SetBool("isWalk", false);
        }
    }

    protected virtual void SpawnMark()
    {
        if (markPrefab != null)
        {
            // 마커를 생성할 위치를 적의 위치에서 markYOffset만큼 Y축으로 올림
            Vector3 spawnPosition = transform.position + new Vector3(0, markYOffset, 0); 
            GameObject markInstance = Instantiate(markPrefab, spawnPosition, Quaternion.identity);

            // 마커의 Mark 스크립트에서 적의 정보를 등록
            Mark markScript = markInstance.GetComponent<Mark>();
            if (markScript != null)
            {
                markScript.enemy = transform;  // 현재 적의 Transform을 마커에 할당
            }
        }
    }

    protected virtual void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;

        if (direction.x > 0) transform.rotation = Quaternion.Euler(0, 0, 0); // 오른쪽 방향
        else transform.rotation = Quaternion.Euler(0, 180, 0); // 왼쪽 방향 (Y축 회전)
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
