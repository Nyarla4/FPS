using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 인카운터 HUD 전체 컨트롤러
///     웨이브, 적 수, 메시지를 표시/애니메이션
///     EnemyEncounterZone의 이벤트를 구독하여 갱신처리
/// </summary>
public class EncounterHUDContorller : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyEncounterZone _zone;//이벤트 발행 구역
    [SerializeField] private CanvasGroup _panelWaveGroup;//웨이브 패널(캔버스 그룹)
    [SerializeField] private RectTransform _panelWaveTransform;//웨이브 패널 트랜스폼(스케일 펄스용)
    [SerializeField] private CanvasGroup _panelObjectiveGroup;//오브젝티브 패널(캔버스 그룹)
    [SerializeField] private TMP_Text _waveLabel;//웨이브 이름
    [SerializeField] private TMP_Text _enemyCounter;//에너미 카운터
    [SerializeField] private TMP_Text _toast;//토스트 문구

    [Header("Appearance")]
    [SerializeField] private string _waveFormat = "Wave {0}";//웨이브 텍스트 서식
    [SerializeField] private string _enemyCounterFormat = "Enemies {0} / {1}";//카운터 텍스트 서식
    public float FadeSpeed = 4.0f;//패널 페이드 속도
    public float PulseScale = 1.5f;//웨이브 갱신 시 잠깐 커지는 배율
    public float PulseDuration = 1.0f;//펄스 지속 시간(초)
    public float ToastDuration = 1.0f;//토스트 노출 유지 시간(초)

    private Coroutine _fadeWaveCo;//웨이브 패널 페이드 코루틴
    private Coroutine _pulseWaveCo;//웨이브 패널 펄스 코루틴
    private Coroutine _toastCo;//토스트 코루틴

    private CanvasGroup _fadeGroup;//현재 페이드 처리중인 그룹
    private void OnEnable()
    {
        if (_zone != null)
        {
            _zone.OnEncounterStarted.AddListener(OnEncounterStarted);
            _zone.OnEncounterCompleted.AddListener(OnEncounterCompleted);
            _zone.OnWaveStarted.AddListener(OnWaveStarted);
            _zone.OnEnemyAliveChanged.AddListener(OnEnemyAliveChanged);
            _zone.OnEnemyTotalChanged.AddListener(OnEnemyTotalChanged);
        }
    }

    private void OnDisable()
    {
        if (_zone != null)
        {
            _zone.OnEncounterStarted.RemoveListener(OnEncounterStarted);
            _zone.OnEncounterCompleted.RemoveListener(OnEncounterCompleted);
            _zone.OnWaveStarted.RemoveListener(OnWaveStarted);
            _zone.OnEnemyAliveChanged.RemoveListener(OnEnemyAliveChanged);
            _zone.OnEnemyTotalChanged.RemoveListener(OnEnemyTotalChanged);
        }
    }

    /// <summary>
    /// 인카운터 시작
    ///     패널 페이드 인
    ///     웨이브/카운터 초기화
    /// </summary>
    private void OnEncounterStarted()
    {
        UpdateWaveLabel(_zone.CurrentWaveIndex);
        UpdateEnemyCounter(_zone.AliveEnemies, _zone.TotalEnemiesThisWave);
        PlayFade(_panelWaveGroup, 1.0f);
        PlayFade(_panelObjectiveGroup, 1.0f);
        ShowToast("Encounter Started");
    }

    /// <summary>
    /// 웨이브 시작
    ///     라벨 갱신
    ///     스케일 펄스 처리
    ///     토스트 처리
    /// </summary>
    /// <param name="waveIndex">웨이브 인덱스</param>
    private void OnWaveStarted(int waveIndex)
    {
        UpdateWaveLabel(waveIndex);
        UpdateEnemyCounter(_zone.AliveEnemies, _zone.TotalEnemiesThisWave);
        PlayPulse(_panelWaveTransform);
        ShowToast($"Wave {waveIndex + 1} Start");
    }

    /// <summary>
    /// 살아있는 적 수 변경
    ///     카운터 즉시 갱신
    /// </summary>
    private void OnEnemyAliveChanged(int alive)
    {
        UpdateEnemyCounter(alive, _zone.TotalEnemiesThisWave);
    }

    /// <summary>
    /// 현재 웨이브 총 소환 수 변경
    ///     카운터 즉시 갱신
    /// </summary>
    private void OnEnemyTotalChanged(int total)
    {
        UpdateEnemyCounter(_zone.AliveEnemies, total);
    }

    /// <summary>
    /// 인카운터 완료
    ///     토스트 출력
    ///     이후 HUD 페이드 아웃
    /// </summary>
    private void OnEncounterCompleted()
    {
        ShowToast("Encounter Clear");
        PlayFade(_panelWaveGroup, 0.0f);
        //Objective패널은 ObjectiveController에서 처리
    }

    private void UpdateWaveLabel(int index)
    {
        int shown = index + 1;
        string text = string.Format(_waveFormat, shown);
        if (_waveLabel != null)
        {
            _waveLabel.text = text; ;
        }
    }

    private void UpdateEnemyCounter(int alive, int total)
    {
        string text = string.Format(_enemyCounterFormat, alive, total);
        if (_enemyCounter != null)
        {
            _enemyCounter.text = text;
        }
    }

    private void ShowToast(string message)
    {
        if (_toastCo != null)
        {
            StopCoroutine(_toastCo);
            _toastCo = null;
        }
        _toastCo = StartCoroutine(CoToast(message));
    }

    private void PlayFade(CanvasGroup group, float target)
    {
        if(group == null)
        {
            return;
        }

        if(_fadeWaveCo != null)
        {
            StopCoroutine (_fadeWaveCo);
            _fadeWaveCo = null;
            _fadeGroup.alpha = 1.0f;
        }

        _fadeWaveCo = StartCoroutine(CoFade(group, target));
    }

    /// <summary>
    /// 약간의 진동 효과
    /// </summary>
    private void PlayPulse(RectTransform target)
    {
        if(target == null)
        {
            return;
        }

        if(_pulseWaveCo != null)
        {
            StopCoroutine (_pulseWaveCo);
            _pulseWaveCo = null;
        }

        _pulseWaveCo = StartCoroutine(CoPulse(target));
    }

    IEnumerator CoToast(string message)
    {
        if (_toast != null)
        {
            _toast.text = message;
        }

        float t = 0.0f;
        float showTime = ToastDuration;

        //간단하게 처리
        //나타남(0.25)-유지-사라짐(0.25)
        float fadeHalf = 0.25f;
        
        //나타남
        float a = 0.0f;
        while (t < fadeHalf)
        {
            t += Time.deltaTime;
            float k = t / fadeHalf;
            if (k > 1.0f)
            {
                k = 1.0f;
            }
            a = Mathf.Lerp(0.0f,1.0f, k);
            SetToastAlpha(a);
            yield return null;
        }

        //유지
        float wait = showTime;
        while (wait > 0.0f)
        {
            wait -= Time.deltaTime;
            yield return null;
        }

        //사라짐
        t = 0.0f;
        while (t < fadeHalf)
        {
            t += Time.deltaTime;
            float k = t / fadeHalf;
            if (k > 1.0f)
            {
                k = 1.0f;
            }
            a = Mathf.Lerp(1.0f, 0.0f, k);
            SetToastAlpha(a);
            yield return null;
        }

        SetToastAlpha(0.0f);
    }

    /// <summary>
    /// 그룹에 전체적으로 페이드 효과를 주기 위함
    /// </summary>
    IEnumerator CoFade(CanvasGroup group, float target)
    {
        _fadeGroup = group;

        float t = 0.0f;
        float start = group.alpha;

        while (t < 1.0f)
        {
            t += Time.deltaTime * FadeSpeed;
            if (t > 1.0f)
            {
                t = 1.0f;
            }

            float a = Mathf.Lerp(start, target, t);
            group.alpha = a;

            yield return null;
        }
    }

    IEnumerator CoPulse(RectTransform target)
    {
        Vector3 original = target.localScale;//원래 스케일
        Vector3 peak = original * PulseScale;//목표 스케일
        float half = PulseDuration * 0.5f;//펄스 처리 시간의 절반

        float t = 0.0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float k = t/ half;
            if (k > 1.0f)
            {
                k = 1.0f;
            }

            target.localScale = Vector3.Lerp(original, peak, k);
            yield return null;
        }
        
        t = 0.0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float k = t/ half;
            if (k > 1.0f)
            {
                k = 1.0f;
            }

            target.localScale = Vector3.Lerp(peak, original, k);
            yield return null;
        }

        target.localScale = original;
    }

    private void SetToastAlpha(float alpha)
    {
        if (_toast != null)
        {
            Color c = _toast.color;
            c.a = alpha;
            _toast.color = c;
        }
    }
}
