using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int Speed = 2;

    private Animator Animator;
    private Rigidbody Rigidbody;
    private CapsuleCollider CapsuleCollider;

    protected Vector3 Movement;

    //private List<Vector3> PreviousMovementBuffer;
    private int BeforeMovementCounter;
    private int MovementAnalyzerTreshold;

    private float TimeCount;
    private float BodyDistanceToGround;

    private float GRAVITY = 9.87f * 600;

    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody>();
        CapsuleCollider = GetComponent<CapsuleCollider>();

        TimeCount = 0.0f;
        BodyDistanceToGround = CapsuleCollider.bounds.extents.y;
    }

    protected bool IsGrounded() => Physics.Raycast
    (
        transform.position + new Vector3(0, CapsuleCollider.center.y, 0),
        Vector3.down,
        BodyDistanceToGround + 0.1f
    );

    public float[] InputComponents
    {
        get
        {
            float HorizontalComponent = Input.GetAxis("Horizontal");
            float VerticalComponent = Input.GetAxis("Vertical");

            return new float[] { HorizontalComponent, VerticalComponent };
        }
    }

    private Vector3 CalculateMovement(float HorizontalComponent, float VerticalComponent) =>
        new Vector3(HorizontalComponent, 0, VerticalComponent);

    private Vector3 CalculateMovement(float[] Components) => CalculateMovement(Components[0], Components[1]);

    private void Run(Vector3 direction)
    {
        bool MovementStoped = false;

        if (BeforeMovementCounter < MovementAnalyzerTreshold)
        {
            BeforeMovementCounter++;
            return;
        }
        else
        {
            BeforeMovementCounter = 0;
            if (direction == Vector3.zero)
                MovementStoped = true;
        }

        if (MovementStoped)
        {
            Animator.SetBool("IsWalking", false);

            TimeCount = 0.0f;
        }
        else
        {
            Animator.SetBool("IsWalking", true);

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction.normalized), TimeCount * 1.5f);
            Rigidbody.AddForce(direction.normalized * (float)Speed * Time.fixedDeltaTime, ForceMode.Impulse);

            TimeCount = TimeCount + Time.deltaTime;
        }
    }

    void Update()
    {
        Movement = CalculateMovement(InputComponents);
    }

    private void FixedUpdate()
    {
        Run(Movement);

        if(!IsGrounded())
            Rigidbody.AddForce(Vector3.down * GRAVITY * Time.fixedDeltaTime, ForceMode.Impulse);
    }
}
