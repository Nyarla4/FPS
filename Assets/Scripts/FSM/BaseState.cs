using UnityEngine;

/// <summary>
/// 모든 상태의 기본 클래스
/// 각 상태는 Enter/Update/Exit 메서드를 구현
/// Context를 통해 참조 전달
/// </summary>
public abstract class BaseState
{
    /// <summary>
    /// 상태가 동작할 StateManager 참조
    /// </summary>
    protected StateManager _context; //현재 상태가 작동할 대상(State/Idle/Dead 등)

    /// <summary>
    /// 상태 매니저를 설정
    /// 모든 상태는 이를 통해 컨텍스트를 받음
    /// </summary>
    /// <param name="context"></param>
    public void SetContext(StateManager context)
    {
        _context = context;
    }

    /// <summary>
    /// 상태 시작 시 호출
    /// 초기화/효과/애니메이션 시작 등 처리
    /// </summary>
    public virtual void OnEnter()
    {
        //파생 상태에서 구현
    }

    /// <summary>
    /// 매 프레임 호출
    /// 상태 동작 로직 처리
    /// 필요시 _context.RequestStateChange(...)로 상태 변경 가능
    /// </summary>
    public virtual void OnUpdate(float dt)
    {
        //파생 상태에서 구현
    }

    /// <summary>
    /// 상태 종료 시 호출
    /// 정리/후처리 수행
    /// </summary>
    public virtual void OnExit()
    {
        //파생 상태에서 구현
    }

    /// <summary>
    /// 상태 이름 반환
    /// </summary>
    /// <returns></returns>
    public abstract string Name();
}
