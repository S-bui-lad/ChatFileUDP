using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ChatServer
{
    private static List<TcpClient> clients = new List<TcpClient>();
    private static object lockObject = new object();

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        int port = 8080;
        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();
        Console.WriteLine("Server đang chạy...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            lock (lockObject)
            {
                clients.Add(client);
            }
            Console.WriteLine("Client mới đã kết nối!");
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        while (true)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Nhận yêu cầu: {request}");

                if (request.StartsWith("FILE:"))
                {
                    string filePath = "C:\\sonnn.txt"; // Đường dẫn file cố định
                    if (File.Exists(filePath))
                    {
                        byte[] fileData = File.ReadAllBytes(filePath);
                        stream.Write(fileData, 0, fileData.Length);
                        Console.WriteLine("Đã gửi file cho client.");
                    }
                    else
                    {
                        string error = "File không tồn tại.";
                        byte[] errorData = Encoding.UTF8.GetBytes(error);
                        stream.Write(errorData, 0, errorData.Length);
                    }
                }
                else
                {
                    BroadcastMessage(request, client); // Chuyển tiếp tin nhắn đến các client khác
                }
            }
            catch
            {
                break;
            }
        }

        lock (lockObject)
        {
            clients.Remove(client);
        }
        Console.WriteLine("Client đã ngắt kết nối.");
        client.Close();
    }

    static void BroadcastMessage(string message, TcpClient senderClient)
    {
        lock (lockObject)
        {
            foreach (var client in clients)
            {
                if (client == senderClient) continue;

                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
