using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : GravityBody
{
    [SerializeField]
    float playerViewDistance = 10f, mouseSensitivity = 5f, rotationSmoothTime = 0.05f, smoothMoveTime = .1f, hangTime = 1f;
    public Camera camera;
    public GameObject holdPoint;
    float lerpDuration = 1f;

    [SerializeField]
    GameObject dotWhite, dotRed, pauseUI;
    [SerializeField]
    LayerMask layerMask;
    Animator animator;
    StateMachine stateMachine;
    [SerializeField]
    LayerMask ground;

    float mouseX, mouseY, rotationX, smoothPitch, smoothYaw, yawSmoothV, pitchSmoothV, origSpeed, origRunSpeed, origGravity, origJumpheigth;
    
    Vector3 targetVelocity, smoothVelocity;

    [HideInInspector]
    public Vector3 direction;
    [HideInInspector]
    public IdleState idleState;
    [HideInInspector]
    public RunState runState;
    [HideInInspector]
    public WalkState walkState;

    bool locked = false, paused = false;
    Quaternion initialRot;

    protected override void Start()
    {
        base.Start();
        initialRot = transform.rotation;
        pauseUI.SetActive(false);
        animator = GetComponent<Animator>();
        stateMachine = new StateMachine();
        idleState = new IdleState(this, stateMachine, "Idle", animator);
        runState = new RunState(this, stateMachine, "Run", animator);
        walkState = new WalkState(this, stateMachine, "Walk", animator);

        stateMachine.Initialize(idleState);
        rotationX = 0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        origGravity = gravity;
        origJumpheigth = jumpHeigth;
        origRunSpeed = runSpeed;
        origSpeed = speed;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) HandlePauseGame();
        if (locked) return;
        HandlePlayerMovement();
        HandleRayCast();
        HandleJump();
        stateMachine.CurrentState.LogicUpdate();
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }

    void FixedUpdate() 
    {
        if (locked) return;
        ApplyGravity();
        HandleLookMovement();
        rb.MovePosition(transform.position + velocity * Time.deltaTime);
        camera.transform.localRotation = Quaternion.Euler(smoothPitch, 0, 0);
        transform.rotation *= Quaternion.Euler(0, smoothYaw, 0);
    }

    protected override void LateUpdate() 
    {
        base.LateUpdate();
    }

    void HandleLookMovement()
    {
        mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity/2;
        mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity/2;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -75f, 75f);

        smoothPitch = Mathf.SmoothDampAngle (smoothPitch, rotationX, ref pitchSmoothV, rotationSmoothTime);
        smoothYaw = Mathf.SmoothDampAngle (smoothYaw, mouseX, ref yawSmoothV, rotationSmoothTime);
    }

    void HandlePlayerMovement()
    { 
        direction = (transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical")).normalized;
        targetVelocity = direction * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : speed);
        velocity = Vector3.SmoothDamp (velocity, targetVelocity, ref smoothVelocity, smoothMoveTime);
    }

    void HandleJump()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, distanceToTheGround + 0.01f, ground);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) 
            rb.AddForce(new Vector3(0, jumpHeigth, 0));
    }

    void HandleRayCast()
    {
        RaycastHit hit;
        if(Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, playerViewDistance, layerMask))
        {
            Interactable interactable = hit.collider.gameObject.GetComponent<Interactable>();
            FlyingObject flyingInteractable = hit.collider.gameObject.GetComponent<FlyingObject>();

            if (interactable != null || flyingInteractable != null)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0)) 
                {
                    if (interactable != null) interactable.Interact();
                    else flyingInteractable.Interact();
                }

                if (!dotRed.activeSelf)
                {
                    dotRed.SetActive(true);
                    dotWhite.SetActive(false);
                }

                return;
            }
        }

        if (!dotWhite.activeSelf)
        {
            dotRed.SetActive(false);
            dotWhite.SetActive(true);
        }
    }

    public void GoToWithLerp(Vector3 position, Vector3 scale, Vector3 cameraPosition)
    {
        StartCoroutine(Lerp(position, scale, cameraPosition));
    }

    public void HandlePauseGame()
    {
        paused = !paused;
        Cursor.lockState = CursorLockMode.Locked;
        if (paused) Cursor.lockState = CursorLockMode.None;        
        Cursor.visible = paused;
        pauseUI.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void SetMouseSensitivity(float sliderValue)
    {
        mouseSensitivity = sliderValue;
    }

    IEnumerator Lerp(Vector3 position, Vector3 scale, Vector3 cameraPosition)
    {
        locked = true;
        GetComponent<Collider>().enabled = false;
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scale, timeElapsed / lerpDuration);
            transform.position = Vector3.Lerp(transform.position, position, timeElapsed / lerpDuration);
            transform.rotation = Quaternion.Lerp(transform.rotation, initialRot, timeElapsed / lerpDuration);
            camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, cameraPosition, timeElapsed / lerpDuration);
            camera.transform.localRotation = Quaternion.Lerp(camera.transform.localRotation, Quaternion.Euler(0, 0, 0), timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        transform.localScale = scale;
        transform.position = position;
        camera.transform.localPosition = cameraPosition;
        smoothPitch = 0f;
        smoothYaw = 0f;
        camera.transform.localRotation = Quaternion.Euler(0, 0, 0);
        speed = origSpeed;
        runSpeed = origRunSpeed;
        jumpHeigth = origJumpheigth;
        gravity = origGravity;
        GetComponent<Collider>().enabled = true;
        locked = false;
        distanceToTheGround = GetComponent<Collider>().bounds.extents.y;
    }
}
