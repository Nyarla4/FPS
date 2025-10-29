using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대상 쪽 상태효과 컨테이너
///     Add/Remove/Find/Update 처리
///     외부 시스템(이동/공격/입력)이 질의가능한 보조 API 제공
/// </summary>
public class StatusEffectHost : MonoBehaviour
{
    //현재 활성중인 효과 리스트
    private List<StatusEffect_Base> _effects = new();//활성 인스턴스 목록

    //캐시된 쿼리 값(성능/가독성)
    private float _cachedSpeelMul = 1.0f;//슬로우 및 버프 반영한 최종 이동속도 배율
    private bool _cachedStunned = false;//현재 스턴중 여부

    //외부에서 호출
    public float SpeedMultiplier => _cachedSpeelMul;//이동코드에서 호출, 이동속도 최종배율
    public bool IsStunned => _cachedStunned;//공격/입력에서 호출, 스턴 상태 여부

    void Update()
    {
        float dt = Time.deltaTime;

        //효과 틱
        for (int effIdx = 0; effIdx < _effects.Count; effIdx++)
        {
            StatusEffect_Base e = _effects[effIdx];
            if (e != null)
            {
                e.Tick(dt);
            }
        }

        //만료 제거(역순), 정순인 경우 에러 발생
        for (int effIdx = _effects.Count -1 ; effIdx >= 0; effIdx--)
        {
            StatusEffect_Base e = _effects[effIdx];
            if(e == null)
            {
                _effects.RemoveAt(effIdx);
                continue;
            }
            if (e.IsExpired)
            {
                e.Detach();
                Destroy(e);
                _effects.RemoveAt(effIdx);
            }
        }

        //쿼리 캐시 갱신
        RebuildCachedQueryValues();
    }

    /// <summary>
    /// 효과 추가
    /// 같은 타입이라면 OnReapplied 호출(클래스별 재적용 규칙 내장)
    /// 프리팹을 인스턴스화하여 이 호스트 객체에 붙임 처리
    /// </summary>
    /// <typeparam name="T"></typeparam> T: Template => 자료형을 미리 정해두지 않음, 호출 시점에서 자료형을 지정
    public T AddEffect<T>(T effectPrefab) where T : StatusEffect_Base
    {
        if(effectPrefab == null)
        {
            return null;
        }

        //기존 효과 중 같은 타입 찾기
        T found = FindEffect<T>();
        if (found != null)
        {
            found.OnReapplied();
            return found;
        }

        //새 인스턴스 생성
        T inst = gameObject.AddComponent<T>();
        CopyFields(effectPrefab, inst);//프리셋 값 간단 복사
        inst.Attach(this);
        _effects.Add(inst);

        RebuildCachedQueryValues();
        return inst;
    }

    /// <summary>
    /// 같은 타입의 첫 효과를 반환
    /// 없는 경우 null
    /// </summary>
    private T FindEffect<T>() where T : StatusEffect_Base
    {
        for (int idx = 0; idx < _effects.Count; idx++)
        {
            T casted = _effects[idx] as T;
            if(casted != null)
            {
                return casted;
            }
        }
        return null;
    }

    /// <summary>
    /// 캐시값 재계산: 이동속도 배율, 스턴 여부 등
    /// </summary>
    private void RebuildCachedQueryValues()
    {
        //계산 누적용 지역 변수
        float speedMul = 1.0f;
        bool stunned = false;

        //최종치 계산
        for (int effIdx = 0; effIdx < _effects.Count; effIdx++)
        {
            StatusEffect_Base e = _effects[effIdx];

            //슬로우로 캐스트, 가장 낮은(가장 강한) 배율만 반역
            StatusEffect_Slow slow = e as StatusEffect_Slow;
            if (slow != null)
            {
                float m = slow.SpeedMultiplier;
                speedMul = Mathf.Min(speedMul, m);
            }

            //스턴으로 캐스트, 있으면 true
            StatusEffect_Stun stun = e as StatusEffect_Stun;
            if (stun != null)
            {
                if (stun.IsStunned)
                {
                    stunned = true;
                }
            }
        }

        _cachedSpeelMul = speedMul;
        _cachedStunned = stunned;
    }

    /// <summary>
    /// 간단 필드 복사(프리셋 => 인스턴스)
    /// 필요 필드만 복사
    /// </summary>
    /// <param name="src">복사 대상</param>
    /// <param name="dst">복사 결과</param>
    private void  CopyFields(StatusEffect_Base src, StatusEffect_Base dst)
    {
        if (src == null || dst == null)
        {
            return;
        }

        dst.EffectName = src.EffectName;
        dst.Icon = src.Icon;
        dst.Duraion = src.Duraion;
        dst.RefreshOnReapply = src.RefreshOnReapply;
    }

    /// <summary>
    /// 현재 활성 효과 리스트(아이콘 UI 표시 등)
    /// </summary>
    public List<StatusEffect_Base> GetActiveEffects()
    {
        List<StatusEffect_Base> list = new(_effects);
        return list;
    }
}