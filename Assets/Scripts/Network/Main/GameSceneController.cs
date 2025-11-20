using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game_Main �� ���� ��� ��Ʈ�ѷ�(����/Ŭ�� ����).
/// - ����: ServerGame ���� -> �κ� ������ ���� -> STATE ��� ����.
/// - Ŭ��: ClientGame ���� -> �ƹ�Ÿ �θ�/������ ���� -> STATE ���� ����.
/// - ���� PC������ Ȯ���� AvatarsRoot/AvatarPrefab�� �غ�ǵ��� ��Ʈ��Ʈ���� å������.
/// </summary>
public class GameSceneController : MonoBehaviour
{
    [Header("Optional Refs (�� �� ����� �ڵ� ������)")]
    public Transform AvatarsRoot;            // �ƹ�Ÿ �θ�(������ �ڵ� ����)
    public GameObject AvatarPrefab;          // �ƹ�Ÿ ������(������ ��Ÿ�� ĸ�� ����)

    [Header("Server Settings")]
    public Transform[] SpawnPoints;          // ���� ���� ������
    public float ServerTickRate = 20.0f;     // ���� ƽ �ֱ�
    public float ServerMoveSpeed = 4.5f;     // �̵� �ӵ�

    private void Start()
    {
        // 1) ��Ʈ��ũ ���� Ȯ��
        if (NetworkRunner.instance == null)
        {
            Debug.LogWarning("GameSceneController: NetworkRunner not found. Did you keep it with DontDestroyOnLoad?");
            return;
        }

        bool isServer = NetworkRunner.instance.IsServerRunning();
        bool isClient = NetworkRunner.instance.IsClientConnected();

        // 2) �ƹ�Ÿ �θ� Ȯ��: ��� ������ ���� ����� (�� ��ȯ ���Ŀ��� 100% �غ�)
        if (AvatarsRoot == null)
        {
            GameObject rootGO = new GameObject("AvatarsRoot");
            AvatarsRoot = rootGO.transform;
        }

        // 3) �ƹ�Ÿ ������ Ȯ��: ��� ������ ��Ÿ�ӿ� ĸ�� �������� �Ｎ ������ ����
        //    (������ ������ ��� �����ϵ���)
        GameObject runtimeAvatarPrefab = AvatarPrefab;
        if (runtimeAvatarPrefab == null)
        {
            runtimeAvatarPrefab = CreateRuntimeCapsulePrefab();
        }

        // 4) ���� ����: �ߺ� �߰� ���� �� ServerGame ����
        if (isServer == true)
        {
            ServerGame sg = gameObject.GetComponent<ServerGame>();
            if (sg == null)
            {
                sg = gameObject.AddComponent<ServerGame>();
            }

            sg.SpawnPoints = SpawnPoints;
            sg.TickRate = ServerTickRate;
            sg.MoveSpeed = ServerMoveSpeed;

            // �κ� ������ id ����� ���ʿ��� ������ ����� ���� ��Ȯ��
            // (�ӽ� ��ƿ ���, ���ʿ� ������ getter�� �ִٸ� �װ� ���)
            //List<int> ids = NetworkUtil.GetCurrentPlayerIds(); // �ӽ� ��� ��� ���̸� ���� ��ü
            List<int> ids = NetworkRunner.instance.GetCurrentPlayerIdsSnapshot();
            sg.SpawnPlayerInitial(ids);
        }

        // 5) Ŭ�� ǥ�� ����: ����/Ŭ�� ��� STATE�� ���� ������ �� �� ClientGame ����
        if (isClient == true || isServer == true)
        {
            ClientGame cg = gameObject.GetComponent<ClientGame>();
            if (cg == null)
            {
                cg = gameObject.AddComponent<ClientGame>();
            }

            cg.AvatarsRoot = AvatarsRoot;          // ����� ���� ����
            cg.AvatarPrefab = runtimeAvatarPrefab;  // ����� ������ ����
            cg.InitAvatarRoot();

            // ���� �Է� ������(Ŭ�� ���ۿ�)
            InputSender input = gameObject.GetComponent<InputSender>();
            if (input == null)
            {
                input = gameObject.AddComponent<InputSender>();
            }
            input.MouseSensitivity = 3.0f;
            input.SendRate = 20.0f;
            input.LockCursorOnStart = true;
        }
    }

    /// <summary>
    /// ��Ÿ�ӿ� ����� "�ӽ� �ƹ�Ÿ ������"�� �ڵ�� �����Ѵ�.
    /// - Capsule + �̸�/���̾� �⺻��.
    /// - ������ ������ ��� ���� ������ ���� �뵵.
    /// </summary>
    private GameObject CreateRuntimeCapsulePrefab()
    {
        // �� �θ� GO�� ���� ��, �� �Ʒ��� ĸ���� �ٿ��� �ϳ��� ������ó�� ���
        GameObject root = new GameObject("AvatarPrefab_RuntimeCapsule");

        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "Body";
        capsule.transform.SetParent(root.transform, false);
        capsule.transform.localPosition = new Vector3(0.0f, 1.0f, 0.0f);

        // �ʿ��ϸ� ��Ƽ����/����/�������� ������ ���� ����(�ʼ��� �ƴ�)

        // ������ó�� Instantiate�ؼ� ����ϹǷ� ��Ȱ���� �ʿ� ����
        return root;
    }
}
