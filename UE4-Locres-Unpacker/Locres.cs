using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UE4_Locres_Unpacker
{
    static class Locres
    {
        public static void Unpack(string locresFile)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(locresFile));
            if (reader.ReadInt32() != 0x7574140E) throw new Exception("Unsuported format!");
            reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
            bool endBytes = Convert.ToInt32(reader.ReadSByte()) > 1;
            int strOffset = reader.ReadInt32();
            reader.BaseStream.Seek(strOffset, SeekOrigin.Begin);
            int strNums = reader.ReadInt32();
            List<string> strings = new List<string>();
            for (int i = 0; i < strNums; i++)
            {
                int strLen = reader.ReadInt32();
                Encoding encoding = Encoding.UTF8;
                int zeroLen = 1;
                if (strLen < 0)
                {
                    strLen = Math.Abs(strLen) * 2;
                    encoding = Encoding.Unicode;
                    zeroLen = 2;                  
                }
                string str = encoding.GetString(reader.ReadBytes(strLen - zeroLen))
                    .Replace("\r\n", "{EOL}")
                    .Replace("\n", "{LF}")
                    .Replace("\r", "{CR}");
                reader.BaseStream.Position += zeroLen;
                strings.Add(str);
                if (endBytes) reader.BaseStream.Position += 4;
            }
            string txtFile = Path.Combine(Path.GetDirectoryName(locresFile), $"{Path.GetFileNameWithoutExtension(locresFile)}.txt");
            File.WriteAllLines(txtFile, strings.ToArray());
            reader.Close();
        }

        public static void Repack(string txtFile)
        {
            string locresFile = Path.Combine(Path.GetDirectoryName(txtFile), $"{Path.GetFileNameWithoutExtension(txtFile)}.locres");
            string newLocresFile = Path.Combine(Path.GetDirectoryName(txtFile), $"{Path.GetFileNameWithoutExtension(txtFile)}-new.locres");
            string[] strings = File.ReadAllLines(txtFile);
            BinaryReader reader = new BinaryReader(File.OpenRead(locresFile));
            if (File.Exists(newLocresFile)) File.Delete(newLocresFile);
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(newLocresFile));
            if (reader.ReadInt32() != 0x7574140E) throw new Exception("Unsuported format!");
            reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
            bool endBytes = Convert.ToInt32(reader.ReadSByte()) > 1;
            int strOffset = reader.ReadInt32();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            writer.Write(reader.ReadBytes(strOffset));
            int strNums = reader.ReadInt32();
            writer.Write(strNums);
            for (int i = 0; i < strNums; i++)
            {
                string str = strings[i].Replace("{EOL}", "\r\n").Replace("{CR}", "\r").Replace("{LF}", "\n");
                byte[] strBytes = Encoding.Unicode.GetBytes(str);
                writer.Write((strBytes.Length + 2) / -2);
                writer.Write(strBytes);
                writer.Write(new byte[2]);
                int strLen = reader.ReadInt32();
                if (strLen < 0) strLen = Math.Abs(strLen) * 2;
                reader.BaseStream.Position += strLen;
                if (endBytes) writer.Write(reader.ReadBytes(4));
            }
            reader.Close();
            writer.Close();
        }
    }
}
