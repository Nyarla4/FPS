/// <summary>
/// Dead: 사망상태
/// 상태전환 없이 정지
/// Health.OnDeath 이벤트에서 호출/등록/호출해제되는 상태 클래스
/// </summary>
public class DeadState : BaseState
{
    public DeadState()
    {

    }
    public DeadState(StateManager context)
    {
        _context = context;
    }

    public override string Name()
    {
        return "Dead";
    }

    public override void OnEnter()
    {
        //사망 모션 등 재생
        //Update동안 상태 변화 없이 정지
    }

    public override void OnUpdate(float dt)
    {
        //죽은 상태에서는 아무 행동도 하지 않음
    }

    public override void OnExit()
    {
        //리스폰한다면 Dead상태 해제 시점에서 초기화
        //부활하지 않는 한 실행할 일 없음
    }
}
