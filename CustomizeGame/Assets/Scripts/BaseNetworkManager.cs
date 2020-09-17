using Grpc.Core;
using UnityEngine;

public class BaseNetworkManager : MonoBehaviour
{
    public static BaseNetworkManager instance;

    private Channel channel;

    [SerializeField] private string host;
    [SerializeField] private string port;

    public BaseNetworkManager() {
        instance = null;
        host = "localhost";
        port = "12345";
    }

    private void Awake()
    {
        //Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    public void SetConnectionInfo(string host, string port) {
        this.host = host;
        this.port = port;
    }
    public Channel GetChannel()
    {
        return channel;
    }

    public Channel ConnectChannel()
    {
        string target = $"{host}:{port}";
        Debug.Log("接続先 " + target);

        if (channel != null)
        {
            DisconnectChannel();
        }
        
        channel = new Channel(target, ChannelCredentials.Insecure);
        return channel;
    }

    public async void DisconnectChannel() {
        if (channel != null)
        {
            await channel.ShutdownAsync();
            channel = null;
        }
    }

    void OnDestroy()
    {
        DisconnectChannel();
    }
}