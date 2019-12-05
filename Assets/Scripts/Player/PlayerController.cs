using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    //引用CharacterController  
    CharacterController characterController;
    //重力  
    public float gravity = 10;
    //水平移动的速度  
    public float walkSpeed = 5;
    //弹跳高度  
    public float jumpHeight = 0.1f;
    //角色攻击动画名
    private string attackname = "Attack01";
    //角色面向
    private bool attacking, jumping;
    private bool aniplaying = false;
    public static bool LookRight = true;
    public GameObject animaobj;
    private Animator m_anim;
    private AnimatorStateInfo info;
    // 控制角色的移动方向  
    Vector3 moveDirection = Vector3.zero;
    float horizontal = 0;
    // Use this for initialization  
    void Start()
    {
        m_anim = animaobj.GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        jumping = false;
        playstandanima();
    }

    // Update is called once per frame  
    void Update()
    {
        info = m_anim.GetCurrentAnimatorStateInfo(0);
        Debug.Log(info);
        characterController.Move(moveDirection * Time.deltaTime);
        // aniplaying = false;
        adjustlook();
        //Debug.Log("");
        horizontal = Input.GetAxis("Horizontal");
        //控制角色的重力  
        moveDirection.y -= gravity * Time.deltaTime;
        //控制角色右移（按d键和右键时）  在这里不直接使用0而是用0.01f是因为使用0之后会持续移动，无法静止  
        if (horizontal > 0.01f)
        {
            moveDirection.x = horizontal * walkSpeed;
            // LookRight = true;
        } //控制角色左移（按a键和左键时）  
        if (horizontal < 0.01f)
        {
            moveDirection.x = horizontal * walkSpeed;
        }
        //弹跳控制  
        if (characterController.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumping = true;
                moveDirection.y = jumpHeight;
            }else{
                jumping = false;
            }
        }
        // 判断动画是否播放完成
        
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown("right"))
        {
            LookRight = true;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown("left"))
        {
            LookRight = false;
        }
        if (Input.GetKey(KeyCode.J))
        {
            playattackanima();
        }
        // else if (jumping)
        // {
        //     playjumpanima(); 
        // }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey("left") || Input.GetKey(KeyCode.D) || Input.GetKey("right"))
        {
            playwalkanima();
        }
        else
        {
            playstandanima();
        }
    }
    void adjustlook()
    {
        if (LookRight)
        {
            rorate(1);
        }
        else
        {
            rorate(0);
        }
    }
    private const int rate = 5;
    private float delaultAngle;
    private int count;
    private bool isRotate = false;
    private Quaternion targetRotation;
    private float origionY;
    void rorate(int i)
    {
        //　if (!isRotate)
        //{
        if (i == 0)//Input.GetKeyDown(KeyCode.LeftArrow)
        {
            isRotate = true;
            count++;
            targetRotation = Quaternion.Euler(0, delaultAngle * count + origionY - 90, 0) * Quaternion.identity;
        }
        else if (i == 1)//Input.GetKeyDown(KeyCode.RightArrow)
        {
            isRotate = true;
            count--;
            targetRotation = Quaternion.Euler(0, delaultAngle * count + origionY + 90, 0) * Quaternion.identity;
        }
        // }
        // else
        // {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5);
        if (Quaternion.Angle(targetRotation, transform.rotation) < 1)
        {
            transform.rotation = targetRotation;
            //      isRotate = false;
        }
        // }
    }
    void playwalkanima()
    {
        m_anim.SetBool("startrun", true);
        // if (!info.IsName("run_00"))
        // {
        //     Debug.Log("running..");
        //     m_anim.CrossFade("run_00", 0.5f);
        // }
    }
    void playstandanima()
    {
        m_anim.SetBool("startjump", false);
        m_anim.SetBool("startrun", false);
        // if (!info.IsName("idle_00"))
        // {
        //     Debug.Log("standing..");
        //     m_anim.CrossFade("idle_00", 0.5f);
        // }
    }
    void playjumpanima()
    {
        m_anim.SetBool("startjump", true);
        m_anim.SetBool("startrun", false);
        // if (!info.IsName("Jump"))
        // {
        //     Debug.Log("jumping..");
        //     m_anim.CrossFade("Jump", 0.5f);
        // }
    }
    void playattackanima()
    {
        m_anim.CrossFade(attackname,0.5f);//.enabled = true;//.Play(attackname);
    }
    //按下攻击按键时创建子弹的prefab，也就是bulletPrefab。  
    void Attack()
    {

    }
    //void onCollisionEnter(Collider other){
    //	Debug.Log("fuck");
    //	}
}