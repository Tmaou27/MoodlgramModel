using MoodlgramModel.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MoodlgramModel
{
    public class Profile
    {
        public string Nickname { get; private set; }
        public string Bio { get; private set; }
        public string ID { get; set; }
        public DateTimeOffset FrendshipStarted { get; private set; }

        public BuiltInCripta inCripta { get; private set; }
        public Profile() { }
        public Profile(string id, string nick, string bio, DateTimeOffset startFriendship, BuiltInCripta cripta)
        {
            ID = id;
            Nickname = nick;
            Bio = bio;
            inCripta = cripta;
            FrendshipStarted = startFriendship;
        }

        public Profile(string id, string nick, string bio, BuiltInCripta cripta)
        {
            ID = id;
            Nickname = nick;
            Bio = bio;
            inCripta = cripta;
        }
        public static Profile CreateFriend (string id, string nick, string bio,
            DateTimeOffset startFriendship, BuiltInCripta cripta)
        {
            BuiltInCripta friendCripta = BuiltInCripta.BuiltInCriptaFriend(cripta.N, cripta.E);
            return new Profile(id, nick, bio, friendCripta);
        }
        public static Profile CreateFriend(byte[] array)
        {
            MemoryStream memStream = new MemoryStream(array);
            BinaryReader binaryReader = new BinaryReader(memStream, Encoding.UTF32, false);

            Profile friend = new Profile();
            friend.ID = binaryReader.ReadString();
            friend.Nickname = binaryReader.ReadString();
            friend.Bio = binaryReader.ReadString();
            friend.FrendshipStarted =
                new DateTimeOffset(binaryReader.ReadInt64(),
                new TimeSpan(binaryReader.ReadInt64()));


            int ln = binaryReader.ReadInt32();
            byte[] n = binaryReader.ReadBytes(ln);
            int le = binaryReader.ReadInt32();
            byte[] e = binaryReader.ReadBytes(le);
            friend.inCripta = BuiltInCripta.BuiltInCriptaFriend(n, e);

            memStream.Close();
            binaryReader.Close();  

            return friend;
        }
        public byte[] FriendToByteArray()
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter binWriter = new BinaryWriter(memStream, Encoding.UTF32, false);

            binWriter.Write(ID);
            binWriter.Write(Nickname);
            binWriter.Write(Bio);
            binWriter.Write(FrendshipStarted.Ticks);
            binWriter.Write(FrendshipStarted.Offset.Ticks);

            binWriter.Write(inCripta.N.Length);
            binWriter.Write(inCripta.N);
            binWriter.Write(inCripta.E.Length);
            binWriter.Write(inCripta.E);

            byte[] array = memStream.ToArray();

            binWriter.Close();
            memStream.Close();

            return array;
        }
        public byte[] FriendToByteArray(DateTimeOffset startFriendship)
        {
            this.FrendshipStarted = startFriendship;
            byte[] result = this.FriendToByteArray();
            this.FrendshipStarted = DateTimeOffset.MinValue;
            return result;
        }
    }
}
