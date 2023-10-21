using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Decrypter
{
    class Program
    {

        static public FAT_FNAMES list;

        public class FAT_FNAMES
        {
            [JsonProperty("FileName")]
            public List<string> FileName { get; set; }
        }

        static void Decrypt(string path)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine(path + " 正在解密");

            string name = Path.GetFileName(path);

            List<string> paths = list.FileName.FindAll(
                delegate(string text)
                {
                    if(text.IndexOf(name) != -1)
                        return true;
                    else
                        return false;
                }
            );

            for (int i = 0; i < paths.Count; i++)
            {
                try
                {
                    // GOTCHARDFILE 秘钥:mtrT40c7lsT0VGn6ltW5Be
                    // 奥特曼融合激战 秘钥:mtrM2KR9eRJk3RH
                    SeekableAesStream aes = new SeekableAesStream(new MemoryStream(File.ReadAllBytes(path)), "mtrT40c7lsT0VGn6ltW5Be", Encoding.UTF8.GetBytes(paths[i]));

                    byte[] data = new byte[aes.Length];
                    aes.Read(data, 0, (int)aes.Length);

                    if (data[0] != 0x55 && data[1] != 0x6e && data[1] != 0x69 && data[1] != 0x74 && data[1] != 0x79)
                        throw new Exception();

                    File.WriteAllBytes(path + "-dec", data);
                    Console.WriteLine(path + " 解密成功");
                    break;
                }
                catch
                {
                    Console.WriteLine(paths[i] + " 尝试失败");
                }
            }
            Console.WriteLine("--------------------------------");

        }

        static void GetAllFileByDir(string DirPath, ref List<string> AL)
        {
            //列举出所有文件,添加到AL
            foreach (string file in Directory.GetFiles(DirPath))
                AL.Add(file);
            //列举出所有子文件夹,并对之调用GetAllFileByDir自己;
            foreach (string dir in Directory.GetDirectories(DirPath))
                GetAllFileByDir(dir, ref AL);
        }

        static void Main(string[] args)
        {

            Console.WriteLine("请输入FAT_FNAMES.json文件路径:");
            try
            {
                list = JsonConvert.DeserializeObject<FAT_FNAMES>(File.ReadAllText(Console.ReadLine()));
            }
            catch
            {
                Console.WriteLine("json错误,按任意键退出");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("请输入资源文件所在文件夹:");

            List<string> filelist = new List<string>();
            GetAllFileByDir(Console.ReadLine(), ref filelist);

            for (int i = 0; i < filelist.Count; i++)
            {
                if (filelist[i].EndsWith("-dec"))
                    continue;
                Decrypt(filelist[i]);
            }

            Console.WriteLine("解密完毕,按任意键退出");
            Console.ReadKey();
        }
    }
}
