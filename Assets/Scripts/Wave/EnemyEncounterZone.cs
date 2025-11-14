using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 조건: 플레이어가 트리거 진입 시
/// 위치: 데이터에 세팅된 스폰포인트에
/// 실행: 세팅된 수의 적을 스폰
/// </summary>
public class EnemyEncounterZone : MonoBehaviour
{
    [SerializeField] private string _encounterName = "EncounterA";//교전명
    [SerializeField] private bool _startOnPlayerEnter = true;//트리거 내에 진입시 자동 시작 처리
    private string _playerTag = "Player";

    [SerializeField] private WaveDefinition[] _waves;//순서대로 실행시킬 웨이브들
    [SerializeField] private int _maxAllEnemies = 10;//동시 생존가능한 최대 적 수량
    [SerializeField] private bool _endEncounterWhenDone = true;//모든 웨이브 종료시 인카우터 자동 종료 여부

    [Header("Optional")]
    public GameObject[] LockOnStart;//이벤트 시작시 이동 혹은 잠금 처리할 개체
    public GameObject[] UnlockOnEnd;//이벤트 종료시 이동 혹은 잠금해제 처리할 개체

    private bool _activated = false;//활성화 여부, 한번 활성화 했었으면 플레이어가 다시 돌아와도 활성화되지 않도록
    private int _currentWaveIndex = -1;//현재 웨이브 인덱스값
    private float _waveCooldownTimer;//다음 웨이브 넘어가기위한 타이머 체크

    private int _aliveEnemies;//현재 생존중인 적 개체 수
    private int _totalSpawnedThisWave;//현재 웨이브에서 스폰된 적의 수량(스폰 처리가 되었음)
    private int _totalToSpawnThisWave;//현재 웨이브에서 스폰될 적의 수량(이만큼 스폰이 되어야함)

    private class RuntimeWaveEntryState
    {//웨이브별 스폰 진행도 추적용
        public WaveEnemyEntry Config;
        public int SpawnedCount;//스폰한 수
        public float NextSpawnTime;//다음 스폰까지의 시간
    }

    private List<RuntimeWaveEntryState> _entryStates = new();

    private Collider _zoneCollider;//콜라이더 참조용

    public UnityEvent OnEncounterStarted;//인카운터 시작
    public UnityEvent OnEncounterCompleted;//인카운터 종료
    public UnityEvent<int> OnWaveStarted;//웨이브 시작
    public UnityEvent<int> OnEnemyAliveChanged;//살아있는 적 수 변경
    public UnityEvent<int> OnEnemyTotalChanged;//웨이브 총 적 수 변경

    public int CurrentWaveIndex => _currentWaveIndex;
    public int AliveEnemies => _aliveEnemies;
    public int TotalEnemiesThisWave => _totalToSpawnThisWave;

    void Update()
    {
        if (!_activated)
        {//활성화중이 아닌 경우
            return;
        }

        if(_waves == null)
        {//웨이브가 없는 경우
            return;
        }

        if(_currentWaveIndex <0 || _currentWaveIndex >= _waves.Length)
        {//웨이브 인덱스 값이 맞지 않는 경우
            return;
        }

        WaveDefinition wave = _waves[_currentWaveIndex];
        UpdateWaveSpawning(wave);
        CheckWaveCompletion(wave);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_startOnPlayerEnter || other == null)
        {
            return;
        }

        if (other.CompareTag(_playerTag))
        {
            StartEncounter();
        }
    }

    /// <summary>
    /// 인카운트 시작 처리
    /// </summary>
    public void StartEncounter()
    {
        if (_activated)
        {//이미 활성화 했으면 다시 시작하지 않음
            return;
        }

        //초기화
        _activated = true;
        _currentWaveIndex = 0;
        _waveCooldownTimer = 0.0f;
        _aliveEnemies = 0;

        SetupWaveRuntimeState();

        SetObjectActive(LockOnStart, true);
        SetObjectActive(UnlockOnEnd, false);

        OnEncounterStarted?.Invoke();
        OnWaveStarted?.Invoke(_currentWaveIndex);
        OnEnemyAliveChanged?.Invoke(_aliveEnemies);
        OnEnemyTotalChanged?.Invoke(_totalToSpawnThisWave);
    }

    /// <summary>
    /// 인카운트 종료 시
    /// </summary>
    private void EncounterCompleted()
    {
        if (_endEncounterWhenDone)
        {
            _activated = false;
        }

        SetObjectActive(LockOnStart, false);
        SetObjectActive(UnlockOnEnd, true);

        OnEncounterCompleted?.Invoke();
    }

    private void SetObjectActive(GameObject[] objects, bool active)
    {
        for (int objIdx = 0; objIdx < objects.Length; objIdx++)
        {
            var obj = objects[objIdx];
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    /// <summary>
    /// 적 개체 사망시 통지 이벤트
    /// </summary>
    public void OnEnemyDead(EnemyLifetimeReporter reporter)
    {
        if(_aliveEnemies > 0)
        {
            --_aliveEnemies;
            OnEnemyAliveChanged?.Invoke(_aliveEnemies);
        }
    }

    /// <summary>
    /// 웨이브 완료 조건 확인
    /// 다음 웨이브 전환 혹은 인카운터 종료 처리
    /// </summary>
    /// <param name="wave"></param>
    private void CheckWaveCompletion(WaveDefinition wave)
    {
        if(_totalSpawnedThisWave < _totalToSpawnThisWave)
        {//스폰이 덜되었으면 return
            return;
        }

        if (wave.WaitUntilAllDead)
        {//다 죽어야 끝나는 웨이브인 경우
            if(_aliveEnemies > 0)
            {//살아있는 개체가 있다면 return
                return;
            }
        }

        //쿨타임 체크
        if (_waveCooldownTimer <= 0f)
        {
            _waveCooldownTimer = wave.DelayAfterWave;
            return;
        }
        if (_waveCooldownTimer > 0f)
        {
            _waveCooldownTimer -= Time.deltaTime;
            if(_waveCooldownTimer > 0f)
            {
                return;
            }
        }

        ++_currentWaveIndex;//다음 웨이브로 전환

        if(_currentWaveIndex >= _waves.Length)
        {//총 웨이브 이상인 경우: 모든 웨이브 완료
            EncounterCompleted();//인카운터 종료 함수 실행
        }
        else
        {
            SetupWaveRuntimeState();//다음 웨이브 세팅

            OnWaveStarted?.Invoke(_currentWaveIndex);
            OnEnemyAliveChanged?.Invoke(_aliveEnemies);
            OnEnemyTotalChanged?.Invoke(_totalToSpawnThisWave);
        }
    }

    /// <summary>
    /// 현재 웨이브의 Definition을 기반으로 Runtime 상태 준비
    /// </summary>
    private void SetupWaveRuntimeState()
    {
        //State 정리
        _entryStates.Clear();
        //스폰된 수량 정리
        _totalSpawnedThisWave = 0;
        _totalToSpawnThisWave = 0;

        if (_waves == null || _currentWaveIndex < 0 || _currentWaveIndex >= _waves.Length)
        {//웨이브가 없거나 인덱스가 0미만이거나 인덱스가 웨이브를 넘으면 return
            return;
        }

        WaveDefinition wave = _waves[_currentWaveIndex];
        if(wave.Enemies == null)
        {//해당 웨이브에서 에너미에 대한 정보가 없는 경우 return
            return;
        }

        float now = Time.time;//현재 시각
        for (int enemyIdx = 0; enemyIdx < wave.Enemies.Length; enemyIdx++)
        {
            WaveEnemyEntry entry = wave.Enemies[enemyIdx];
            if (entry == null || entry.SpawnPoint == null || entry.EnemyPrefab == null || entry.Count <= 0)
            {//해당 개체가 없거나, 스폰장소 설정이 안되어있거나, 프리팹이 없거나, 최대 스폰 수량이 0이하인 경우 넘어감
                continue;
            }

            //런타임 설정
            RuntimeWaveEntryState state = new();
            state.Config = entry;
            state.SpawnedCount = 0;
            state.NextSpawnTime = now;

            _entryStates.Add(state);
            _totalToSpawnThisWave += entry.Count;
        }
    }

    /// <summary>
    /// 현재 웨이브 스폰 처리
    /// </summary>
    private void UpdateWaveSpawning(WaveDefinition wave)
    {
        if(_entryStates.Count <= 0)
        {//엔트리가 없는 경우 return
            return;
        }

        if(_waveCooldownTimer > 0f)
        {
            _waveCooldownTimer -= Time.deltaTime;
            if(_waveCooldownTimer <= 0f)
            {
                _waveCooldownTimer = 0.0f;
            }
            return;
        }

        if(_aliveEnemies >= _maxAllEnemies)
        {//최대 스폰 수 이상으로 살아있는 경우 return
            return;
        }

        float now = Time.time;//현재 시각 기록
        for (int entryIdx = 0; entryIdx < _entryStates.Count; entryIdx++)
        {//한마리씩 스폰 처리
            if(_aliveEnemies >= _maxAllEnemies)
            {//최대 스폰 수 이상으로 살아있는 경우 break
                break;
            }

            RuntimeWaveEntryState state = _entryStates[entryIdx];
            if (state != null && state.SpawnedCount < state.Config.Count && now >= state.NextSpawnTime)
            {//null이거나 목표이상으로 스폰했거나 스폰시간이 덜 됐으면 패스
                GameObject spawned = state.Config.SpawnPoint.SpawnOne(this, state.Config.EnemyPrefab);
                if (spawned != null)
                {//무사 스폰이 되었을 경우
                    ++state.SpawnedCount;//state의 스폰 수량 증가
                    state.NextSpawnTime = now + state.Config.SpawnInterval;//다음 스폰시간 처리
                    ++_aliveEnemies;//생존 적 증가
                    ++_totalSpawnedThisWave;//해당 웨이브에 스폰된 수량 추가

                    OnEnemyAliveChanged?.Invoke(_aliveEnemies);
                    OnEnemyTotalChanged?.Invoke(_totalToSpawnThisWave);
                }
            }
        }
    }
}