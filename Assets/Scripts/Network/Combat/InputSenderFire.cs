using UnityEngine;

/// <summary>
/// ��Ŭ�� �� ������ FIRE ��� �۽�
///     ������ sim.yaw/pitch�� ���� => �߰� ������ ����
///     �߻� ����(����/�ݵ�/����ũ)�� ���ÿ��� ��� ���
/// </summary>
public class InputSenderFire : MonoBehaviour
{
    public float LocalFireCooldown = 0.08f;//���� �ǵ�� ��ٿ�(���� ��ٿ�� �����ϰ� �� ��)
    [Header("Optional")]
    public MuzzleFlash MuzzleFlash;//���� �÷��� ���� ������Ʈ
    public CameraRecoil CameraRecoil;//ī�޶� �ݵ� ������Ʈ
    public ScreenShake ScreenShake;//ȭ�� ���� ������Ʈ

    private float _lastLocalFireTime;//������ ���� �߻� �ð�(���� �ߺ� ������)

    void Update()
    {
        bool pressed = Input.GetMouseButtonDown(0);//��Ŭ�� 1ȸ
        if (pressed)
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        float now = Time.time;
        if (now < _lastLocalFireTime + LocalFireCooldown)
        {
            //��Ÿ�� ���������� �߻� ����
            return;
        }
        _lastLocalFireTime = now;

        //������ FIRE ����
        if (NetworkRunner.instance != null)
        {
            NetworkRunner runner = NetworkRunner.instance;
            bool isClient = runner.IsClientConnected();
            bool isServer = runner.IsServerRunning();

            if (isClient)
            {
                runner.ClientSendLine("FIRE|");
            }
            else if (isServer)
            {
                //ȣ��Ʈ����(Ŭ���̾�Ʈ�� �ƴ� ���� �ܵ� �׽�Ʈ�� ���)
                runner.ServerInjectCommand(0, "FIRE", "");
            }
        }

        //���� ���� ó��
        if (MuzzleFlash != null)
        {
            MuzzleFlash.PlayOnce();
        }
        if (CameraRecoil != null)
        {
            CameraRecoil.Kick(2.2f, 0.6f);//�ݵ� ���� ����
        }
        if (ScreenShake != null)
        {
            ScreenShake.ShakeOnce(0.08f, .012f);
        }
    }
}