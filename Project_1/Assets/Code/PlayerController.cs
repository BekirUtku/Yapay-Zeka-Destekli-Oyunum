using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
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
        if (isGrounded == true)
        {
            extraJumps = extraJumpsValue;
        }
        if (Input.GetKeyDown(KeyCode.Space) && extraJumps > 0)
        {
            rb.linearVelocity = Vector2.up * jumpForce;
            extraJumps--;
            playerRealAnimator.SetBool("jump", true);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && extraJumps == 0 && isGrounded)
        {
            rb.linearVelocity = Vector2.up * jumpForce;
            playerRealAnimator.SetBool("jump", true);
        }

        if (Mathf.Approximately(rb.linearVelocity.y, 0) && playerRealAnimator.GetBool("jump"))
        {
            playerRealAnimator.SetBool("jump", false);
        }

        characterAttack();
        characterRunAttack();
        CharachterAttackSpacing();

        if (transform.position.y < -15f)
        {
            Time.timeScale = 0f;
            gameOverPanel.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        horizontal = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector3(horizontal * Time.deltaTime * speed, rb.linearVelocity.y, 0);

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
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);

    }
}
