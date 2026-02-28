using UnityEngine;

namespace GhostStory
{
    public enum PlayerState { Idle = 0, Walk = 1, Run = 2 };
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {

        [Header("컴포넌트")]
        public CharacterController CCon;
        public Animator Anim;

        public PlayerInputReader playerInput;


        [Header("이동 변수")]
        public float walkSpeed;
        public float runSpeed;
        public bool isRunPressed => playerInput.IsRunPressed;


        [Header("땅 중력")]
        [SerializeField] private float _sphereRadius = 0.05f;
        [SerializeField] private float _sphereYOffset;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _gravity = -9.81f;

        public Vector2 InputDir => playerInput.MoveInput;

        public float lastMoveX;
        public float lastMoveZ;

        private Vector3 _yVelocity;
        Vector3 spherePos;

        #region FSM
        [Header("FSM")]
        private MovementBaseState _currentState;
        public IdleState idleState = new IdleState();
        public WalkState walkState = new WalkState();
        public RunState runState = new RunState();
        #endregion


        private void Awake()
        {
            if (CCon == null) CCon = GetComponent<CharacterController>();
            if (Anim == null) Anim = GetComponent<Animator>();

            playerInput = GetComponent<PlayerInputReader>();
        }

        private void Start()
        {
            SwitchState(idleState);
        }
        
        private void Update()
        {
            CalculateGravity();

            _currentState.UpdateState(this);
        }

        public void UpdateAnimState(PlayerState newState)
        {
            Anim.SetInteger("State", (int)newState);
        }

        public void SwitchState(MovementBaseState newState)
        {
            _currentState?.ExitState(this);
            _currentState = newState;
            _currentState.EnterState(this);
        }

        public void Move(float speed)
        {
            // 이동
            Vector3 moveDir = new Vector3(InputDir.x, 0, InputDir.y).normalized;
            CCon.Move(moveDir * speed * Time.deltaTime);

            // 애니메이션에 방향값 전달
            if (InputDir.magnitude > 0.01f)
            {
                Anim.SetFloat("xInput", InputDir.x);
                Anim.SetFloat("zInput", InputDir.y);
            }

            lastMoveX = InputDir.x;
            lastMoveZ = InputDir.y;
        }

        // 공중에 떠있는지 확인
        bool IsGrounded()
        {
            spherePos = new Vector3(transform.position.x, transform.position.y - _sphereYOffset, transform.position.z);

            // 구체가 바닥 레이어와 맞붙어있으면 true
            if (Physics.CheckSphere(spherePos, _sphereRadius, _groundLayer))
            {
                return true;
            }

            return false;
        }

        private void CalculateGravity()
        {
            if (IsGrounded() && _yVelocity.y < 0)
            {
                _yVelocity.y = -2f;
            }
            else
            {
                _yVelocity.y += _gravity * Time.deltaTime;
            }


            CCon.Move(_yVelocity * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            spherePos = new Vector3(transform.position.x, transform.position.y - _sphereYOffset, transform.position.z);
            // 플레이어보다 절대적인 구체의 사이즈를 작게 해서 벽면에 걸리는 듯한 효과 방지
            Gizmos.DrawWireSphere(spherePos, CCon.radius - _sphereRadius);
        }
    }
}