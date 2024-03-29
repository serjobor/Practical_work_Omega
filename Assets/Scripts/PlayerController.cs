﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Animator animator;
    Rigidbody rb;

    Vector3 startGamePosition;
    Quaternion startGameRotation;
    Coroutine movingCoroutine;

    bool isMoving = false;
    bool isJumping = false;

    public float laneChangeSpeed = 15;
    public float jumpPower = 15;
    public float jumpGravity = -40;
    float pointStart;
    float pointFinish;
    float laneOffset;
    float realGravity = -9.8f;
    float lastVectorX;

    [SerializeField] private int coins;
    [SerializeField] private Text coinsText;

    // Start is called before the first frame update
    void Start()
    {
        laneOffset = MapGenerator.instance.laneOffset;
        rb = GetComponent<Rigidbody>();

        startGamePosition = transform.position;
        startGameRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A) && pointFinish > -laneOffset)
        {
            MoveHorizontal(-laneChangeSpeed);
        }

        if(Input.GetKeyDown(KeyCode.D) && pointFinish < laneOffset)
        {
            MoveHorizontal(laneChangeSpeed);

        }

        if (Input.GetKeyDown(KeyCode.W) && isJumping == false)
        {
            Jump();
        }
    }

    void Jump()
    {
        isJumping = true;
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        Physics.gravity = new Vector3(0, jumpGravity, 0);
        StartCoroutine(StopJumpCoroutine());
    }

    IEnumerator StopJumpCoroutine()
    {
        do
        {
            yield return new WaitForSeconds(0.02f);
        }while (rb.velocity.y != 0);

        isJumping = false;
        Physics.gravity = new Vector3(0, realGravity, 0);
    }

    void MoveHorizontal(float speed)
    {
        pointStart = pointFinish;
        pointFinish += Mathf.Sign(speed) * laneOffset;

        if(isMoving)
        {
            StopCoroutine(movingCoroutine);
            isMoving = false;
        }

        movingCoroutine = StartCoroutine(MoveCoroutine(speed));     
    }

    IEnumerator MoveCoroutine(float vectorX)
    {
        isMoving = true;

        while (Mathf.Abs(pointStart - transform.position.x) < laneOffset)
        {
            yield return new WaitForFixedUpdate();

            rb.velocity = new Vector3(vectorX, rb.velocity.y, 0);

            lastVectorX = vectorX;

            float x = Mathf.Clamp(transform.position.x, Mathf.Min(pointStart, pointFinish), Mathf.Max(pointStart, pointFinish));
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        rb.velocity = Vector3.zero;
        transform.position = new Vector3(pointFinish, transform.position.y, transform.position.z);

        if(transform.position.y > 2)
        {
            rb.velocity = new Vector3(rb.velocity.x, -10, rb.velocity.z);
        }

        isMoving = false;
    }

    public void StartGame()
    {
        GameObject.Find("RoadGenerator").GetComponent<RoadGenerator>().StartLevel();
        //RoadGenerator.instance.StartLevel();
    }
    public void ResetGame()
    {
        rb.velocity = Vector3.zero;
        pointStart = 0;
        pointFinish = 0;

        transform.position = startGamePosition;
        transform.rotation = startGameRotation;

        ///////////////////////////////////////
        coins = 0;
        coinsText.text = coins.ToString();
        ///////////////////////////////////////
       
        GameObject.Find("RoadGenerator").GetComponent<RoadGenerator>().ResetLevel();
        //RoadGenerator.instance.ResetLevel();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Ramp")
        {
            rb.constraints |= RigidbodyConstraints.FreezePositionZ;
        }
        if (other.gameObject.tag == "Lose")
        {
            ResetGame();
        }
        ///////////////////////////////////////////////
        if (other.gameObject.tag == "Coin")
        {
            coins++;
            coinsText.text = coins.ToString();
            //Destroy(other.gameObject);
        }
        //////////////////////////////////////////////
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ramp")
        {
            rb.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }

        if (collision.gameObject.tag == "NotLose")
        {
            MoveHorizontal(-lastVectorX);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "RampPlane")
        {
            if(rb.velocity.x == 0 && isJumping == false)
            {
                rb.velocity = new Vector3(rb.velocity.x, -10, rb.velocity.z);
            }
        }
    }
    //public void StartGame()
    //{
    //    //RoadGenerator.instance.StartLevel();
    //}

    //public void ResetGame()
    //{
    //    transform.position = startGamePosition;
    //    transform.rotation = startGameRotation;
    //    //RoadGenerator.instance.ResetLevel();
    //}
}
