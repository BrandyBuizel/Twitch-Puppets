using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class puppetControl : MonoBehaviour
{
    public PlayerInput input;

    [HideInInspector] public bool isController = false;
    [HideInInspector] public bool isKeyboard = true;

    [Header("Movement")]
    public float moveSpeed = 100;
    public float rotationSpeed = 4;
    Vector2 movementInput;
    Vector2 lookInput;
    
    [Header("Head and Arms")]
    public GameObject neck;

    public float headRotaitonSpeed = 5;
    public float maxRotX = 10, maxRotY = 5;
    public float armMoveSpeed = 20;
    public float armBounds = 150;
    Rigidbody mRigid;

    Vector3 moveDirection = Vector3.zero;
    Vector3 lookDirection = Vector3.zero;

    [HideInInspector] public bool isInverse = true;
    [HideInInspector] public GameObject rArmIkTarget, lArmIkTarget;
    [HideInInspector] public int isLArm = 0, isRArm = 0;

    [Header("Jumping")]
    public float jumpAm = 30;
    [HideInInspector] public bool grounded = false;
    public float overrideGravity = 30;
    public float groundRayDist = 20;

    float curOverRideGravity = 0;
    
    float jumpTimer = 0, cayoteeTimer = 0;
    
    float isJumpingAm = 0;
    float jumpDither = 0;

    float maxRArmY, maxRArmX, minRArmY, minRArmX, maxLArmY, maxLArmX, minLArmY, minLArmX;
    float startTime = 2;
    float maxRX, maxRY, minRY, minRX;

  
    // Start is called before the first frame update
    void Start()
    {
        mRigid = GetComponent<Rigidbody>();

        maxRX = neck.transform.localPosition.x + maxRotX;
        maxRY = neck.transform.localPosition.y + maxRotY;
        minRX = neck.transform.localPosition.x - maxRotX;
        minRY = neck.transform.localPosition.y - maxRotY;

        InvokeRepeating("subtractSeconds", .022f, .022f);

    }

    void subtractSeconds()
    {
        if (jumpTimer > 0)
            jumpTimer--;

        if (cayoteeTimer > 0)
            cayoteeTimer--;
    }

    void Jump()
    {
        //lazy jump
        isJumpingAm = jumpAm - (jumpDither * 10);
        jumpDither++;
    }

    void getStartBounds()
    {
        maxRArmY = rArmIkTarget.transform.localPosition.y + armBounds;
        maxRArmX = rArmIkTarget.transform.localPosition.x + armBounds;
        minRArmY = rArmIkTarget.transform.localPosition.y - armBounds;
        minRArmX = rArmIkTarget.transform.localPosition.x - armBounds;

        maxLArmY = lArmIkTarget.transform.localPosition.y + armBounds;
        maxLArmX = lArmIkTarget.transform.localPosition.x + armBounds;
        minLArmY = lArmIkTarget.transform.localPosition.y - armBounds;
        minLArmX = lArmIkTarget.transform.localPosition.x - armBounds;

    }

    // Update is called once per frame
    void Update()
    {
        if (startTime > 0)
            startTime--;

        if (startTime == 1)
            getStartBounds();


        var direction = (Camera.main.transform.position - transform.position).normalized;
        if (Vector3.Dot(transform.forward, direction) < 0.2f)
        {
            isInverse = false;
        }
        else
        {
            isInverse = true;
        }

        //Arm Stuff
        if (isLArm == 0)
        {
            moveDirection = (new Vector3(-movementInput.x, 0, -movementInput.y));
        }
        else
        {
            //moveDirection = Vector3.zero;

            moveArm(lArmIkTarget, minLArmX, maxLArmX, minLArmY, maxLArmY, movementInput,armMoveSpeed);
        }


        if (isRArm == 0)
        {
            if (!isInverse)
                //Middle Input is Depth
                lookDirection = (new Vector3(-lookInput.x, lookInput.x-lookInput.y, -lookInput.y));
            else
                lookDirection = (new Vector3(lookInput.x, -lookInput.x+lookInput.y, lookInput.y));

            moveArm(neck, minRX, maxRX, minRY, maxRY, lookInput,headRotaitonSpeed);
        }
        else
        {
            lookDirection = Vector3.zero;

            moveArm(rArmIkTarget, minRArmX, maxRArmX, minRArmY, maxRArmY, lookInput, armMoveSpeed);
        }

        if (!grounded && cayoteeTimer == 0)
        {
            curOverRideGravity = overrideGravity;
        }
        else
        {
            
            curOverRideGravity = 0;
        }


        if(jumpTimer == 0)
            isJumpingAm = 0;


        moveMe();
        rotMe();
        checkGround();

        if (input.actions["Jump"].triggered && grounded) {
            jumpDither = 0;
            jumpTimer = 5;
            cayoteeTimer = 7;
        }

        if (jumpTimer > 0){
            Jump();
        }



        isLArm = (input.actions["lArm"].ReadValue<float>() > 0) ? 1 : 0; ;
        isRArm = (input.actions["rArm"].ReadValue<float>() > 0) ? 1 : 0; ;


    }
    void checkGround()
    {
        grounded = false;

        Ray ray = new Ray(transform.position + Vector3.up, -transform.up * groundRayDist);
        RaycastHit groundHit;
        if (Physics.Raycast(ray, out groundHit, groundRayDist))
        {
            if (groundHit.transform == transform)
                return;


            grounded = true;
        }

    }

    public void onMove(InputAction.CallbackContext ctx) => movementInput = ctx.ReadValue<Vector2>();

    public void onLook(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();



    public void moveArm(GameObject arm, float minX,float maxX, float minY,float maxY, Vector2 input, float speed)
    {
        if (!isInverse)
            arm.transform.Translate(new Vector3(input.x, input.y, 0) * speed * Time.deltaTime);
        else
            arm.transform.Translate(new Vector3(-input.x, input.y, 0) * speed * Time.deltaTime);



        if (arm.transform.localPosition.x > maxX)
            arm.transform.localPosition = new Vector3(maxX, arm.transform.localPosition.y, arm.transform.localPosition.z);

        if (arm.transform.localPosition.x < minX)
            arm.transform.localPosition = new Vector3(minX, arm.transform.localPosition.y, arm.transform.localPosition.z);



        if (arm.transform.localPosition.y > maxY)
            arm.transform.localPosition = new Vector3(arm.transform.localPosition.x, maxY, arm.transform.localPosition.z);

        if (arm.transform.localPosition.y < minY)
            arm.transform.localPosition = new Vector3(arm.transform.localPosition.x, minY, arm.transform.localPosition.z);


    }

    public void moveMe()
    {
        Vector3 newVel;
        
        newVel = moveDirection * moveSpeed * Time.deltaTime;
        newVel.y = -curOverRideGravity + isJumpingAm;

        mRigid.AddForce(newVel, ForceMode.VelocityChange);

        
        if (movementInput != Vector2.zero && isLArm == 0)
        {
            Quaternion newRot = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime / rotationSpeed);
        }
        
    }

    public void rotMe()
    {
        if (input.actions["lRot"].inProgress){
            //Quaternion newRot = Quaternion.LookRotation(15);
            transform.Rotate(0,-rotationSpeed,0);
            //transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * rotationSpeed);
        }

        if (input.actions["rRot"].inProgress){
            //Quaternion newRot = Quaternion.LookRotation(0,0);

            transform.Rotate(0,rotationSpeed,0);
            //transform.rotation = 
        }
    }


    private void isMouseKeyboard()
    {

        // mouse movement
        if (
             Input.GetMouseButton(0) ||
             Input.GetMouseButton(1) ||
             Input.GetMouseButton(2) ||
             Input.GetAxis("Mouse ScrollWheel") != 0.0f || Input.anyKey || Mathf.Abs(Input.GetAxis("checkMouse1")) > 0 || Mathf.Abs(Input.GetAxis("checkMouse2")) > 0)
        {
            isController = false;
            isKeyboard = true;
        }

    }

    private void isControlerInput()
    {
        // joystick buttons
        if (Input.GetKey(KeyCode.Joystick1Button0) ||
           Input.GetKey(KeyCode.Joystick1Button1) ||
           Input.GetKey(KeyCode.Joystick1Button2) ||
           Input.GetKey(KeyCode.Joystick1Button3) ||
           Input.GetKey(KeyCode.Joystick1Button4) ||
           Input.GetKey(KeyCode.Joystick1Button5) ||
           Input.GetKey(KeyCode.Joystick1Button6) ||
           Input.GetKey(KeyCode.Joystick1Button7) ||
           Input.GetKey(KeyCode.Joystick1Button8) ||
           Input.GetKey(KeyCode.Joystick1Button9) ||
           Input.GetKey(KeyCode.Joystick1Button10) ||
           Input.GetKey(KeyCode.Joystick1Button11) ||
           Input.GetKey(KeyCode.Joystick1Button12) ||
           Input.GetKey(KeyCode.Joystick1Button13) ||
           Input.GetKey(KeyCode.Joystick1Button14) ||
           Input.GetKey(KeyCode.Joystick1Button15) ||
           Input.GetKey(KeyCode.Joystick1Button16) ||
           Input.GetKey(KeyCode.Joystick1Button17) ||
           Input.GetKey(KeyCode.Joystick1Button18) ||
           Input.GetKey(KeyCode.Joystick1Button19) ||
           Mathf.Abs(Input.GetAxis("checkAxis1")) > 0 || Mathf.Abs(Input.GetAxis("checkAxis2")) > 0 || Mathf.Abs(Input.GetAxis("checkAxis3")) > 0 || Mathf.Abs(Input.GetAxis("checkAxis4")) > 0)
        {
            isController = true;
            isKeyboard = false;
        }
    }
}
