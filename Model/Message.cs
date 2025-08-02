using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MoodlgramModel
{
    public class Message
    {
        public DateTimeOffset date;
        public string idAuthor;
        public string idAdress;
        public enum ContentType
        {
            Text = 0,
            Picture =1
        }
        public ContentType contentType;
        public byte[] content;

        public Message(DateTimeOffset date, string idAuthor,
            string idAddres, ContentType contentType, byte[] content)
        {
            this.date = date;
            this.idAuthor = idAuthor;
            this.idAdress = idAddres;
            this.contentType = contentType;
            this.content = content;
        }
        public static Message FactoryMethod(byte[] array)
        {
            MemoryStream memStream = new MemoryStream(array);
            BinaryReader br = new BinaryReader(memStream, Encoding.UTF32, false);

            DateTimeOffset date =
                new DateTimeOffset(br.ReadInt64(), new TimeSpan(br.ReadInt64()));

            string idAuthor = br.ReadString();
            string idAdress = br.ReadString();

            ContentType contentType = (ContentType)br.ReadInt32();
            
            int contentLength = br.ReadInt32();
            byte[] content = br.ReadBytes(contentLength);//new byte[contentLength];
            Message message = 
                new Message(date, idAuthor, idAdress,
                contentType, content);

            br.Close();
            memStream.Close();

            return message;
        }
        public byte[] ToByteArray()
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter binWriter = new BinaryWriter(memStream, Encoding.UTF32, false);

            binWriter.Write(date.Ticks);
            binWriter.Write(date.Offset.Ticks);
            binWriter.Write(idAuthor);
            binWriter.Write(idAdress);
            binWriter.Write((Int32)contentType);
            binWriter.Write(content.Length);
            binWriter.Write(content);

            byte[] array = memStream.ToArray();

            binWriter.Close();
            memStream.Close();

            return array;
        }
    }
}