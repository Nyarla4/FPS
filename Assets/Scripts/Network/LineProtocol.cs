using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// 네트워크 스트림을 한줄(개행 \n)단위로 읽고 쓰는 간단 유틸
///     텍스트 기반 프로토콜을 쉽게 다루기 위한 버퍼
/// 사용방법
///     LineProtocol lp = new(stream);
///     lp.WriteLine("Join | Alice");
///     List<string> lines = lp.ReadAvaliableLines();   //DataAvailable 일 때 만 호출
/// </summary>
public class LineProtocol
{
    private NetworkStream stream;//TCP 네트워크 스트림
    private byte[] readBuffer;//수신 버퍼(고정크기)
    private StringBuilder incoming;//수신 중 문자열 누적 버퍼
    private Encoding encoding;//문자열 인코딩(UTF8)

    public LineProtocol(NetworkStream s)
    {
        stream = s;
        readBuffer = new byte[4096];//최대한 크게 할당
        incoming = new StringBuilder();
        encoding = Encoding.UTF8;
    }

    /// <summary>
    /// 개행(\n)으로 끝나는 한 줄 전송
    /// </summary>
    /// <param name="line"></param>
    public void WriteLine(string line)
    {
        if(stream == null || string.IsNullOrEmpty(line))
        {//빈 문자인 경우 처리X
            return;
        }

        string withNewLine = line + "\n";//개행문자 추가
        byte[] data = encoding.GetBytes(withNewLine);//바이트로 변형

        try
        {
            stream.Write(data, 0, data.Length);//송신할 데이터, 데이터의 인덱스(0: 처음부터), 송신할 데이터 크기
        }
        catch (IOException)
        {
            //연결 끊김 가능성. 상위에서 처리
        }
        catch (ObjectDisposedException)
        {
            //스트림 이미 종료됨
        }
    }

    /// <summary>
    /// 스트림에 도착해 있는 모든 바이트를 읽어, 줄 단위로 반환
    ///     blocking없이 NetworkStream.DataAvaliabe 일때만 호출 권장
    ///         *Blocking : 네트워크 송수신이 다 될때까지 대기시키는 것
    ///         *Non-Blocking: 송수신이 다 되지 않아도 타 작업 실행
    /// </summary>
    public List<string> ReadAvailableLines()
    {
        List<string> result = new();

        if(stream == null)
        {//읽을 데이터 없으면 즉시 반환
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
                incoming.Append(text);//계속해서 누적 처리

                //누적 버퍼에서 줄 단위로 분리
                while (true)
                {
                    string current = incoming.ToString();
                    int idx = current.IndexOf('\n');//줄 단위
                    if(idx < 0)//더이상 줄단위로 분리 안되면 분리 종료
                    {
                        break;
                    }

                    string line = current.Substring(0, idx);
                    result.Add(line.Trim('\r'));//\r: 개행문자, 커서를 행 앞으로 이동
                    string rest = current.Substring(idx + 1);
                    incoming.Clear();
                    incoming.Append(rest);
                }
            }
        }
        catch (IOException)
        {
            //연결 오류 시 상위에서 정리
        }
        catch (ObjectDisposedException)
        {
            //스트림 이미 종료됨
        }

        return result;
    }
}
