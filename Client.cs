using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ChatClient
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        string serverIp = "127.0.0.1";
        int port = 8080;

        TcpClient client = new TcpClient();
        client.Connect(serverIp, port);
        Console.WriteLine("Đã kết nối đến server!");

        NetworkStream stream = client.GetStream();

        // Thread nhận tin nhắn
        Thread receiveThread = new Thread(() => ReceiveMessages(stream));
        receiveThread.Start();

        // Gửi yêu cầu hoặc tin nhắn
        while (true)
        {
            string input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) continue;

            if (input.ToUpper().StartsWith("FILE"))
            {
                // Gửi yêu cầu file
                byte[] data = Encoding.UTF8.GetBytes("FILE:");
                stream.Write(data, 0, data.Length);

                // Nhận file và lưu trữ
                byte[] buffer = new byte[1024];
                string directoryPath = "D:\\DocCument\\Lap Trinh Mang";
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string savePath = Path.Combine(directoryPath, "received_file.txt");
                using (FileStream fs = new FileStream(savePath, FileMode.Create))
                {
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                        if (bytesRead < buffer.Length) break; // Nếu hết dữ liệu, thoát vòng lặp
                    }
                }
                Console.WriteLine($"File đã được lưu tại: {savePath}");
            }
            else
            {
                // Gửi tin nhắn
                byte[] data = Encoding.UTF8.GetBytes(input);
                stream.Write(data, 0, data.Length);
            }
        }
    }

    static void ReceiveMessages(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        int bytesRead;

        while (true)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Tin nhắn mới: {message}");
            }
            catch
            {
                break;
            }
        }

        Console.WriteLine("Ngắt kết nối từ server.");
    }
}
