using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float maxVelocity;
    [SerializeField] private float maxForce;
    [SerializeField] private float flipDelay;

    [SerializeField] private int direction;

    [SerializeField] private GameObject splashPrefab;
    [SerializeField] private GameObject winObject;
    [SerializeField] private GameObject loseObject;
    [SerializeField] private AudioSource walkSource;

    [SerializeField] private bool b_canMove;

    private Rigidbody2D playerRigidbody2D;
    private CircleCollider2D circleCollider2D;
    private GameObject doorLock;

    private RaycastHit2D raycastHit2D;
    private Vector2 playerVelocity;

    private int collectedKeyCount;
    private int totalKeyCount;

    private string keyTagName = "Key";
    private string lockTagName = "Lock";
    private string doorTagName = "Door";
    private string spikeTagName = "Spikes";

    private float startTime;
    private float lastFlip;
    
    private bool useVelocityMethod;

    private void Start()
    {
        playerRigidbody2D = GetComponent<Rigidbody2D>();
        circleCollider2D = GetComponent<CircleCollider2D>();

        totalKeyCount = GameObject.FindGameObjectsWithTag(keyTagName).Length;
        doorLock = GameObject.FindGameObjectWithTag(lockTagName);

        lastFlip = Time.time;
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            b_canMove = true;
            startTime = Time.time;
        }

        raycastHit2D = Physics2D.Raycast(transform.position, Vector2.down + Vector2.right * 0.5f * direction, 1.5f);

        if (raycastHit2D && Vector2.Angle(Vector3.left, Vector2.Perpendicular(raycastHit2D.normal)) < 45.0)
        {
            transform.up = raycastHit2D.normal;
        }

        if ((playerRigidbody2D.velocity.magnitude <= 0.1) && (Time.time - lastFlip > flipDelay) && (Time.time - startTime > 0.5) && b_canMove)
        {
            direction = -direction;
            lastFlip = Time.time;
            GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
        }

        if ((Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(2))) ReloadScene();

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }


    private void FixedUpdate()
    {
        Vector2 vector2 = (Vector2)transform.position + circleCollider2D.offset * (Vector2)transform.localScale;
        float distance1 = (float)(circleCollider2D.radius * (transform.localScale[1] + 0.2));
        Vector2 direction1 = -transform.up;

        RaycastHit2D raycastHit2D = Physics2D.Raycast(vector2, direction1, distance1);

        if (useVelocityMethod && !raycastHit2D)
        {
            float distance2 = distance1 + 0.4f;
            Vector2 direction2 = -Vector3.down;
            raycastHit2D = Physics2D.Raycast(vector2, direction2, distance2);
        }

        if (raycastHit2D && b_canMove)
        {
            if (useVelocityMethod)
            {
                playerRigidbody2D.velocity = transform.right * direction * maxVelocity;
            }
            else
            {
                playerVelocity = playerRigidbody2D.velocity;

                if (playerVelocity.magnitude < maxVelocity)
                {
                    playerRigidbody2D.AddForce(transform.right * maxForce * direction);
                }
            }
        }

        playerVelocity = playerRigidbody2D.velocity;

        if(playerVelocity.magnitude > 0.3 && raycastHit2D && playerRigidbody2D.simulated && b_canMove)
        {
            if(walkSource.isPlaying) return;
            walkSource.Play();
        }
        else
        {
            if(!walkSource.isPlaying) return;
            walkSource.Pause();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == keyTagName)
        {
            collectedKeyCount++;
            SFXHandler.Instance.Play(SFXHandler.Sounds.Chain);
            SpawnSplash(collision.gameObject.transform.position);
            Destroy(collision.gameObject);

            if (collectedKeyCount == totalKeyCount) UnlockDoor();
        }

        else if (collision.tag == doorTagName && doorLock == null)
        {
            SFXHandler.Instance.Play(SFXHandler.Sounds.Ding_02);
            Disappear();
            winObject.SetActive(true);
            PlayerPrefs.SetInt("levelIndex", SceneManager.GetActiveScene().buildIndex);
            Invoke("LoadNextScene", 3f);
        }

        else if (collision.tag == spikeTagName)
        {
            SFXHandler.Instance.Play(SFXHandler.Sounds.Book);
            Disappear();
            //loseObject.SetActive(true);
            Invoke("ReloadScene", 1f);
        }       
    }


    private void ReloadScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    private void LoadNextScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    private void UnlockDoor()
    {
        SpawnSplash(doorLock.gameObject.transform.position);
        Destroy(doorLock);
        SFXHandler.Instance.Play(SFXHandler.Sounds.LockOpened);
    }

    private void SpawnSplash(Vector3 pos)
    {
        GameObject gameObject = Instantiate(splashPrefab, pos, Quaternion.identity);
        Destroy(gameObject, gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
    }

    private void Disappear()
    {
        playerRigidbody2D.simulated = false;
        GetComponent<SpriteRenderer>().enabled = false;
        SpawnSplash(transform.position);
        walkSource.Stop();
    }
}
