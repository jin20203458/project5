using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class HeroKnightUsing : MonoBehaviour
{
    [Header("속성")]
    [SerializeField] float m_speed = 4.0f;           // 이동 속도
    [SerializeField] float m_jumpForce = 7.5f;       // 점프 힘
    [SerializeField] float m_rollForce = 6.0f;       // 구르기 힘
    //[SerializeField] bool m_noBlood = false;         // 피 여부
    [SerializeField] GameObject m_slideDust;         // 슬라이딩 먼지 이펙트

    private Animator m_animator;                      // 애니메이터
    private Rigidbody2D m_body2d;                     // 물리 엔진
    private Sensor_HeroKnight m_groundSensor;         // 바닥 센서
    private Sensor_HeroKnight[] m_wallSensors = new Sensor_HeroKnight[4]; // 벽 센서 배열

    private bool m_isWallSliding = false;             // 벽에 붙어 있는지 여부
    private bool m_grounded = false;                  // 바닥에 닿아 있는지 여부
    private bool m_rolling = false;                   // 구르고 있는지 여부
    private int m_facingDirection = 1;                // 캐릭터가 보는 방향
    private int m_currentAttack = 1;                  // 현재 공격 상태 (콤보)
    private float m_timeSinceAttack = 0.0f;           // 공격 후 시간
    private float m_delayToIdle = 0.0f;               // 아이들로 돌아가는 딜레이
    private float m_rollDuration = 8.0f / 14.0f;      // 구르기 지속 시간
    private float m_rollCurrentTime = 0.0f;           // 구르기 현재 시간

    public Transform pos;                             // 공격 위치
    public Vector2 boxSize;                           // 공격 범위 크기

    private bool m_isAttacking = false;               // 공격 중인지 여부
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>(); // 타격된 적 추적

    void Start()
    {
        // 필요한 컴포넌트 초기화
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();

        // 벽 센서 초기화
        for (int i = 0; i < 4; i++)
        {
            m_wallSensors[i] = transform.Find("WallSensor_" + (i < 2 ? "R" : "L") + (i % 2 + 1)).GetComponent<Sensor_HeroKnight>();
        }
    }

    void Update()
    {
        // 타이머 업데이트
        m_timeSinceAttack += Time.deltaTime;
        if (m_rolling)
        {
            m_rollCurrentTime += Time.deltaTime;
            if (m_rollCurrentTime > m_rollDuration)
                m_rolling = false;
        }

        // 바닥에 닿았는지 체크
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        HandleAnimations(); // 애니메이션 처리
    }

    void FixedUpdate()
    {
        HandleMovement(); // 이동 처리
    }

    private void HandleMovement()
    {
        float inputX = Input.GetAxis("Horizontal");

        // 방향 전환 처리
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // 구르기 중이 아닐 때만 이동
        if (!m_rolling)
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        // 공중에 있을 때의 애니메이션 처리
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);
    }

    // 애니메이션 처리 (방향 고정)
    private void HandleAnimations()
    {
        // 벽에 붙어 있는지 체크
        m_isWallSliding = (m_wallSensors[0].State() && m_wallSensors[1].State()) || (m_wallSensors[2].State() && m_wallSensors[3].State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        // 공격, 구르기, 점프 등 애니메이션 트리거 처리
        if (Input.GetKeyDown("e") && !m_rolling) m_animator.SetTrigger("Death");
        else if (Input.GetKeyDown("q") && !m_rolling) m_animator.SetTrigger("Hurt");
        else if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling && !m_isAttacking)
        {
            StartCoroutine(AttackCoroutine()); // 공격 코루틴 호출
        }
        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (Input.GetMouseButtonUp(1)) m_animator.SetBool("IdleBlock", false);
        // 구르기 입력 시
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding)
        {
            // 구르기 시작
            m_rolling = true;
            m_animator.SetTrigger("Roll");

            // 구르기 이동 속도 및 거리 조정 (기존 m_rollForce 값을 크게 설정)
            float enhancedRollSpeed = m_facingDirection * m_rollForce * 3;  // 기존보다 1.5배 더 빨리 이동
            m_body2d.velocity = new Vector2(enhancedRollSpeed, m_body2d.velocity.y);

            // 구르기 지속 시간 늘리기 (기존 시간을 조금 늘려서 구를 때 더 멀리 이동)
            m_rollDuration = 0.25f; // 기존보다 약간 더 길게 (예시로 0.25초로 설정)
            m_rollCurrentTime = 0.0f;
        }
        else if (Input.GetKeyDown("space") && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0) m_animator.SetInteger("AnimState", 0);
        }

        // 공격 중에는 방향을 고정
        if (m_isAttacking)
        {
            // 공격 중에 방향을 고정하고, 반대 방향으로 전환되지 않도록 함
            if (m_facingDirection == 1)
            {
                GetComponent<SpriteRenderer>().flipX = false; // 오른쪽 방향 유지
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = true; // 왼쪽 방향 유지
            }
        }
        else
        {
            // 공격이 끝나면 방향 전환을 허용
            float inputX = Input.GetAxis("Horizontal");

            // 방향 전환 처리
            if (inputX > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
                m_facingDirection = 1;
            }
            else if (inputX < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
                m_facingDirection = -1;
            }

            // 구르기 중이 아닐 때만 이동
            if (!m_rolling)
                m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
        }
    }

    private IEnumerator AttackCoroutine()
    {
        m_isAttacking = true;
        hitEnemies.Clear(); // 타격된 적 리스트 초기화
        m_animator.SetTrigger("Attack" + m_currentAttack); // 공격 애니메이션 시작

        // 공격 지속 시간 동안 공격 판정 처리
        float attackDuration = 0.2f; // 공격 지속 시간
        float timeElapsed = 0f;

        // 공격 중에 타격 체크
        while (timeElapsed < attackDuration)
        {
            Attack(); // 타겟을 때리기
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        m_isAttacking = false;
        m_currentAttack = (m_currentAttack % 3) + 1; // 공격 콤보 순서 변경
    }
    private void Attack()
    {
        // 공격 박스 위치 조정
        Vector3 attackBoxPosition = pos.position;
        if (m_facingDirection == -1)
            attackBoxPosition = new Vector3(pos.position.x - boxSize.x, pos.position.y, pos.position.z);

        // 적과 충돌 체크
        Collider2D[] colliders = Physics2D.OverlapBoxAll(attackBoxPosition, boxSize, 0);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy") && !hitEnemies.Contains(collider))
            {
                collider.GetComponent<Enemy>().TakeDamage(20); // 피해 적용
                hitEnemies.Add(collider); // 타격된 적으로 등록
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (pos != null)
        {
            Gizmos.color = Color.blue;
            Vector3 attackBoxPosition = m_facingDirection == -1 ? new Vector3(pos.position.x - boxSize.x, pos.position.y, pos.position.z) : pos.position;
            Gizmos.DrawWireCube(attackBoxPosition, boxSize); // 공격 범위 시각화
        }
    }
}




