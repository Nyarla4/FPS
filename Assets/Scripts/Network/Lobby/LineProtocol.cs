using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// 네트워크 스트림을 '한 줄(개행 \n)' 단위로 읽고 쓰는 간단 유틸.
/// - 텍스트 기반 프로토콜을 쉽게 다루기 위한 버퍼.
///
/// 사용법:
///   LineProtocol lp = new LineProtocol(stream);
///   lp.WriteLine("JOIN|Alice");
///   List<string> lines = lp.ReadAvailableLines(); // DataAvailable일 때만 호출
/// </summary>
public class LineProtocol
{
    private NetworkStream _stream;                  // TCP 네트워크 스트림
    private byte[] _readBuffer;                     // 수신 버퍼(고정 크기)
    private StringBuilder _incoming;                // 수신 중 문자열 누적 버퍼
    private Encoding _encoding;                     // 문자열 인코딩(UTF8)

    public LineProtocol(NetworkStream s)
    {
        _stream = s;
        _readBuffer = new byte[4096];//최대한 크게
        _incoming = new StringBuilder();
        _encoding = Encoding.UTF8;
    }

    /// <summary>
    /// 개행(\n)으로 끝나는 한 줄을 전송한다.
    /// </summary>
    public void WriteLine(string line)
    {
        if(_stream == null || string.IsNullOrEmpty(line))
        {//비어있으면 안함
            return;
        }

        string withNewLine = line + "\n";//���๮�� �߰�
        byte[] data = _encoding.GetBytes(withNewLine);//����Ʈ�� ����

        try
        {
            _stream.Write(data, 0, data.Length);//�۽��� ������, �������� �ε���(0: ó������), �۽��� ������ ũ��
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
    /// 스트림에 도착해 있는 모든 바이트를 읽어, 줄 단위로 반환한다.
    /// - blocking 없이 NetworkStream.DataAvailable 일 때만 호출 권장.
    ///         *Blocking : ��Ʈ��ũ �ۼ����� �� �ɶ����� ����Ű�� ��
    ///         *Non-Blocking: �ۼ����� �� ���� �ʾƵ� Ÿ �۾� ����
    /// </summary>
    public List<string> ReadAvailableLines()
    {
        List<string> result = new();

        if(_stream == null)
        {//���� ������ ������ ��� ��ȯ
            return result;
        }

        bool available = _stream.DataAvailable;
        if (!available)
        {
            return result;
        }

        try
        {
            while (_stream.DataAvailable)
            {
                int read = _stream.Read(_readBuffer, 0 , _readBuffer.Length);
                if(read <= 0)
                {
                    break;
                }

                string text = _encoding.GetString(_readBuffer, 0, read);
                _incoming.Append(text);//����ؼ� ���� ó��

                //���� ���ۿ��� �� ������ �и�
                while (true)
                {
                    string current = _incoming.ToString();
                    int idx = current.IndexOf('\n');//�� ����
                    if(idx < 0)//���̻� �ٴ����� �и� �ȵǸ� �и� ����
                    {
                        break;
                    }

                    string line = current.Substring(0, idx);
                    result.Add(line.Trim('\r'));//\r: ���๮��, Ŀ���� �� ������ �̵�
                    string rest = current.Substring(idx + 1);
                    _incoming.Clear();
                    _incoming.Append(rest);
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
