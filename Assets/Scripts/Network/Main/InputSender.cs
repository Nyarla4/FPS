using UnityEngine;

/// <summary>
/// �Է� ������ �о �ֱ������� ����
/// ���� �Է�(WASD, ���콺 �ü�)�� �о� �ֱ������� ������ INPUT ����.
/// - ���� PC �ÿ�: ���(.exe) â���� ����.
/// - yaw/pitch�� ����(���� ī�޶� ����).
/// </summary>
public class InputSender : MonoBehaviour
{
    [Header("Settings")]
    public float MouseSensitivity = 3.0f;     // ���콺 ����(��/�ȼ� �ٻ�)
    public float SendRate = 20.0f;            // �ʴ� ���� ȸ��(���� tick�� ����ϰ�)
    public bool LockCursorOnStart = true;     // ���� �� Ŀ�� ���

    private float _yaw;                         // ���� ��(��)
    private float _pitch;                       // ���� ��(��)
    private float _sendAccumulator;             // ���� �ֱ� ����

    private void Start()
    {
        if (LockCursorOnStart == true)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // ���콺 �ü�
        float mdx = Input.GetAxis("Mouse X");
        float mdy = Input.GetAxis("Mouse Y");

        _yaw = _yaw + (mdx * MouseSensitivity);
        _pitch = _pitch - (mdy * MouseSensitivity); // ���콺 Y�� �ݴ�
        _pitch = Mathf.Clamp(_pitch, -80.0f, 80.0f);

        // �̵� �Է�
        float mx = Input.GetAxisRaw("Horizontal"); // A/D: -1/1
        float my = Input.GetAxisRaw("Vertical");   // W/S: -1/1

        // ���� �ֱ�
        float dt = Time.deltaTime;
        _sendAccumulator = _sendAccumulator + dt;

        float interval = 1.0f / SendRate;
        while (_sendAccumulator >= interval)
        {
            SendInput(mx, my);
            _sendAccumulator = _sendAccumulator - interval;
        }
    }

    private void SendInput(float mx, float my)
    {
        if (NetworkRunner.instance == null)
        {
            return;
        }

        int fire = 0; // ���� ����� ����
        string payload = $"{mx:F3},{my:F3},{_yaw:F1},{_pitch:F1},{fire}";

        bool isClient = NetworkRunner.instance.IsClientConnected();
        bool isServer = NetworkRunner.instance.IsServerRunning();

        // 1) �Ϲ� Ŭ���̾�Ʈ: ��Ʈ��ũ�� INPUT ����
        if (isClient == true)
        {
            string line = "INPUT|" + payload;
            NetworkRunner.instance.ClientSendLine(line);
            return;
        }

        // 2) ȣ��Ʈ ���� ���: ������ ���� ����(fromClientId = 0)
        if (isServer == true && isClient == false)
        {
            NetworkRunner.instance.ServerInjectCommand(0, "INPUT", payload);
            return;
        }

        // �� ��(����/Ŭ�� ��� �ƴ�)�� �ƹ� �͵� ���� �ʴ´�.
    }
}
