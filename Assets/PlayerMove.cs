using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] float crounchSpeed;
    float applySpeed;
    [SerializeField] float crouchPosY;
    float originPosY;
    float applyCrouchPosY;
    [SerializeField] float jumpForce;
    
    [SerializeField] float lookSensitivity;
    [SerializeField] 
    float CameraRotationLimit;
    float currentCameraRotationX;
    bool isCrouch;
    bool isRun;
    [SerializeField] GameObject resize;
    bool isGround;
    [SerializeField] Camera theCamera;
    private Rigidbody rigid;
    CapsuleCollider capsuleCollider;
    bool isRide;
    bool wait;
    GameObject saveObj;
    Animator animator;
    AudioR audioR;
    void Start()
    {
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        rigid = GetComponentInChildren<Rigidbody>();

        applySpeed = walkSpeed;

        originPosY = transform.localScale.y;
        applyCrouchPosY = originPosY;
        if(saveObj == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        TrySkill();
        TryRide();
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();
    }
    void TryRide(){
        if(Input.GetKeyDown(KeyCode.F)&&!isRide){
            Ride();
        }
        else if(Input.GetKeyDown(KeyCode.F)&&isRide){
            cutObj();
        }
    }
    void TrySkill(){
        if(Input.GetKeyDown(KeyCode.R)){
            Sound();
        }

    }
    void Sound(){
        if(saveObj != null){
            audioR = saveObj.GetComponentInChildren<AudioR>();
            Debug.Log("?");
            audioR.Sound();

        }
    }
    void Ride(){
        LayerMask Ride =LayerMask.GetMask("Ride");
        RaycastHit hit;
        Vector3 rayDirection = transform.forward;
        Vector3 rayOriginal = new Vector3(transform.position.x, theCamera.transform.position.y,transform.position.z);
        float rayDistance = 3f;
        

        if(Physics.Raycast(rayOriginal, rayDirection,out hit ,rayDistance,Ride)){
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ride")){
                isRide = true;
                if(isCrouch)
                    Crouch();
                StartCoroutine(AttachPlayerTos(hit.transform.gameObject));
            }else{
                Debug.Log("인식은 됨?" + hit.collider.name);
            }
        }
    }
    IEnumerator AttachPlayerTos(GameObject targetObject){
        transform.SetParent(targetObject.transform);
        transform.localPosition = Vector3.zero;
        wait = true;
        yield return new WaitForSeconds(0.2f);
        Vector3 colliderCenter = targetObject.GetComponent<Collider>().bounds.center;
        Vector3 offset = new Vector3(0, targetObject.GetComponent<Collider>().bounds.extents.y, 0);
        transform.position = colliderCenter + offset;
        yield return new WaitForSeconds(0.1f);
        transform.SetParent(null);
        yield return new WaitForSeconds(0.1f);
        targetObject.transform.SetParent(transform);
        wait = false;
        saveObj = targetObject;
        animator = targetObject.GetComponent<Animator>();
        targetObject.transform.rotation = transform.rotation;
        transform.position = targetObject.GetComponent<Collider>().bounds.center  + offset;

    }
    void cutObj(){
        transform.SetParent(null);
        saveObj.transform.SetParent(null);
        isRide = false;
        saveObj = null;
        animator = GetComponent<Animator>();
    }
    
    void Move(){
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;
        if(!wait)
        rigid.MovePosition(transform.position + _velocity * Time.deltaTime);
        float speed = _velocity.magnitude;  
        animator.SetFloat("move",speed);
    }
    void IsGround(){
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }
    void TryJump(){
        if(Input.GetKeyDown(KeyCode.Space) && isGround){
            Jump();
        }
    }
    void Jump(){
        if(isCrouch)
        Crouch();

        rigid.velocity = transform.up * jumpForce;
    }

    void TryRun(){
        if(Input.GetKey(KeyCode.LeftShift)){
            Running();
        }
        if(Input.GetKeyUp(KeyCode.LeftShift)){
            RunningCancel();
        }
    }

    private void Running()
    {  
        if(isCrouch)
            Crouch();
        isRun = true;
        applySpeed = runSpeed;
    }

    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }
    void TryCrouch(){
        if(Input.GetKeyDown(KeyCode.LeftControl)){
            Crouch();
        }
    }
    void Crouch(){
        isCrouch = !isCrouch;
        if(isCrouch){
            applySpeed = crounchSpeed;
            applyCrouchPosY = crouchPosY;
        }else{
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;

        }
        StartCoroutine(CrouchCroutine());
        
    }
    IEnumerator CrouchCroutine(){
        float _posY = transform.localScale.y;
        int count = 0;
        while(_posY != applyCrouchPosY){
            count++;
            _posY = Mathf.Lerp(_posY,applyCrouchPosY,0.2f);
            transform.localScale = new Vector3(1,_posY,1);
            if(count > 15)
                break;
            yield return null;
        }
        // theCamera.transform.localScale = new Vector3(1,applyCrouchPosY,1);
    }

    void CameraRotation(){
        float _XRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _XRotation * lookSensitivity;

        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -CameraRotationLimit, CameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);
    }
    void CharacterRotation(){
        if(!wait){
            float _yRotation = Input.GetAxisRaw("Mouse X");
            Vector3 _characterRotationY = new Vector3(0,_yRotation,0) * lookSensitivity;
            rigid.MoveRotation(rigid.rotation * Quaternion.Euler(_characterRotationY));

        }
    }

}
