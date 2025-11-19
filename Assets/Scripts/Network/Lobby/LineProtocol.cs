using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// ��Ʈ��ũ ��Ʈ���� ����(���� \n)������ �а� ���� ���� ��ƿ
///     �ؽ�Ʈ ��� ���������� ���� �ٷ�� ���� ����
/// �����
///     LineProtocol lp = new(stream);
///     lp.WriteLine("Join | Alice");
///     List<string> lines = lp.ReadAvaliableLines();   //DataAvailable �� �� �� ȣ��
/// </summary>
public class LineProtocol
{
    private NetworkStream stream;//TCP ��Ʈ��ũ ��Ʈ��
    private byte[] readBuffer;//���� ����(����ũ��)
    private StringBuilder incoming;//���� �� ���ڿ� ���� ����
    private Encoding encoding;//���ڿ� ���ڵ�(UTF8)

    public LineProtocol(NetworkStream s)
    {
        stream = s;
        readBuffer = new byte[4096];//�ִ��� ũ�� �Ҵ�
        incoming = new StringBuilder();
        encoding = Encoding.UTF8;
    }

    /// <summary>
    /// ����(\n)���� ������ �� �� ����
    /// </summary>
    /// <param name="line"></param>
    public void WriteLine(string line)
    {
        if(stream == null || string.IsNullOrEmpty(line))
        {//�� ������ ��� ó��X
            return;
        }

        string withNewLine = line + "\n";//���๮�� �߰�
        byte[] data = encoding.GetBytes(withNewLine);//����Ʈ�� ����

        try
        {
            stream.Write(data, 0, data.Length);//�۽��� ������, �������� �ε���(0: ó������), �۽��� ������ ũ��
        }
        catch (IOException)
        {
            //���� ���� ���ɼ�. �������� ó��
        }
        catch (ObjectDisposedException)
        {
            //��Ʈ�� �̹� �����
        }
    }

    /// <summary>
    /// ��Ʈ���� ������ �ִ� ��� ����Ʈ�� �о�, �� ������ ��ȯ
    ///     blocking���� NetworkStream.DataAvaliabe �϶��� ȣ�� ����
    ///         *Blocking : ��Ʈ��ũ �ۼ����� �� �ɶ����� ����Ű�� ��
    ///         *Non-Blocking: �ۼ����� �� ���� �ʾƵ� Ÿ �۾� ����
    /// </summary>
    public List<string> ReadAvailableLines()
    {
        List<string> result = new();

        if(stream == null)
        {//���� ������ ������ ��� ��ȯ
            return result;
        }

        bool available = stream.DataAvailable;
        if (!available)
        {
            return result;
        }

        try
        {
            while (stream.DataAvailable)
            {
                int read = stream.Read(readBuffer, 0 , readBuffer.Length);
                if(read <= 0)
                {
                    break;
                }

                string text = encoding.GetString(readBuffer, 0, read);
                incoming.Append(text);//����ؼ� ���� ó��

                //���� ���ۿ��� �� ������ �и�
                while (true)
                {
                    string current = incoming.ToString();
                    int idx = current.IndexOf('\n');//�� ����
                    if(idx < 0)//���̻� �ٴ����� �и� �ȵǸ� �и� ����
                    {
                        break;
                    }

                    string line = current.Substring(0, idx);
                    result.Add(line.Trim('\r'));//\r: ���๮��, Ŀ���� �� ������ �̵�
                    string rest = current.Substring(idx + 1);
                    incoming.Clear();
                    incoming.Append(rest);
                }
            }
        }
        catch (IOException)
        {
            //���� ���� �� �������� ����
        }
        catch (ObjectDisposedException)
        {
            //��Ʈ�� �̹� �����
        }

        return result;
    }
}
