using System;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class WebSocketManager : MonoBehaviour
{
    private WebSocket _websocket;
    public event EventHandler OnOpen;
    public event EventHandler OnClose;
    public event EventHandler<WebSocketEventArgs> OnMessageReceived;

    public bool IsOpen
    {
        get
        {
            return _websocket != null && _websocket.State == WebSocketState.Open;
        }
    }

    public async Task ConnectAsync(string url, EventHandler onOpen = null, EventHandler onClose = null, EventHandler<WebSocketEventArgs> onMessage = null)
    {
        _websocket = new WebSocket(url);

        _websocket.OnOpen += () =>
        {
            Debug.Log("[WebSocketService] Connection Open");
            OnOpen?.Invoke(this, EventArgs.Empty);
        };

        _websocket.OnError += (e) =>
        {
            Debug.Log($"[WebSocketService] Connection Error. {e}");
        };

        _websocket.OnClose += (e) =>
        {
            Debug.Log($"[WebSocketService] Connection Close. {e}");
            OnClose?.Invoke(this, EventArgs.Empty);
        };

        _websocket.OnMessage += HandleMessage;

        if (onOpen != null)
        {
            OnOpen += onOpen;
        }

        if (onClose != null)
        {
            OnClose += onClose;
        }

        if (onMessage != null)
        {
            OnMessageReceived += onMessage;
        }

        await _websocket.Connect();
    }

    private void Update()
    {
        if (_websocket != null)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _websocket.DispatchMessageQueue();
#endif
        }
    }

    public async Task Send(byte[] payload)
    {
        if (IsOpen)
        {
            await _websocket.Send(payload);
        }
    }

    public void Reset()
    {
        if (_websocket != null)
        {
            _websocket.OnMessage -= HandleMessage;
        }
    }

    public async Task Dispose()
    {
        if (_websocket != null)
        {
            _websocket.OnMessage -= HandleMessage;
            await _websocket.Close();
            _websocket = null;
        }
    }

    private void HandleMessage(byte[] bytes)
    {
        //First 8 bytes are the prelude with info about header lengths and total length.
        byte[] totalByteLengthBytes = new byte[4];
        Array.Copy(bytes, totalByteLengthBytes, 4);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(totalByteLengthBytes);
        }

        //an int32 is 4 bytes
        int totalByteLength = BitConverter.ToInt32(totalByteLengthBytes, 0);

        byte[] headersByteLengthBytes = new byte[4];
        Array.Copy(bytes, 4, headersByteLengthBytes, 0, 4);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(headersByteLengthBytes);
        }

        int headersByteLength = BitConverter.ToInt32(headersByteLengthBytes, 0);

        //Use the prelude to get the offset of the message.
        int offset = headersByteLength + 12;
        //Message length is everything but the headers, CRCs, and prelude.
        int payloadLength = totalByteLength - (headersByteLength + 16);
        byte[] payload = new byte[payloadLength];
        Array.Copy(bytes, offset, payload, 0, payloadLength);
        string message = System.Text.Encoding.UTF8.GetString(payload);

        //Convert the message to and object so we can easily get the results.
        if (!string.IsNullOrEmpty(message))
        {
            TranslateWebsocketMessage jsonMessage = JsonUtility.FromJson<TranslateWebsocketMessage>(message);

            // invoke event to send message to other resources
            OnMessageReceived?.Invoke(this, new WebSocketEventArgs(jsonMessage));
        }
    }
}
