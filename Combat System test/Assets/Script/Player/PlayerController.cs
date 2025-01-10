using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float rotateSpeed = 300f;
    [Header("Ground Check")]
    [SerializeField] float groundCheckRidous = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    [Header("部分音效（盔甲）")]
    [SerializeField] private float minMoveThreshold = 0.2f; // 播放音效的最小移动量
    [SerializeField] private float stopDelay = 0.5f; // 停止音效的延迟时间
    private float stopTimer = 0f; // 记录停止音效的计时器
    Quaternion targetRotation;
    CameraController cameraController;
    Animator animator;
    CharacterController characterController;
    MeeleFighter meeleFighter;

    bool isGrounded = false;
    float ySpeed = 0f;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meeleFighter = GetComponent<MeeleFighter>();
    }

    private void Update()
    {
        if (meeleFighter.InAction)
        {
            animator.SetFloat("moveAmount", 0);
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        var moveInput = (new Vector3(h, 0, v)).normalized;//归一化防止对角线方向移动速度更快

        var moveDir = cameraController.PlaneRotation * moveInput;
        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        //检测是否处于地面，设置y轴速度
        GroundCheck();
        if (isGrounded)
        {
            ySpeed = -0.3f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }
        var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;

        //实现移动和转向
        characterController.Move(velocity * Time.deltaTime);
        if (moveAmount > 0f)
        {
            targetRotation = Quaternion.LookRotation(moveDir);//转向
        }
        //平滑旋转
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
        targetRotation, rotateSpeed * Time.deltaTime);

        animator.SetFloat("moveAmount", moveAmount, 0.25f, Time.deltaTime);

        //播放盔甲声
        if (moveAmount > minMoveThreshold && isGrounded)
        {
            if (!AudioManager.instance.IsPlaying("盔甲声"))
            {
                AudioManager.instance.Play("盔甲声");
            }
            stopTimer = 0f; // 重置停止计时器
        }
        else
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= stopDelay && AudioManager.instance.IsPlaying("盔甲声"))
            {
                AudioManager.instance.Stop("盔甲声");
            }
        }
    }


    //地面检测
    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset)
        , groundCheckRidous, groundLayer);
    }

    //unity编辑器中绘制Gizmos，方便观察GroundCheck
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRidous);
    }
}
