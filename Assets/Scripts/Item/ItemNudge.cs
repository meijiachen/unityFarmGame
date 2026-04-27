using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemNudge : MonoBehaviour
{
    [Header("摆动参数")]
    public float swingAngle = 15f;    // 弯曲角度
    public float speed = 0.03f;       // 动画速度

    private Quaternion _originalRot;
    private bool _isAnimating = false;

    private void Awake()
    {
        _originalRot = transform.rotation; // 记录自身初始角度
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 只响应玩家（记得给玩家标Tag：Player）
        if (!collision.CompareTag(Tag.Player) || _isAnimating) 
            return;

        // 判断方向：玩家在右边 → 草向右倒，反之向左
        int dir = transform.position.x < collision.transform.position.x ? 1 : -1;
        StartCoroutine(Swing(dir));
    }

    private IEnumerator Swing(int dir)
    {
        _isAnimating = true;

        // 1. 弯曲
        for (int i = 0; i < 6; i++)
        {
            transform.Rotate(0, 0, dir * swingAngle * 0.25f);
            yield return new WaitForSeconds(speed);
        }

        // 2. 回弹
        for (int i = 0; i < 11; i++)
        {
            transform.Rotate(0, 0, -dir * swingAngle * 0.25f);
            yield return new WaitForSeconds(speed);
        }

        // 强制回正，永远不歪
        transform.rotation = _originalRot;
        _isAnimating = false;
    }

    // private WaitForSeconds pause;
    // private bool isAnimating = false;

    // private void Awake()
    // {
    //     pause = new WaitForSeconds(0.04f);
    // }

    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (!isAnimating)
    //     {
    //         if (gameObject.transform.position.x < collision.gameObject.transform.position.x)
    //         {
    //             StartCoroutine(RotateAntiClock());
    //         }
    //         else{
    //             StartCoroutine(RotateClock());
    //         }
    //     }
    // }

    // private IEnumerator RotateClock()
    // {
    //     isAnimating = true;
    //     for (int i = 0;i < 4; i++)
    //     {
    //         gameObject.transform.GetChild(0).Rotate(0f,0f,-2f);
    //         yield return pause;
    //     }

    //      for (int i = 0;i < 5; i++)
    //     {
    //         gameObject.transform.GetChild(0).Rotate(0f,0f,2f);
    //         yield return pause;
    //     }

    //     gameObject.transform.GetChild(0).Rotate(0f,0f,-2f);
    //     yield return pause;
    //     isAnimating = false;
    // }

    // private IEnumerator RotateAntiClock()
    // {
    //     isAnimating = true;
    //     for (int i = 0;i < 4; i++)
    //     {
    //         gameObject.transform.GetChild(0).Rotate(0f,0f,2f);
    //         yield return pause;
    //     }

    //      for (int i = 0;i < 5; i++)
    //     {
    //         gameObject.transform.GetChild(0).Rotate(0f,0f,-2f);
    //         yield return pause;
    //     }

    //     gameObject.transform.GetChild(0).Rotate(0f,0f,2f);
    //     yield return pause;
    //     isAnimating = false;
    // }
}
