using UnityEngine;

/// <summary>
/// 순찰 웨이포인트 제공
/// 루프 또는 핑퐁 모드로 인덱스 이동
/// </summary>
public class PatrolRoute : MonoBehaviour
{
    [SerializeField] private Transform[] _points;//순찰 포인트 배열
    public bool Loop;//루프 모드 여부
    public bool PingPong;//핑퐁 모드 여부

    private int _index;    //현재 인덱스
    private int _direction;//진행방향(+/-)

    private void Awake()
    {
        _direction = 1;//방향 초기화(+)
        _index = 0;//시작 인덱스는 0
    }

    void Update()
    {

    }

    public Transform GetCurrent()
    {
        if (_points == null || _points.Length == 0)
        {
            return null;
        }
        if (_index < 0 || _index >= _points.Length)
        {
            return null;
        }

        return _points[_index];
    }

    public void MoveNext()
    {
        if (_points == null || _points.Length == 0)
        {
            return;
        }

        //다음 인덱스 예정값
        int next = _index + _direction;

        //범위 내인 경우 다음 인덱스로 처리
        if (next >= 0 && next < _points.Length)
        {
            _index = next;
            return;
        }

        if (Loop)
        {
            _index = 0;
            return;
        }

        if (PingPong)
        {
            _direction *= -1;//방향 반전 처리
            _index += _direction;//반전된 방향으로 이동
            return;
        }

        //어느것도 아닌 경우 경계에서 정지
        _index = Mathf.Clamp(_index, 0, _points.Length - 1);
    }
}
