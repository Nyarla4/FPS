using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 전체 붉게 플래시 연출
///     이미지의 알파값 대신 캔버스그룹의 알파값을 제어하여 페이드인/아웃 처리
///     대미지양에 비례한 강도를 누적하여 자연스럽게 감쇠 처리
///     싱글톤 적용(간편 호출용)
/// </summary>
public class ScreenDamageOverlay : MonoBehaviour
{
    [Header("Refs")]
    public Image OverlayImage;//전체 화면 덮는 이미지(붉은 색)
    public CanvasGroup CanvasGroup;//알파 제어용 캔버스그룹(동일 오브젝트 권장)

    [Header("Appearance")]
    public Color OverlayColor = Color.red;//오버레이 색상(알파는 캔버스 그룹에서 제어)
    [Range(0f, 1f)]
    public float MaxAlpha = 0.6f;//최대 알파(상한 캡, 0~1)
    public float HitBoost = 0.35f;//대미지 100 기준 가중치(상황에 맞게 조정할 것)
    public float FadeOutSpeed = 2.5f;//초당 감쇠 속도

    [Header("Optional Pulse")]
    public bool UseQuickPulse = true;//피격 직후 잠깐 알파를 살짝 올려서 틱 하는 느낌 낼지 여부
    public float PulseUpSpeed = 40.0f;//펄스 올라가는 속도(고속 권장)
    public float PulseDuration = 0.05f;//펄스 유지 시간(초)

    public static ScreenDamageOverlay Instance;//싱글톤

    private float _targetAlpha;//현재 목표 알파(감쇠 대상)
    private float _pulseTimer;//펄스 유지 타이머(0보다 크면 펄스 상태)

    private void Awake()
    {
        //싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }

        if (OverlayImage == null)
        {
            OverlayImage = GetComponent<Image>();
        }

        if (CanvasGroup == null)
        {
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        //초기 색 및 알파값 셋업
        if (OverlayImage != null)
        {
            OverlayImage.color = OverlayColor;
        }
        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = 0.0f;
        }

        _targetAlpha = 0.0f;
        _pulseTimer = 0.0f;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        //펄스 처리: 피격 직후 잠깐 빠르게 올라간 후, Duration이 지나면 일반 감쇠로 복귀
        if (UseQuickPulse)
        {
            if (_pulseTimer > 0.0f)
            {
                _pulseTimer -= dt;
                if (_pulseTimer < 0.0f)
                {
                    _pulseTimer = 0.0f;
                }
            }
        }

        //현재 알파 값
        float current = 0.0f;
        if (CanvasGroup != null)
        {
            current = CanvasGroup.alpha;
        }

        //목표 알파값으로 이동
        float next = current;

        //펄스중인 경우 빠르게 상향 보간
        if (_pulseTimer > 0.0f)
        {
            float stepUp = PulseUpSpeed * dt;
            next = current + stepUp;
        }
        else
        {//통상의 경우 일반 감쇠, 목표까지 선형 이동
            float stepDown = FadeOutSpeed * dt;
            next = Mathf.MoveTowards(current, _targetAlpha, stepDown);
        }

        //범위 클램프 처리
        next = Mathf.Clamp(next, 0.0f, MaxAlpha);

        //적용
        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = next;
        }

        //목표 알파값도 0으로 지속 감쇠 처리
        _targetAlpha -= FadeOutSpeed * dt;
        if (_targetAlpha < 0.0f)
        {
            _targetAlpha = 0.0f;
        }
    }

    /// <summary>
    /// 대미지 발생시 호출
    /// 대미지 양과 최대 체력으로 강도 계산 및 누적
    /// </summary>
    /// <param name="damage">받은 피해량(양수)</param>
    /// <param name="maxHealth">플레이어 최대 체력(강도 정규화 계산용)</param>
    public void PlayDamageFlash(float damage, float maxHealth)
    {
        if (damage <= 0.0f)
        {
            return;
        }
        if (maxHealth <= 0.0f)
        {
            maxHealth = 100.0f;
        }

        //대미지 비율
        float ratio = damage / maxHealth;

        //새로 더할 목표 알파 증가량 계산
        float add = ratio * HitBoost;//HitBoost: 100% 대미지 가정시 더할 양(디자이너가 조정해야할 부분)

        //목표 알파값 누적
        _targetAlpha += add;

        //상한 캡
        if (_targetAlpha > MaxAlpha)
        {
            _targetAlpha = MaxAlpha;
        }

        //펄스 트리거
        if (UseQuickPulse)
        {
            _pulseTimer = PulseDuration;
        }
    }

    /// <summary>
    /// 외부에서 강도 직접 지정시 호출
    /// </summary>
    /// <param name="normalized">강도(0~1)</param>
    public void PlayDamageFlashNormalized(float normalized)
    {
        float n = Mathf.Clamp01(normalized);

        //정규화 값을 MaxAlpha에 매핑
        float add = n * HitBoost;
        _targetAlpha += add;

        if (_targetAlpha > MaxAlpha)
        {
            _targetAlpha = MaxAlpha;
        }
    }
}