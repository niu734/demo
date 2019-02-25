using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PreCompareFile
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        static void CopyFolder()
        {
            if (Settings.Default.FolderDestPaths != null && Settings.Default.FolderDestPaths.Count > 0)
            {
                int index = 0;
                //PacsImage、OA
                foreach (string type in Settings.Default.FolderDestPaths)
                {
                    type.LogNoTime();
                    var sourcePaths = Settings.Default.FolderPaths[index].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string folderPath in sourcePaths)
                    {
                        if (!Directory.Exists(folderPath))
                        {
                            ("源路径不存在：" + folderPath).Log();
                            badDirectories.Add(folderPath);
                            continue;
                        }

                        //目标路径
                        String destPath = Settings.Default.DestPath + type;
                        CreateDirectory(destPath);

                        //获取源路径信息
                        DirectoryInfo sourceDirectory = new DirectoryInfo(folderPath);

                        Stopwatch t = new Stopwatch();
                        t.Start();
                        Console.WriteLine("正在复制文件夹：" + sourceDirectory.FullName);
                        ("正在复制文件夹：" + sourceDirectory.FullName).Log();

                        try
                        {
                            //开始复制
                            CopyDirectoryAndFile(folderPath, destPath, true);
                            Console.WriteLine("成功！");
                            string.Format("共{0}个文件，新增文件：{1}个，更新文件：{2}个。", fileCount, newFileCount, modifyFileCount).Log();
                            ("成功！用时：" + TimeTransform(t.ElapsedMilliseconds)).Log();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("失败！");
                            ("失败！").Log();
                            ex.Message.LogNoTime();
                        }
                    }
                    index++;
                }
            }
        }
    }
}
