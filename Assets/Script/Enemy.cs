using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer SpriteRenderer;
    RectTransform hpBar;
    Image nowHpbar;
    Transform player;
    Animator anim;

    public GameObject prfHpBar;
    public GameObject canvas;
    public string enemyName;
    public float maxHp;
    public float nowHp;
    public int atkDmg=10;
    public int atkSpeed;
    public float moveSpeed = 3f; // 기본 이동 속도
    public float height = 1.7f;
    public float detectionRange = 5f;
    public int nextMove;
    public CameraShake cameraShake;
    public float baseKnockbackForce = 6f;
    [SerializeField] Vector3 scale = new Vector3(1, 1, 1);
    [SerializeField] float scalex;
   
    bool isChasing = false;
    private Vector3 initialPosition;
    private int attackCount = 0;  // 공격 횟수를 추적하는 변수
    private float currentKnockbackForce;     // 현재 적용된 넉백 강도 (3타 후 고정값 적용)
    private bool isEnemyDead = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 체력 바 초기화 (각 적에 대해 독립적으로 생성)
        hpBar = Instantiate(prfHpBar, canvas.transform).GetComponent<RectTransform>();
        scalex = scale.x;
        // 적 상태 설정
        if (name.Equals("Enemy1"))
        {
            SetEnemyStatus("Enemy1", 100, 10, 1); // Enemy1의 상태 설정
        }
        else if (name.Equals("Enemy2"))
        {
            SetEnemyStatus("Enemy2", 80, 12, 2); // Enemy2의 상태 설정 (예시)
        }
        else
        {
            // 여기에 다른 적 상태 초기화 코드 추가
            SetEnemyStatus("Enemy3", 150, 8, 1); // 추가적인 적 초기화
        }

        // 체력 바의 이미지 초기화
        nowHpbar = hpBar.transform.GetChild(0).GetComponent<Image>();

        // 초기화된 값 확인 (디버깅용)
        Debug.Log($"Enemy Name: {enemyName}, Max HP: {maxHp}, Current HP: {nowHp}");

        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player object not found!");
        }
        initialPosition = transform.position;
        // 게임 시작 시 기본 넉백 강도로 초기화
        currentKnockbackForce = baseKnockbackForce;
    }

    void Update()
    {
        // 체력 바 위치 갱신
        Vector3 _hpBarPos = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + height, 0));
        hpBar.position = _hpBarPos;

        // 체력 바 상태 갱신
        if (nowHpbar != null) // nowHpbar가 null이 아닌 경우에만 갱신
        {
            nowHpbar.fillAmount = (float)nowHp / (float)maxHp;
        }

        // 플레이어 탐지 및 추적
        if (isChangingDirection == false && nowHp > 0)
            DetectAndChasePlayer();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("충돌 감지: " + collision.gameObject.name);
        
        if (isEnemyDead)
        {
            Debug.Log("OnCollisionEnter2D : 사망함!");
            return;
        }

        if (collision.gameObject.CompareTag("Player") && (isEnemyDead == false))
        {
            HeroKnightUsing player = collision.gameObject.GetComponent<HeroKnightUsing>();
            if (player != null)
            {
                // 플레이어가 살아 있을 때만 데미지를 입힘
                if (!player.isDead)
                {
                    player.TakeDamage(atkDmg);
                    Debug.Log($"{enemyName}가 플레이어에게 {atkDmg}만큼 공격했습니다.");
                }
                else
                {
                    Debug.Log($"{enemyName}가 공격하려 했지만 플레이어는 이미 죽었습니다.");
                }
            }
            else
            {
                Debug.LogWarning("플레이어의 HeroKnightUsing 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }



    public void TakeDamage(ParameterPlayerAttack argument)
    {
        // 이미 죽는 중이면 추가 피해를 무시
        if (anim.GetBool("isDead")) return;

        // 체력 감소
        nowHp -= argument.damage;

        // 체력이 0 이하일 경우 즉시 죽음 처리
        if (nowHp <= 0)
        {
            HandleWhenDead();
            return; // 함수 종료
        }

        // 체력이 남아 있는 경우 피격 처리
        Debug.Log($"Damage taken: {argument.damage}, Remaining HP: {nowHp}");

        // 피격 애니메이션 트리거
        if (!anim.GetBool("isHunt"))
        {
            anim.SetBool("isHunt", true); // 피격 상태 시작
        }

        // 넉백 처리 (피격 시 밀려나는 효과)
        Vector2 knockbackDirection = (transform.position - player.position).normalized;

        // 넉백 강도 설정 (기본 또는 3타 공격에서의 강도)
        rigid.velocity = Vector2.zero; // 현재 속도를 초기화
        rigid.AddForce(knockbackDirection * argument.knockback, ForceMode2D.Impulse);

        StartCoroutine(ResetKnockback());

        //// 공격 횟수 증가
        //attackCount++;

        //// 3번째 공격마다 카메라 흔들림 발생
        //if (attackCount == 3)
        //{
        //    cameraShake.ShakeCamera(); // 카메라 흔들림 실행
        //    currentKnockbackForce = 8f; // 3번째 공격 넉백 강도 고정
        //    attackCount = 0; // 공격 횟수 초기화
        //}
    }

    private void HandleWhenDead()
    {
        isEnemyDead = true;

        nowHp = 0; // 체력을 0으로 고정
        anim.SetBool("isHunt", false); // 피격 애니메이션 해제
        anim.SetBool("isDead", true);  // 죽음 애니메이션 트리거

        Debug.Log($"Enemy {enemyName} is dead."); // 디버그 메시지
        StartCoroutine(HandleDeath()); // 죽음 처리 코루틴 호출
    }

    // 넉백 초기화
    private IEnumerator ResetKnockback()
    {
        yield return new WaitForSeconds(0.1f); // 0.1초 후 넉백 상태 초기화
        anim.SetBool("isHunt", false);
        // 넉백 강도를 기본값으로 되돌림
        currentKnockbackForce = baseKnockbackForce; // 기본 넉백 강도로 초기화
    }

    // 죽을 때 처리 (1초 대기 후 객체 파괴)
    private IEnumerator HandleDeath()
    {
        // 체력 바 숨김 처리
        hpBar.gameObject.SetActive(false);

        // 죽음 애니메이션 재생 (0.6초 대기)
        yield return new WaitForSeconds(2f);

        // 적 객체와 체력 바 완전히 삭제
        Destroy(gameObject);
        Destroy(hpBar.gameObject);
    }

    private void SetEnemyStatus(string _enemyName, int _maxHp, int _atkDmg, int _atkSpeed)
    {
        enemyName = _enemyName;
        maxHp = _maxHp;
        nowHp = _maxHp;
        atkDmg = _atkDmg;
        atkSpeed = _atkSpeed;
    }

    private void DetectAndChasePlayer()
    {
        if (player == null) return;

        // 플레이어가 죽었는지 확인
        HeroKnightUsing playerScript = player.GetComponent<HeroKnightUsing>();
        if (playerScript != null && playerScript.isDead)
        {
            isChasing = false; // 플레이어가 죽었으면 추적을 멈춤
            return; // 추적을 멈추고 더 이상 진행하지 않음
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 플레이어가 탐지 범위 안에 있을 경우에만 추적
        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true; // 플레이어가 범위 안에 있으면 추적 시작
        }
        else
        {
            isChasing = false; // 범위 밖이면 추적 멈춤
        }

        // 플레이어를 추적
        if (isChasing)
        {
            Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.5f, rigid.position.y);
            RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 3, LayerMask.GetMask("Platform"));

            if (rayHit.collider == null && !isChangingDirection) // 벽 끝에 닿은 경우 (반복하지 않도록)
            {
                MoveAwayAndRetry(); // 벽 끝에서 처리하는 로직 호출
                isChangingDirection = true; // 반대 방향으로 이동 중임을 표시
            }
            else
            {
                // 추적 로직
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                LookAtPlayer();
            }
        }
    }


    private bool isChangingDirection = false; // 벽 끝에 닿을 때 이동 상태 관리

    private void MoveAwayAndRetry()
    {
        Debug.Log("Hit wall, stopping movement for a moment.");

        // 이동 속도를 0으로 설정하여 멈춤
        moveSpeed = 0f;

        // 잠시 멈춘 후 추적을 재개
        StartCoroutine(ResumeMovementAfterDelay()); // 일정 시간 후 이동을 재개하는 코루틴 호출
    }

    private IEnumerator ResumeMovementAfterDelay()
    {
        yield return new WaitForSeconds(2f); // 1초 동안 대기

        moveSpeed = 3f; // 기본 이동 속도 (원하는 값으로 설정)
        transform.position = Vector3.MoveTowards(transform.position, initialPosition, Time.deltaTime * 5f);
        isChangingDirection = false; // 다시 추적을 시작할 수 있도록 상태 변경
    }

    private void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
        {
            // 플레이어가 오른쪽에 있으면
            scale.x = scalex;
            transform.localScale = scale * (nowHp / maxHp); // 오른쪽으로 반전
        }
        else
        {
            // 플레이어가 왼쪽에 있으면
            scale.x = -scalex;
            transform.localScale = scale * (nowHp / maxHp); // 왼쪽으로 반전
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}