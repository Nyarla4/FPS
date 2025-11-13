using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Objective 표시 컨트롤러
///     현재 목표 텍스트로 표시
///     완료시 토스트/체크 애니메이션
///     Encounter 시작/완료에 맞춰 기본 목표 세팅 처리
/// </summary>
public class ObjectiveController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyEncounterZone _zone;//이벤트 제공
    [SerializeField] private CanvasGroup _objectiveGroup;//페이드할 패널
    [SerializeField] private TMP_Text _objective;//목표 문장

    [Header("Appearance")]
    [SerializeField] private string _startObjective = "Clear Zone";//시작 시 텍스트
    [SerializeField] private string _afterClearObjective = "Move To Next Zone";//클리어 시 텍스트
    public float FadeSpeed;

    private Coroutine _fadeCo;//페이드 코루틴

    private void OnEnable()
    {
        if (_zone != null)
        {
            _zone.OnEncounterStarted.AddListener(OnEncounterStarted);
            _zone.OnEncounterCompleted.AddListener(OnEncounterCompleted);
        }
    }

    private void OnDisable()
    {
        if (_zone != null)
        {
            _zone.OnEncounterStarted.RemoveListener(OnEncounterStarted);
            _zone.OnEncounterCompleted.RemoveListener(OnEncounterCompleted);
        }
    }

    private void OnEncounterStarted()
    {
        SetObjective(_startObjective);
        PlayFade(1.0f);
    }

    private void OnEncounterCompleted()
    {
        SetObjective(_afterClearObjective);
        PlayFade(1.0f);
    }

    /// <summary> 텍스트 설정 </summary>
    public void SetObjective(string message)
    {
        if (_objective != null)
        {
            _objective.text = message;
        }
    }

    private void PlayFade(float target)
    {
        if (_objectiveGroup == null)
        {
            return;
        }

        if (_fadeCo != null)
        {
            StopCoroutine(_fadeCo);
            _fadeCo = null;
        }

        _fadeCo = StartCoroutine(CoFade(target));
    }

    IEnumerator CoFade(float target)
    {
        float t = 0.0f;
        float start = _objectiveGroup.alpha;

        if (t < 1.0f)
        {
            t += Time.deltaTime;
            if(t > 1.0f)
            {
                t = 1.0f;
            }

            float a = Mathf.Lerp(start, target, t);
            _objectiveGroup.alpha = a;

            yield return null;
        }
    }
}
