using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    float horizontal;
    public float speed;
    float moveInput;

    bool sagabak = true;
    Vector3 scale;

    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask whatIsGround;
    public float checkRadius;
    private int extraJumps;
    public int extraJumpsValue;
    public float jumpForce;

    Animator playerRealAnimator;

    PlayerCombat playerCombat;

    public bool characterattack;

    public float charactertimer;

    public GameObject gameOverPanel;

    // Game Over bir kez tetiklensin.
    private bool gameOverTriggered = false;

    void Start()
    {
        Time.timeScale = 1f;
        rb = gameObject.GetComponent<Rigidbody2D>();

        playerRealAnimator = GetComponent<Animator>();

        playerCombat = GetComponent<PlayerCombat>();

        charactertimer = 1f;
    }

    private void Update()
    {
        // Oyun duraklatildiginda (bilgi paneli, Game Over, kazanma ekrani)
        // girdi islenmesin ve fizik sorgulari bosa calismasin.
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

        // Girdi Update'te okunur; FixedUpdate'te okumak tek karelik basislari kacirir.
        horizontal = Input.GetAxis("Horizontal");
        moveInput = Input.GetAxisRaw("Horizontal");
        playerRealAnimator.SetFloat("speed", Mathf.Abs(moveInput));

        if (horizontal > 0 && sagabak == false)
        {
            cevir();
        }
        if (horizontal < 0 && sagabak == true)
        {
            cevir();
        }

        if (isGrounded)
        {
            extraJumps = extraJumpsValue;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (extraJumps > 0)
            {
                extraJumps--;
                Jump();
            }
        }

        // Yere inince zipla animasyonunu kapat.
        if (isGrounded && rb.linearVelocity.y <= 0.01f && playerRealAnimator.GetBool("jump"))
        {
            playerRealAnimator.SetBool("jump", false);
        }

        characterAttack();
        characterRunAttack();
        CharachterAttackSpacing();

        if (transform.position.y < -15f)
        {
            Die();
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // linearVelocity zaten "saniyedeki birim"; deltaTime ile carpilmaz.
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        playerRealAnimator.SetBool("jump", true);

        // Ayni yerdeyken ikinci kez tetiklenmesin; FixedUpdate yeniden hesaplayacak.
        isGrounded = false;
    }

    void characterAttack()
    {
        if (Input.GetKeyDown(KeyCode.E) && horizontal == 0 && characterattack)
        {
            playerRealAnimator.SetTrigger("isattack");
            playerCombat.DamageEnemy();
            characterattack = false;
        }
    }

    void characterRunAttack()
    {
        if (Input.GetKeyDown(KeyCode.E) && (horizontal > 0 || horizontal < 0) && characterattack)
        {
            playerRealAnimator.SetTrigger("isRunAttack");
            playerCombat.DamageEnemy();
            characterattack = false;
        }
    }

    void CharachterAttackSpacing()
    {
        if (characterattack == false)
        {
            charactertimer -= Time.deltaTime;
        }
        if (charactertimer < 0)
        {
            charactertimer = 0f;
        }
        if (charactertimer == 0f)
        {
            characterattack = true;
            charactertimer = 1f;
        }
    }

    void cevir()
    {
        sagabak = !sagabak;
        scale = gameObject.transform.localScale;
        scale.x *= -1;
        gameObject.transform.localScale = scale;
    }

    public void Die()
    {
        // Hem dusme hem can bitisi buraya geliyor; yalnizca bir kez calissin.
        if (gameOverTriggered) return;
        gameOverTriggered = true;

        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"{name}: PlayerController.gameOverPanel atanmamis.", this);
        }
    }
}
