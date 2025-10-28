using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 폭발물 던지기 제어 스크립트
///     짧은 조작: 기본속도로 던짐
///     길게 누름: 던지기 힘(파워)을 충전 후 던짐
/// </summary>
public class ThrowableController : MonoBehaviour
{
    [Header("Refs")]
    public Camera PlayerCamera; // 플레이어 시야 카메라
    public Transform Hand; // 손/ 카메라의 위치
    public Grenade GrenadePrefab; //수류탄 프리팹

    [Header("Throw")]
    public float BaseThrowSpeed = 14.0f; //기본 던지기 속도
    public float MaxThrowSpeed = 22.0f; //충전했을 때 최대 속도
    public float ChargeTime = 1.0f; //충전 완료까지 걸리는 시간(초)
    public float AngularSpin = 20.0f; //회전속도(랜덤)

    private bool _charging; //충전 중 여부
    private float _charge; //0~1 사이값으로 충전 정도
    private bool _fireRequested; //던지기 요청 플래그

    void Update()
    {
        if (_fireRequested)
        {
            _fireRequested = false;
            ThrowOne();
        }

        if (_charging)
        {//충전 중일 때
            float inc = 0.0f;
            if (ChargeTime > 0.0001f)
            {
                inc = Time.deltaTime / ChargeTime;
            }
            _charge += inc;
            if (_charge > 1.0f)
            {
                _charge = 1.0f;
            }
        }
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _charging = true;
            _charge = 0.0f;
        }
        else if (context.canceled)
        {
            _charging = false;
            _fireRequested = true;
        }
    }

    private void ThrowOne()
    {
        if (GrenadePrefab == null || PlayerCamera == null)
        {
            return;
        }

        //카메라 위치/방향
        Vector3 origin = Hand != null ? Hand.position : PlayerCamera.transform.position;
        Vector3 dir = PlayerCamera.transform.forward;

        //던지기 속도
        float spd = Mathf.Lerp(BaseThrowSpeed, MaxThrowSpeed, _charge);
        Vector3 vel = dir * spd;

        //회전값(랜덤 방향으로)
        Vector3 ang = Random.onUnitSphere * AngularSpin;

        //수류탄을 생성 후 Throw 함수 호출
        Grenade g = Instantiate(GrenadePrefab, origin, Quaternion.identity);
        g.Throw(vel, ang);

        //충전값 초기화
        _charge = 0.0f;
    }
}
