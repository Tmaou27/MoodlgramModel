using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MoodlgramModel.Model
{
    public class InternetClient
    {
        private TcpClient tcpClient;
        private string ip;
        public static InternetClient Instance { get; private set; }
        public InternetClient(string ip)
        {
            Instance = this;
            this.ip = ip;
        }
        //Scode 
        //10 - get heap
        //20 - send msg
        //30 - create heap (returns heap name)
        public string RegisterOnServer(byte[] N, byte[] E)
        {
            string newID = "";
            try
            {
                
                tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Parse(ip), 8080);
                
                NetworkStream stream = tcpClient.GetStream();
                                
                stream.Write(new byte[1] { 30 }, 0, 1);
                
                stream.Write(BitConverter.GetBytes(N.Length), 0, 4);
                stream.Write(N, 0, N.Length);
                stream.Write(BitConverter.GetBytes(E.Length), 0, 4);
                stream.Write(E, 0, E.Length);

                byte[] buffer = new byte[4];
                stream.Read(buffer, 0, buffer.Length);
                int l = BitConverter.ToInt32(buffer, 0);
                buffer = new byte[l];

                stream.Read(buffer, 0, buffer.Length);
                newID = Encoding.UTF32.GetString(buffer);

                Console.WriteLine("Reg Done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                tcpClient.Close();
            }
            return newID;
        }
        //Scode 
        //10 - get heap
        //20 - send msg
        //30 - create heap
        public void SendMessage(string idAddress, byte[] message)
        {
            try
            {

                tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Parse(ip), 8080);
                NetworkStream stream = tcpClient.GetStream();
                stream.Write(new byte[1] { 20 }, 0, 1);
                byte[] id_bytes = Encoding.UTF32.GetBytes(idAddress);                
                stream.Write(BitConverter.GetBytes( id_bytes.Length), 0, 4);
                stream.Write(id_bytes, 0, id_bytes.Length);
                stream.Write(BitConverter.GetBytes(message.Length), 0, 4);
                stream.Write(message, 0, message.Length);
                Console.WriteLine("Sent");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                tcpClient.Close();
            }
        }

        //Scode 
        //10 - get heap
        //20 - send msg
        //30 - create heap

        //message = time, id, sign
        public byte[] GetHeap(byte[] message)
        {
            byte[] response = null;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Parse(ip), 8080);
                NetworkStream stream = tcpClient.GetStream();

                stream.Write(new byte[]{ (byte)10 }, 0, 1);
                stream.Write(BitConverter.GetBytes(message.Length), 0, 4);
                stream.Write(message, 0, message.Length);

                byte[] buffer = new byte[4];
                stream.Read(buffer, 0, buffer.Length);
                int l = BitConverter.ToInt32(buffer, 0);
                response = new byte[l];
                stream.Read(response, 0, response.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                tcpClient.Close();
            }
            return response;
        }
        
        //Client - server, write file
        //Client - server, read file
        //Client - server - client
    }
}
