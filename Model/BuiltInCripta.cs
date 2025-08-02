using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MoodlgramModel.Model
{
    public class BuiltInCripta
    {
        public byte[] N { get { return publicKey.Modulus; } }
        public byte[] E { get { return publicKey.Exponent; } }
        public byte[] D { get { return privateKey.D; } }
        RSAParameters privateKey;
        RSAParameters publicKey;
        RSACryptoServiceProvider RSA;
        public BuiltInCripta()
        {
            RSA = new RSACryptoServiceProvider(2048);

            privateKey = RSA.ExportParameters(true);
            publicKey = RSA.ExportParameters(false);
        }
        public BuiltInCripta(byte[] d, byte[] n, byte[] e)
        {
            RSA = new RSACryptoServiceProvider();
            //
            privateKey = new RSAParameters();
            privateKey.D = d;
            privateKey.Exponent = e;
            privateKey.Modulus = n;
            RSA.ImportParameters(privateKey);
            //Пункт 2
            privateKey = RSA.ExportParameters(true);
            publicKey = RSA.ExportParameters(false);

            /*
            UnicodeEncoding byteConverter = new UnicodeEncoding();
            string toEncrypt = "Hello, world";

            Console.WriteLine($"To encode: {toEncrypt}");

            byte[] encBytes = RSA.Encrypt(byteConverter.GetBytes(toEncrypt), false);

            string encrypt = byteConverter.GetString(encBytes);
            Console.WriteLine("Encrypt str: " + encrypt);
            Console.WriteLine("Encrypt bytes: " + string.Join(", ", encBytes));

            byte[] decBytes = RSA.Decrypt(encBytes, false);

            Console.WriteLine("Decrypt str: " + byteConverter.GetString(decBytes));
            Console.WriteLine("Decrypt bytes: " + string.Join(", ", byteConverter.GetBytes(encrypt)));

            Console.ReadKey();*/
        }
        public static BuiltInCripta BuiltInCriptaFriend(byte[] n, byte[] e)
        {
            BuiltInCripta friend = new BuiltInCripta();

            friend.publicKey = new RSAParameters();
            friend.publicKey.Modulus = n;
            friend.publicKey.Exponent = e;

            friend.RSA.ImportParameters(friend.publicKey);

            return friend;
        }
        public static byte[][] GetWorkingArray(byte[] message, byte[] n)
        {
            Console.WriteLine("3");
            Int32 length = n.Length / 2;
            Int32 count = message.Length / length + 1;
            byte[][] arrays = CreateJaggedArray<byte[][]>(count, length);
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (j + length * i < message.Length)
                        arrays[i][j] = message[j + length * i];
                    else
                        arrays[i][j] = 0;
                }
            }
            Console.WriteLine("4");
            return arrays;
        }
        private static object InitializeJaggedArray(Type type, int index, int[] lengths)
        {
            Array array = Array.CreateInstance(type, lengths[index]);
            Type elementType = type.GetElementType();

            if (elementType != null)
            {
                for (int i = 0; i < lengths[index]; i++)
                {
                    array.SetValue(
                        InitializeJaggedArray(elementType, index + 1, lengths), i);
                }
            }

            return array;
        }
        private static T CreateJaggedArray<T>(params int[] lengths)
        {
            return (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        }
        public byte[] Code(byte[] array)
        {
            //return new byte[0];
            Console.WriteLine($"{N.Length}, {array.Length}");
            return RSA.Encrypt(array, true);
        }
        public byte[] Decode(byte[] array)
        {
            //return new byte[0];
            return RSA.Decrypt(array, true);
        }
        public byte[] Sign(byte[] array) 
        {
            //return new byte[0];
            return RSA.SignData(array, SHA512.Create());
        }
        public bool TrySign(byte[] sign, byte[] buffer)
        {
            //sign.ToList().ForEach(i => Console.Write(i.ToString()));
            Console.WriteLine($"{RSA.VerifyData(buffer, SHA512.Create(), sign)}");
            return RSA.VerifyData(buffer, SHA512.Create(), sign);
        }
    }
}
