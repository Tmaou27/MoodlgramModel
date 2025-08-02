using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MoodlgramModel.Model
{
    public class BuiltInMessenger
    {
        public List<Profile> friends;
        public Profile me;
        public BuiltInMessenger(string serverIp, Profile user = null)
        {
            friends = new List<Profile>();
            me = user;
            new InternetClient(serverIp);
            //new Profile("1", "2", "3", new BuiltInCripta());
            //new PirateJavaInternetClient();
        }
        public Profile GetContact(string id)
        {
            return friends.Find(x => x.ID == id);
        }
        //first of all - length

        //ccode
        //byte codes before message
        //10 - coded: coded msg, signature
        //20 - opened: profile.ToByte, sign
        public void SendMessage(Profile author, Profile addres, Message message)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter binWriter = new BinaryWriter(memStream, Encoding.UTF32, false);

            binWriter.Write((byte)10);

            byte[] signature = me.inCripta.Sign(message.ToByteArray());
            byte[] msg = message.ToByteArray();
            byte[][] temp = BuiltInCripta.GetWorkingArray(msg, addres.inCripta.N);
            //Console.WriteLine("5");
            binWriter.Write(temp.GetLength(0));
            //Console.WriteLine("6 "+ temp.GetLength(0));
            for (int i = 0; i < temp.GetLength(0); i++)
            {
                //Console.WriteLine("7 "+ temp[i].Length);
                byte[] data = addres.inCripta.Code(temp[i]);
                //Console.WriteLine("8");

                binWriter.Write(data.Length);
                binWriter.Write(data);
                //Console.WriteLine("9");

            }
            //Console.WriteLine("10");
            binWriter.Write(signature.Length);
            binWriter.Write(signature);

            InternetClient.Instance.SendMessage(addres.ID, memStream.ToArray());

            binWriter.Close();
            memStream.Close();
        }
        public void RegisterOnServer()
        {
            me.ID =
                 InternetClient
                 .Instance.RegisterOnServer
                 (me.inCripta.N,
                 me.inCripta.E);
        }
        //Ccode
        //10 new msg
        //20 hello
        public void SendHello(Profile author, string addres)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter binWriter = new BinaryWriter(memStream, Encoding.UTF32, false);

            binWriter.Write((byte)20);

            byte[] hello = me.FriendToByteArray(DateTimeOffset.Now);
            binWriter.Write(hello.Length);
            binWriter.Write(hello);

            byte[] signature = me.inCripta.Sign(hello.ToArray());

            binWriter.Write(signature.Length);
            binWriter.Write(signature);

            InternetClient.Instance.SendMessage(addres, memStream.ToArray());

            binWriter.Close();
            memStream.Close();
        }

        public byte[] GetHeap()
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memStream, Encoding.UTF32, false);

            binaryWriter.Write(DateTimeOffset.UtcNow.Ticks);
            binaryWriter.Write(me.ID);

            byte[] sign = me.inCripta.Sign
                (memStream.ToArray());
            binaryWriter.Write(sign.Length);
            binaryWriter.Write(sign);

            byte[] array = memStream.ToArray();

            memStream.Close();
            binaryWriter.Close();
            byte[] responce = InternetClient.Instance.GetHeap(array);
            return responce;
        }
        public List<Message> ReadHeap(byte[] heap)
        {
            if (heap == null)
                return null;
            MemoryStream memStream = new MemoryStream(heap);
            BinaryReader binaryReader = new BinaryReader(memStream, Encoding.UTF32);

            List<Message> messages = new List<Message>();

            //Ccode
            //10 new msg
            //20 hello

            while (memStream.Position < memStream.Length)
            {
                long savedPosition = memStream.Position + binaryReader.ReadInt32();
                try
                {
                    byte msgType = binaryReader.ReadByte();
                    switch (msgType)
                    {
                        case 10:
                            //Console.WriteLine(1);
                            int count = binaryReader.ReadInt32();
                            int l;
                            //Console.WriteLine(2);

                            List<byte> msg = new List<byte>();
                            for (int i = 0; i < count; i++)
                            {
                                //Console.WriteLine(3);

                                l = binaryReader.ReadInt32();
                                byte[] pack = binaryReader.ReadBytes(l);
                                msg.AddRange(me.inCripta.Decode(pack));
                            }
                            l = binaryReader.ReadInt32();
                            byte[] signature = binaryReader.ReadBytes(l);
                            //Console.WriteLine(4);

                            Message newMessage = Message.FactoryMethod(msg.ToArray());
                            //Console.WriteLine(5);

                            Profile sender = GetContact(newMessage.idAuthor);
                            //Console.WriteLine(6);

                            if (!sender.inCripta.TrySign(signature, newMessage.ToByteArray()))
                            {
                                //Console.WriteLine(7);

                                continue;
                            }
                            else
                            {
                                //Console.WriteLine(8);

                                messages.Add(newMessage);
                            }
                            break;
                        case 20:
                            //Console.WriteLine("1");
                            l = binaryReader.ReadInt32();

                            Profile newProfile
                                = Profile.CreateFriend(binaryReader.ReadBytes(l));
                            //Console.WriteLine("2 ", newProfile.inCripta.N, newProfile.Nickname);

                            l = binaryReader.ReadInt32();
                            //Console.WriteLine("3");
                            signature = binaryReader.ReadBytes(l);
                            //Console.WriteLine("4");

                            if (!newProfile.inCripta.TrySign(signature, newProfile.FriendToByteArray())
                                || friends.FindIndex(_ => _.ID == newProfile.ID) != -1)
                            {
                                Console.WriteLine("NO Friends with " + newProfile.Nickname);
                                continue;
                            }
                            else
                            {
                                Console.WriteLine("New Friends");
                                if (friends.FindIndex(_ => _.ID == newProfile.ID) != -1)
                                    this.SendHello(this.me, friends.Last().ID);
                                friends.Add(newProfile);
                            }
                            break;
                        default:
                            throw new Exception
                                ("В сообщении из кучи непонятный код " + msgType);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    memStream.Position = savedPosition;
                }
            }


            memStream.Close();
            binaryReader.Close();
            return messages;
        }
    }
}