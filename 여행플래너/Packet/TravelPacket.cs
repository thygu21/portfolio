using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary;

namespace Packet
{
    public enum PacketType
    {
        초기화 = 0,
        위젯,
        채널,
        날짜,
        채팅,
        이동,
        정보,
        종료
    }
    [Serializable]
    public class TravelPacket
    {
        public int Type;
        public string tts;
        public string nickname;

        public TravelPacket()
        {

        }

        public static byte[] Serialize(Object o)
        {
            MemoryStream ms = new MemoryStream(1024 * 4);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, o);
            return ms.ToArray();
        }

        public static Object Deserialize(byte[] bt)
        {
            MemoryStream ms = new MemoryStream(1024 * 4);
            foreach (byte b in bt)
            {
                ms.WriteByte(b);
            }

            ms.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            Object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;
        }
    }

    [Serializable]
    public class TravelData : TravelPacket
    {
        public int locationX;
        public int locationY;
        public int chan, day;
        public int index;
        public string text;
        public int color;
        public int btnType;
        public int widgetIndex;
        public MyAccommodation a;
        public MyRestaurant r;
        public MySite s;
        public MyVehicle v;


        public TravelData()
        {
            locationX = 0;
            locationX = 0;
        }
    }
}
