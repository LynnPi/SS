using UnityEngine;
using System.Collections;

/// <summary>
/// 虚拟服务器
/// </summary>
public class DummyServer : MonoBehaviour {
    
    public static readonly DummyServer DummyServerInstance = new DummyServer();
    private DummyServer() {

    }
}
