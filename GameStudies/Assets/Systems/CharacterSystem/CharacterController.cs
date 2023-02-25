using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public int Speed = 2;

    private Animator Animator;
    private Rigidbody Rigidbody;
    private CapsuleCollider CapsuleCollider;
    private AutomaticInputSystem InputSystem;

    protected Vector3 Movement;
    protected float FacingAngle;
    protected Vector3 FacingRotation;

    private float TimeCount;
    private float RotateTimeCount;
    private float BodyDistanceToGround;

    private float GRAVITY = 9.87f * 600;

    private bool MovementStoped;
    private bool Turned;
    private bool Turning;

    private Quaternion TargetRotation;

    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody>();
        CapsuleCollider = GetComponent<CapsuleCollider>();
        InputSystem = GetComponent<AutomaticInputSystem>();

        TimeCount = 0.0f;
        RotateTimeCount = 0.0f;
        BodyDistanceToGround = CapsuleCollider.bounds.extents.y;

        MovementStoped = true;
        Turned = false;
        Turning = false;
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
            float HorizontalComponent = InputSystem.GetAxis("Horizontal");
            float VerticalComponent = InputSystem.GetAxis("Vertical");

            return new float[] { HorizontalComponent, VerticalComponent };
        }
    }

    private Vector3 CalculateMovement(float HorizontalComponent, float VerticalComponent) =>
        new Vector3(HorizontalComponent, 0, VerticalComponent);

    private Vector3 CalculateMovement(float[] Components) => CalculateMovement(Components[0], Components[1]);

    protected enum Orientation
    {
        North,
        NorthWest,
        West,
        SouthWest,
        South,
        SouthEast,
        East,
        NorthEast
    }

    protected Orientation GetDirectionOrientation(Vector3 direction)
    {
        if (direction.x > 0.0f && direction.z > 0.0f) return Orientation.NorthWest;
        if (direction.x > 0.0f && direction.z == 0.0f) return Orientation.West;
        if (direction.x > 0.0f && direction.z < 0.0f) return Orientation.SouthWest;
        if (direction.x == 0.0f && direction.z < 0.0f) return Orientation.South;
        if (direction.x < 0.0f && direction.z < 0.0f) return Orientation.SouthEast;
        if (direction.x < 0.0f && direction.z == 0.0f) return Orientation.East;
        if (direction.x < 0.0f && direction.z > 0.0f) return Orientation.NorthEast;

        return Orientation.North;
    }

    protected float[] GetLookRotation(Vector3 direction)
    {
        var orientation = GetDirectionOrientation(direction);

        switch (orientation)
        {
            case Orientation.NorthWest:
                return new float[] { 0.5f, 0.5f };
            case Orientation.West:
                return new float[] { 1.0f, 0.0f };
            case Orientation.SouthWest:
                return new float[] { 0.5f, -0.5f };
            case Orientation.South:
                return new float[] { 0.0f, -1.0f };
            case Orientation.SouthEast:
                return new float[] { -0.5f, -0.5f };
            case Orientation.East:
                return new float[] { -1.0f, 0.0f };
            case Orientation.NorthEast:
                return new float[] { -1.0f, 0.5f };
            case Orientation.North:
            default:
                return new float[] { 0.0f, 0.0f };
        }
    }

    private void Move(Vector3 direction)
    {
        var Treshold = 0.3f * Vector3.one;
        MovementStoped = Mathf.Abs(direction.x) < Treshold.x && Mathf.Abs(direction.y) < Treshold.y && Mathf.Abs(direction.z) < Treshold.z;
        Turned = direction != Vector3.zero && MovementStoped;

        if(Turned)
        {
            var TurnedRotation = GetLookRotation(direction);
            TargetRotation = Quaternion.LookRotation(new Vector3(TurnedRotation[0], 0, TurnedRotation[1]));
            Turning = true;
        }

        if (Turning)
        {
            transform.rotation = Quaternion.Lerp
            (
                transform.rotation,
                TargetRotation,
                RotateTimeCount * 1.5f
            );

            RotateTimeCount = RotateTimeCount + Time.deltaTime;

            if (Quaternion.Angle(transform.rotation, TargetRotation) <= 0.01f)
            {
                Turning = false;
                Turned = false;
                RotateTimeCount = 0.0f;
            }
        }

        if (!MovementStoped)
        {
            Animator.SetBool("IsWalking", true);

            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction.normalized), TimeCount * 1.5f);
            Rigidbody.AddForce(direction.normalized * (float)Speed * Time.fixedDeltaTime, ForceMode.Impulse);

            TimeCount = TimeCount + Time.deltaTime;
        }
        else
        {
            TimeCount = 0;
            Animator.SetBool("IsWalking", false);
        }
    }

    void Update()
    {
        Movement = CalculateMovement(InputComponents);
    }

    private void FixedUpdate()
    {
        Move(Movement);

        if (!IsGrounded())
            Rigidbody.AddForce(Vector3.down * GRAVITY * Time.fixedDeltaTime, ForceMode.Impulse);
    }
}
