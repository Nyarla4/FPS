using UnityEngine;

/// <summary>
/// 좌클릭 시 서버에 FIRE 명령 송신
///     서버가 sim.yaw/pitch로 판정 => 추가 데이터 생략
///     발사 연출(머즐/반동/셰이크)는 로컬에서 즉시 재생
/// </summary>
public class InputSenderFire : MonoBehaviour
{
    public float LocalFireCooldown = 0.08f;//로컬 피드백 쿨다운(서버 쿨다운과 유사하게 할 것)
    [Header("Optional")]
    public MuzzleFlash MuzzleFlash;//머즐 플래시 연출 컴포넌트
    public CameraRecoil CameraRecoil;//카메라 반동 컴포넌트
    public ScreenShake ScreenShake;//화면 진동 컴포넌트

    private float _lastLocalFireTime;//마지막 로컬 발사 시간(연출 중복 방지용)

    void Update()
    {
        bool pressed = Input.GetMouseButtonDown(0);//좌클릭 1회
        if (pressed)
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        float now = Time.time;
        if (now < _lastLocalFireTime + LocalFireCooldown)
        {
            //쿨타임 덜지났으면 발사 실패
            return;
        }
        _lastLocalFireTime = now;

        //서버에 FIRE 전송
        if (NetworkRunner.instance != null)
        {
            NetworkRunner runner = NetworkRunner.instance;
            bool isClient = runner.IsClientConnected();
            bool isServer = runner.IsServerRunning();

            if (isClient)
            {
                runner.ClientSendLine("FIRE|");
            }
            else if (isServer)
            {
                //호스트전용(클라이언트가 아닌 서버 단독 테스트인 경우)
                runner.ServerInjectCommand(0, "FIRE", "");
            }
        }

        //로컬 연출 처리
        if (MuzzleFlash != null)
        {
            MuzzleFlash.PlayOnce();
        }
        if (CameraRecoil != null)
        {
            CameraRecoil.Kick(2.2f, 0.6f);//반동 세기 예시
        }
        if (ScreenShake != null)
        {
            ScreenShake.ShakeOnce(0.08f, .012f);
        }
    }
}