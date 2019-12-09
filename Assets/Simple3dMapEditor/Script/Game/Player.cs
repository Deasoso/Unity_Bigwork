using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    LayerMask _NoneMoveCollisionlayer;
    [SerializeField]
    float _moveSpeed = 2f;
    [SerializeField]
    float _jumpPower = 2f;

    bool _isJumping            = false;
    bool _isGround             = false;
    float _groundCheckDistance = 0.5f;

    Rigidbody _rigidbody = null;

    Vector3 _movePosition = Vector3.zero;
    Vector3 _jumpVelocity = Vector3.zero;

    Vector3 _MoveDir    = Vector3.zero;
    Vector3 _spanwPoint = Vector3.zero;

    Vector2 _moveMin    = Vector2.zero;
    Vector2 _moveMax    = Vector2.zero;

    int _starCount = 0;
    Action<int> _onChangeStarCount;

    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    public void Initailized(Vector3 spawnPoint,Vector2 moveMin, Vector2 moveMax, Action<int> changeStarCount)
    {
        _spanwPoint        = spawnPoint;
        _moveMin           = moveMin;
        _moveMax           = moveMax;
        transform.position = spawnPoint;
        _onChangeStarCount = changeStarCount;
  
        _isJumping = false;
        _isGround  = true;
        _starCount = 0;
    }
    
    void Update()
    {
        GroundCheck();
        if (Input.GetKeyDown(KeyCode.Space) && _isGround)
        {
            _isJumping = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
            _MoveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _MoveDir = Vector3.left;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _MoveDir = Vector3.right;
        }
    }
    
    void FixedUpdate()
    {
        Jump();
        Move();
    }

    void Jump()
    {
        if (!_isJumping || !_isGround)
            return;

        _jumpVelocity = Vector3.up * _jumpPower;
        _rigidbody.velocity  = _jumpVelocity;
        _isJumping           = false;
    }

    void Move()
    {
        _movePosition   = transform.position +  (_MoveDir * _moveSpeed * Time.deltaTime);
        bool isHit = false;      
      
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            float rayDist = Mathf.Max(Vector3.Distance(_movePosition, transform.position), 0.2f);
            RaycastHit hit;
            isHit = Physics.BoxCast(transform.position, transform.lossyScale / 2.5f, _MoveDir, out hit, transform.rotation, 0.2f, ~_NoneMoveCollisionlayer);
            if (isHit)
            {
                float x = hit.transform.position.x + (((transform.lossyScale.x / 2f) + (transform.lossyScale.x / 2f)) * -_MoveDir.x);
                _movePosition = new Vector3(x, transform.position.y, transform.position.z);
            }
        }
        
        _movePosition.x = Mathf.Clamp(_movePosition.x, _moveMin.x, _moveMax.x);
        _movePosition.y = Mathf.Clamp(_movePosition.y, _moveMin.y, _moveMax.y);

        _rigidbody.MovePosition(_movePosition);
    }

    void GroundCheck()
    {
        RaycastHit hit;
        for (int i = -1; i < 2; i++)
        {
            Vector3 position = transform.position + (new Vector3(i, 0, 0) * 0.4f);
            if (Physics.Raycast(position, Vector3.down, out hit, _groundCheckDistance))
            {
                if (hit.transform.tag == "Ground")
                {
                    _isGround = true;
                    break;
                }
            }
            else
            {
                _isGround = false;
            }
        }


    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Star")
        {
            _starCount++;
            _onChangeStarCount(_starCount);
            Destroy(other.gameObject);
        }
        if (other.tag == "Ironbed")
        {
            transform.position= _spanwPoint;
            _isJumping        = false;
            _isGround         = false;
        }
    }
}
