using UnityEngine;
using ProtoBuf;
using NetMQ;
using NetMQ.Sockets;

public class PosResetter : MonoBehaviour
{
    NetMQContext context = null;
    SubscriberSocket subSocket = null;
    double distance = 0.0;
    
    void Start ()
    {
        Debug.Log("Starting up NetMQ interface");
        context = NetMQContext.Create();
        subSocket = context.CreateSubscriberSocket();
        subSocket.Connect("tcp://127.0.0.1:21744");
        subSocket.Subscribe("EasyErgsocket");
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 100, 50), "Distance: " + distance.ToString("0.00") + "m");
    }

    void Update()
    {
        var message = new Msg();
        message.InitEmpty();
        if (subSocket.TryReceive(ref message, System.TimeSpan.Zero))
        {
            //do not try to deserialize if this is just the envelope... Our data is in the "more"
            if(message.HasMore)
            {
                return;
            }

            byte[] rawMessage = message.Data;
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(rawMessage))
            {
                var newErg = Serializer.Deserialize<EasyErgsocket.Erg>(memoryStream);
                distance = newErg.distance;
                Debug.Log("Distance: " + newErg.distance);
            }
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Shutting down...");
        subSocket.Close();
        context.Terminate();
        Debug.Log("Done...");
    }
}
