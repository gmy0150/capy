using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
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
    float applyjump;
    
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
    [SerializeField] GameObject normalground;
    GameObject groundcheck;
    Vector3 saveground;
    ObjectSetting ObjectSetting;
    bool runningBall;
    float mass;
    float drag;
    public GameObject napal;
    void Start()
    {
        applyjump = jumpForce;
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        rigid = GetComponent<Rigidbody>();
        groundcheck = normalground;
        applySpeed = walkSpeed;
        napal.SetActive(false);
        saveground = groundcheck.transform.localPosition;
        Debug.Log(saveground);
        originPosY = resize.transform.localScale.y;
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
        if(!wait)
        CharacterRotation();
        ForceMove();
        if(napal.active)
        audioR.SoundStop(napal);
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
            if(runningBall)
            napal.SetActive(true);
            audioR.Sound();
        }
    }
    void Ride(){
        LayerMask Ride =LayerMask.GetMask("Ride");
        RaycastHit hit;
        Vector3 rayDirection = transform.forward;
        Vector3 rayOriginal = new Vector3(transform.position.x, theCamera.transform.position.y,transform.position.z);
        float rayDistance = 3f;
        
        Debug.DrawRay(rayOriginal,rayDirection * rayDistance,Color.red);
        if(Physics.Raycast(rayOriginal, rayDirection,out hit ,rayDistance,Ride)){
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ride")){
                isRide = true;
                if(isCrouch)
                    Crouch();
                StartCoroutine(AttachPlayerTos(hit.transform.gameObject));
                Debug.Log(hit.transform.name);
            }else{
                Debug.Log("인식은 됨?" + hit.collider.name);
            }
        }
    }
    IEnumerator AttachPlayerTos(GameObject targetObject){
        Rigidbody rigidbody = targetObject.GetComponent<Rigidbody>();
        rigid.mass = rigidbody.mass;
        mass = rigidbody.mass;
        drag = rigidbody.drag;
        targetObject.transform.rotation = transform.rotation;
        GameObject.DestroyImmediate(targetObject.GetComponent<Rigidbody>());//컴포넌트 제거 스크립트
        wait = true;
        Vector3 colliderCenter = targetObject.GetComponentInChildren<Collider>().bounds.center;
        Vector3 offset = new Vector3(0, targetObject.GetComponentInChildren<Collider>().bounds.extents.y, 0);
        float length = offset.y + 1f;
        Vector3 depth = new Vector3(0, offset.y + 0.4f, 0);

        transform.position = colliderCenter + offset;
        yield return new WaitForSeconds(0.2f);
        targetObject.transform.SetParent(transform);
        wait = false;
        saveObj = targetObject;
        animator = targetObject.GetComponentInChildren<Animator>();
        transform.position = colliderCenter  + offset;

        groundcheck.transform.position = colliderCenter - depth ;
        ObjectSetting = targetObject.GetComponentInChildren<ObjectSetting>();
        applySpeed = ObjectSetting.walkSpeed;
        runningBall = ObjectSetting.runningBall;
        Debug.Log(groundcheck);
        RaycastHit hit;
        if(Physics.Raycast(colliderCenter , Vector3.down,out hit, length,LayerMask.GetMask("Ground"))){
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground")){
                transform.position = hit.point + (hit.normal * length);
            }
            
        }
        Debug.DrawRay(colliderCenter, Vector3.down *length,Color.red,10f);

    }
    void cutObj(){
        Rigidbody rigidbody = saveObj.AddComponent<Rigidbody>();
        rigidbody.mass = mass;
        rigidbody.drag = drag;
        rigid.mass = 1;
        animator.SetFloat("move",0);
        if(!runningBall)
        rigidbody.constraints = RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
        else
        rigidbody.constraints = RigidbodyConstraints.FreezeRotationY;

        transform.SetParent(null);
        saveObj.transform.SetParent(null);
        isRide = false;
        saveObj = null;
        animator = GetComponent<Animator>();
        groundcheck.transform.localPosition = saveground;
        Debug.Log(groundcheck.transform.position);
        ObjectSetting = null;
        applySpeed = walkSpeed;
        if(runningBall)
        getOffBall();
         
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
    void ForceMove(){
        if(runningBall&&isRide){
            applyjump = jumpForce * 1.5f;
            wait = true;
            CharacterRotation();
            Vector3 _moveVertical = transform.forward;
            Vector3 _velocity = _moveVertical.normalized * ObjectSetting.runSpeed;
            rigid.MovePosition(transform.position + _velocity * Time.deltaTime);
            float speed = _velocity.magnitude;
            animator.SetFloat("move",speed);
        }
    }
    void getOffBall(){
        wait = false;
        runningBall = false;
    }
    void IsGround(){
        isGround = Physics.Raycast(groundcheck.transform.position, Vector3.down, 0.2f);
        Debug.DrawRay(groundcheck.transform.position ,Vector3.down * 0.2f,Color.blue,0.2f);
    }
    void TryJump(){
        if(Input.GetKeyDown(KeyCode.Space) && isGround){
            Jump();
        }
    }
    void Jump(){
        if(isCrouch)
        Crouch();

        rigid.velocity = transform.up * applyjump;
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
        if(saveObj != null)
            applySpeed = ObjectSetting.runSpeed;
        animator.speed = 1.5f;
    }

    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
        if(saveObj != null)
            applySpeed = ObjectSetting.walkSpeed;
        animator.speed = 1.2f;
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
            if(saveObj != null)
                applySpeed = ObjectSetting.crounchSpeed;
            applyCrouchPosY = crouchPosY;
            animator.speed = 1f;
        }else{
            applySpeed = walkSpeed;
            if(saveObj != null)
                applySpeed = ObjectSetting.walkSpeed;
            applyCrouchPosY = originPosY; 
            animator.speed = 1.2f;

        }
        StartCoroutine(CrouchCroutine());
        
    }
    IEnumerator CrouchCroutine(){
        float _posY = transform.localScale.y;
        int count = 0;
        while(_posY != applyCrouchPosY){
            count++;
            _posY = Mathf.Lerp(_posY,applyCrouchPosY,0.2f);
            resize.transform.localScale = new Vector3(1,_posY,1);
            if(count > 15)
                break;
            yield return null;
        }
        // theCamera.transform.localScale = new Vector3(1,applyCrouchPosY,1);
        resize.transform.localScale = new Vector3(1,_posY,1);

    }

    void CameraRotation(){
        float _XRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _XRotation * lookSensitivity;

        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -CameraRotationLimit, CameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0, 0);
    }
    void CharacterRotation(){
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0,_yRotation,0) * lookSensitivity;
        rigid.MoveRotation(rigid.rotation * Quaternion.Euler(_characterRotationY));
    }

}
