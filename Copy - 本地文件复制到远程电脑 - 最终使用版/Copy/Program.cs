using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Copy.Properties;
using System.Diagnostics;
using System.Threading;
using System.Xml.Serialization;

namespace Copy
{
    class Program
    {
        //出错个数
        static int errorNum = 0;
        //成功个数
        static int successNum = 0;

        //net use 【IP】 【密码】 /USER:【用户】
        static string loginCommand = "net use \"{0}\" {2} /USER:{1}";

        //路径不存在
        static List<string> badDirectories = new List<string>();

        static string baseDestPath = "";

        public static string now = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss");
        public static string logFile     = string.Format("Z{0} CopyLog.txt",       now);
        public static string detailFile  = string.Format("Z{0} Detail.txt",        now);
        public static string errorFile   = string.Format("Z{0} Error______________________________.txt", now);
        public static string successFile = string.Format("Z{0} Success______.txt", now);
        public static string newFile     = string.Format("Z{0} Folder新增.txt",    now);
        public static string sizeFile    = string.Format("Z{0} Folder大小.txt",    now);
        public static string timeFile    = string.Format("Z{0} Folder时间.txt",    now);
        public static string bothFile    = string.Format("Z{0} Folder时间大小.txt",now);
        public static string skipFile    = string.Format("Z{0} Folder跳过.txt",    now);

        public static Dictionary<string, string> dic = new Dictionary<string, string>();

        public static string DestFolder = "";

        public static int PacsImageDays = 60;

        static string CopyType = "";

        static void Main(string[] args)
        {
            try
            {
                //读取配置
                var properites = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "properties.txt");
                foreach (var p in properites)
                {
                    if (!string.IsNullOrEmpty(p))
                    {
                        var temp = p.Split('：');
                        if (temp != null && temp.Length == 2)
                        {
                            dic[temp[0]] = temp[1];
                        }
                    }
                }

                CopyType = dic["Type"];

                DestFolder = CopyType;
                if (dic.ContainsKey("CreateDateFolder") && "true".Equals(dic["CreateDateFolder"]))
                {
                    DestFolder = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss");
                }

                int temp2;
                if (dic.ContainsKey("PacsImageDays") && Int32.TryParse(dic["PacsImageDays"], out temp2))
                {
                    PacsImageDays = temp2;
                }

                //连接远程电脑
                string user = "123";
                string pwd = "123";
                if (dic.ContainsKey("user"))
                {
                    user = dic["user"];
                }
                if (dic.ContainsKey("pwd"))
                {
                    pwd = dic["pwd"];
                }
                String cmd = string.Format(loginCommand, dic["ConnPath"], user, pwd);
                "---------------------------------".LocalNoTime();
                "net use".LocalNoTime();
                RunCmd(cmd);


                //创建目标-根目录
                baseDestPath = dic["DestPath"] + DestFolder + "\\";
                CreateDirectory(baseDestPath);

                Stopwatch all = new Stopwatch();
                all.Start();
                "---------------------------------".LogNoTime();
                DateTime.Now.ToString("yyyy-MM-dd").LogNoTime();
                "复制开始".Log();

                if (dic.ContainsKey("Folder") && "true".Equals(dic["Folder"]))
                {
                    CopyFolder();
                }
                else
                {
                    CopyFile();
                }

                Console.WriteLine("-------------------------");
                Console.WriteLine("成功个数：" + successNum);
                Console.WriteLine("失败个数：" + errorNum);
                "复制结束".Log();
                "-------------------------".LogNoTime();
                ("总用时：" + TimeTransform(all.ElapsedMilliseconds)).LogNoTime();
                ("成功个数：" + successNum).LogNoTime();
                ("失败个数：" + errorNum).LogNoTime();
                if (badDirectories.Count > 0)
                {
                    ("无法访问的路径：" + badDirectories.Count).LogNoTime();
                    foreach (string badDirectory in badDirectories)
                    {
                        badDirectory.LogNoTime();
                    }
                }
                string.Empty.Success();
                
                //检查日志文件
                "-------------------------".LogNoTime();
                CheckLogFile();
            }
            catch (Exception ex)
            {
                ("Main：未知异常\n" + ex.Message).Error();
            }
        }


        static void RunCmd(string cmd)
        {
            try
            {
                using (Process p = new Process())
                {
                    ("执行命令：" + cmd).Local();
                    p.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.StandardInput.WriteLine(cmd);
                    p.StandardInput.WriteLine("exit");
                    p.StandardInput.AutoFlush = true;
                    //string result = p.StandardOutput.ReadToEnd();
                    //result.Local();
                    //p.WaitForExit();
                    while (!p.StandardOutput.EndOfStream)
                    {
                        p.StandardOutput.ReadLine().Local();
                    }
                    p.Close();
                }
            }
            catch (Exception ex)
            {
                ("RunCmd：执行CMD命令出错\n" + cmd + "\n" + ex.Message).Error();
            }
        }

        static void Copy(String sourceFileName, String destPath)
        {
            try
            {
                FileInfo sourceFile = new FileInfo(sourceFileName);
                //目标文件名
                String destFileName = destPath + sourceFile.Name;
                //目标文件名不存在，则复制
                if (!File.Exists(destFileName))
                {
                    Stopwatch t = new Stopwatch();
                    t.Start();
                    Console.WriteLine("正在复制文件：" + sourceFileName);
                    ("正在复制文件：" + sourceFileName).Log();

                    //目标路径不存在则创建7852
                    if (!Directory.Exists(destPath))
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    //复制文件，不允许覆盖同名文件
                    File.Copy(sourceFileName, destFileName, false);

                    if (!"PL-Log".Equals(CopyType) && sourceFileName.EndsWith(".log"))
                    {
                        var lines = File.ReadAllLines(sourceFileName);
                        var length = lines.Length;
                        lines[length - 4].LogNoTime();
                        lines[length - 3].LogNoTime();
                        lines[length - 2].LogNoTime();
                    }

                    successNum++;
                    Console.WriteLine("成功！");
                    ("成功！用时：" + TimeTransform(t.ElapsedMilliseconds)).Log();
                    
                }
                else
                {
                    Console.WriteLine("文件已存在：" + sourceFileName);
                    ("文件已存在：" + sourceFileName).Log();

                    ("文件已存在：" + sourceFileName).Error();
                }
            }
            catch (Exception ex)
            {
                errorNum++;
                Console.WriteLine("失败！");
                ("失败！").Log();
                ex.Message.LogNoTime();

                ("Copy：复制文件失败\n" + sourceFileName + "\n" + destPath + "\n" + ex.Message).Error();
            }
        }

        
        static void CheckLogFile()
        {
            try
            {
                ("可保留日志数：" + dic["LogFileCount"]).Log();
                int maxLogCount = Convert.ToInt32(dic["LogFileCount"]);
                string deleteDateTime = "Z" + DateTime.Now.AddDays(-maxLogCount).ToString("yyyy-MM-dd");

                //本地
                var localLogList = GetDeleteFiles(AppDomain.CurrentDomain.BaseDirectory, deleteDateTime);
                foreach (FileInfo localLog in localLogList)
                {
                    ("删除本地日志：" + localLog.Name).Log();
                    localLog.Delete();
                }

                //远程电脑
                var remoteLogList = GetDeleteFiles(baseDestPath, deleteDateTime);
                foreach (FileInfo remoteLog in remoteLogList)
                {
                    ("删除远程日志：" + remoteLog.Name).Log();
                    remoteLog.Delete();
                }
                
            }
            catch (Exception ex)
            {
                ("CheckLogFile：删除日志文件报错\n" + ex.Message).Error();
            }
        }

        static string TimeTransform(long ms)
        {
            if (ms == 0)
            {
                return "";
            }
            if (ms < 60000)
            {
                return ms / 1000.0 + "秒";
            }
            if (ms < 3600000)
            {
                return ms / 60000 + "分" + TimeTransform(ms % 60000);
            }
            return ms / 3600000 + "小时" + TimeTransform(ms % 3600000);
        }

        static void DeleteFiles(string destPath)
        {
            //父路径存在：his
            if (Directory.Exists(destPath))
            {
                string[] subPaths = Directory.GetDirectories(destPath);
                int maxBakCount = Convert.ToInt32(dic["MaxBakCount"]);
                if (subPaths != null && subPaths.Length >= maxBakCount)
                {
                    var list = subPaths.OrderBy(p => p).ToList();
                    for (int i = 0; i <= list.Count - maxBakCount; i++)
                    {
                        try
                        {
                            ("删除过期备份：" + list[i]).Log();
                            Directory.Delete(list[i], true);
                            "成功！".Log();
                        }
                        catch (Exception ex)
                        {
                            "失败！".Log();
                            ex.Message.LogNoTime();

                            ("DeleteFiles：删除过期备份失败\n" + ex.Message).Error();
                        }
                    }
                }
            }
        }

        static List<string> GetLatestFile(string type, string sourcePath)
        {
            //源路径存在：保存数据库备份的文件夹
            if (Directory.Exists(sourcePath))
            {
                string[] files = Directory.GetFiles(sourcePath);
                if ((files != null) && (files.Length > 0))
                {
                    if ("pacs".Equals(type))
                    {
                        string today = DateTime.Now.ToString("yyyy_MM_dd");
                        var orderedFiles = files.Where(p => p.Contains(today)).OrderBy(p => p).ToList();

                        List<string> result = new List<string>();
                        if (orderedFiles.Count > 1)
                        {
                            var prevFile = orderedFiles[0].Substring(0, orderedFiles[0].IndexOf(today));
                            result.Add(orderedFiles[0]);
                            for (int i = 1; i < orderedFiles.Count; i++)
                            {
                                var currentFile = orderedFiles[i];
                                if (!currentFile.StartsWith(prevFile))
                                {
                                    prevFile = currentFile.Substring(0, currentFile.IndexOf(today));
                                    result.Add(currentFile);
                                }
                            }
                        }
                        return result;
                    }
                    else if ("lis".Equals(type))
                    {
                        string fileName = files.Where(p => !p.Contains(".log")).OrderByDescending(p => p).FirstOrDefault();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            return new List<string> { fileName };
                        }
                    }
                    else if ("PL-Log".Equals(type))
                    {
                        string zuotian = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                        string fileName = files.Where(p => p.Contains(zuotian)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            return new List<string> { fileName };
                        }
                    }
                    else
                    {
                        return files.ToList();
                    }
                }
            }
            return null;
        }

        static int fileCount = 0;
        static int newFileCount = 0;
        static List<string> newFileList = new List<string>();
        static int lastModifyFileCount = 0;
        static List<string> lastModifyFileList = new List<string>();
        static int lengthModifyFileCount = 0;
        static List<string> lengthModifyFileList = new List<string>();
        static int bothModifyFileCount = 0;
        static List<string> bothModifyFileList = new List<string>();
        static List<string> skipFolderList = new List<string>();

        static void CopyDirectoryAndFile(string sourcePath, string destPath, bool createRoot)
        {
            //获取源路径信息
            DirectoryInfo srcDirectory = new DirectoryInfo(sourcePath);
            //第一次调用会走这里
            if (createRoot)
            {
                fileCount = 0;
                newFileCount = 0;
                lastModifyFileCount = 0;
                lengthModifyFileCount = 0;
                bothModifyFileCount = 0;
                newFileList.Clear();
                lastModifyFileList.Clear();
                lengthModifyFileList.Clear();
                bothModifyFileList.Clear();
                skipFolderList.Clear();
                destPath += "\\" + srcDirectory.Name;
                CreateDirectory(destPath);
            }
            var files = srcDirectory.GetFiles();
            if (files != null && files.Length > 0)
            {
                var destfiles = (new DirectoryInfo(destPath)).GetFiles();
                foreach (var file in files)
                {
                    fileCount++;
                    string destFileName = destPath + "\\" + file.Name;
                    var destFile = destfiles.FirstOrDefault(p => p.FullName.Equals(destFileName));
                    string.Format("进度：{0}", fileCount).Detail();
                    //目标文件已存在
                    if (destFile != null /*File.Exists(destFileName)*/)
                    {
                        //(string.Format("文件已存在({0})：", fileCount) + destFileName).Detail();
                        //比较文件
                        if (modify /*dic.ContainsKey("Modify") && "true".Equals(dic["Modify"])*/)
                        {
                            bool lastModify = false;
                            //FileInfo destFile = new FileInfo(destFileName);
                            //修改时间不一样
                            if (!file.LastWriteTime.Equals(destFile.LastWriteTime))
                            {
                                lastModify = true;
                                File.Copy(file.FullName, destFileName, true);
                            }
                            //文件大小不一样
                            if (!file.Length.Equals(destFile.Length))
                            {
                                //修改时间不一样
                                if (lastModify)
                                {
                                    //"修改时间和文件大小都不一样".Detail();
                                    bothModifyFileCount++;
                                    bothModifyFileList.Add(file.FullName);
                                }
                                //修改时间一样
                                else
                                {
                                    //"文件大小不一样".Detail();
                                    lengthModifyFileCount++;
                                    lengthModifyFileList.Add(file.FullName);
                                    File.Copy(file.FullName, destFileName, true);
                                }
                            }
                            //文件大小不变
                            else
                            {
                                //修改时间变了
                                if (lastModify)
                                {
                                    //"修改时间不一样".Detail();
                                    lastModifyFileCount++;
                                    lastModifyFileList.Add(file.FullName);
                                }
                            }
                        }
                    }
                    else
                    {
                        newFileCount++;
                        newFileList.Add(file.FullName);
                        File.Copy(file.FullName, destFileName);
                    }
                }
            }
            //复制sourcePath下的文件夹
            var subDirectories = srcDirectory.GetDirectories();
            if (subDirectories != null && subDirectories.Length > 0)
            {
                List<DirectoryInfo> youXiaoMuLu = new List<DirectoryInfo>();

                if ("PacsImage".Equals(CopyType) &&
                    "US".Equals(srcDirectory.Name))
                {
                    var jiangXuMuLu = subDirectories.OrderByDescending(p => p.Name);
                    DateTime tempDate;
                    if (first)
                    {
                        youXiaoMuLu.AddRange(subDirectories);
                    }
                    else if (queryold)
                    {
                        bool buzaipanduan = false;
                        foreach (var mulu in jiangXuMuLu)
                        {
                            if (!buzaipanduan)
                            {
                                if (DateTime.TryParse(mulu.Name, out tempDate) &&
                                    tempDate >= pacsdate)
                                {
                                    skipFolderList.Add(mulu.FullName);
                                }
                                else
                                {
                                    buzaipanduan = true;
                                    youXiaoMuLu.Add(mulu);
                                }
                            }
                            else
                            {
                                youXiaoMuLu.Add(mulu);
                            }
                        }
                    }
                    else
                    {
                        bool buzaipanduan = false;
                        foreach (var mulu in jiangXuMuLu)
                        {
                            if (!buzaipanduan)
                            {
                                if (DateTime.TryParse(mulu.Name, out tempDate) &&
                                    tempDate < pacsdate)
                                {
                                    buzaipanduan = true;
                                    skipFolderList.Add(mulu.FullName);
                                }
                                else
                                {
                                    youXiaoMuLu.Add(mulu);
                                }
                            }
                            else
                            {
                                skipFolderList.Add(mulu.FullName);
                            }
                        }
                    }
                }
                else
                {
                    youXiaoMuLu.AddRange(subDirectories);
                }

                foreach (var subDirectory in youXiaoMuLu)
                {
                    var tempDestPath = destPath + "\\" + subDirectory.Name;
                    CreateDirectory(tempDestPath);
                    CopyDirectoryAndFile(subDirectory.FullName, tempDestPath, false);

                    #region 作废代码
                    /* 
                    if (first)
                    {
                        var tempDestPath = destPath + "\\" + subDirectory.Name;
                        CreateDirectory(tempDestPath);
                        CopyDirectoryAndFile(subDirectory.FullName, tempDestPath, false);

                    }
                    else if (queryold)
                    {
                        DateTime tempDate;
                        if ("PacsImage".Equals(CopyType) &&
                            "US".Equals(subDirectory.Parent.Name) &&
                            DateTime.TryParse(subDirectory.Name, out tempDate) && tempDate >= pacsdate)
                        {
                            skipFolderList.Add(subDirectory.FullName);
                        }
                        else
                        {
                            var tempDestPath = destPath + "\\" + subDirectory.Name;
                            CreateDirectory(tempDestPath);
                            CopyDirectoryAndFile(subDirectory.FullName, tempDestPath, false);
                        }
                    }
                    else
                    {
                        DateTime tempDate;
                        if ("PacsImage".Equals(CopyType) &&
                            "US".Equals(subDirectory.Parent.Name) &&
                            DateTime.TryParse(subDirectory.Name, out tempDate) && tempDate < pacsdate)
                        {
                            skipFolderList.Add(subDirectory.FullName);
                        }
                        else
                        {
                            var tempDestPath = destPath + "\\" + subDirectory.Name;
                            CreateDirectory(tempDestPath);
                            CopyDirectoryAndFile(subDirectory.FullName, tempDestPath, false);
                        }
                    }
                    */
                    #endregion
                }
            }
        }


        static void CreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                ("CreateDirectory：创建路径出错\n" + path + "\n" + ex.Message).Error();
            }
        }

        static List<FileInfo> GetDeleteFiles(string path, string start)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            var files = info.GetFiles("*.txt");
            if (files != null)
            {
                return files.Where(p => p.Name.StartsWith("Z") && p.Name.Substring(0, 11).CompareTo(start) <= 0 && !p.Name.Contains("Error")).OrderBy(p => p.Name).ToList();
            }
            return new List<FileInfo>();
        }


        //static List<FileInfo> GetFiles(string path, string keyName)
        //{
        //    DirectoryInfo info = new DirectoryInfo(path);
        //    var files = info.GetFiles("*" + keyName + "*.txt");
        //    if (files != null)
        //    {
        //        return files.OrderByDescending(p => p.Name).ToList();
        //    }
        //    return new List<FileInfo>();
        //}

        static void CopyFile()
        {
            //源路径
            var sourcePaths = dic["SourcePath"].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (sourcePaths != null && sourcePaths.Length > 0)
            {
                CopyType.LogNoTime();
                ("源路径：" + sourcePaths.Length).LogNoTime();
                foreach (String sourcePath in sourcePaths)
                {
                    if (!Directory.Exists(sourcePath))
                    {
                        ("源路径不存在：" + sourcePath).Log();
                        ("源路径不存在：" + sourcePath).Error();
                        badDirectories.Add(sourcePath);
                        continue;
                    }

                    //目标路径
                    String destPath = baseDestPath + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + "\\";
                    CreateDirectory(destPath);

                    //删除过期的备份
                    DeleteFiles(baseDestPath);

                    List<string> sourceFileNames = GetLatestFile(CopyType, sourcePath);
                    if ((sourceFileNames != null) && (sourceFileNames.Count > 0))
                    {
                        foreach (string sourceFileName in sourceFileNames)
                        {
                            Copy(sourceFileName, destPath);
                        }
                    }

                }
            }
        }

        //private void serialize()
        //{
        //    string savepath = baseDestPath + "save\\" ;
        //    CreateDirectory(savepath);
        //    XmlSerializer serializer = new XmlSerializer(typeof(SaveInfo));
        //    FileStream file = new FileStream(baseDestPath + string.Format("sql{0}.xml", DateTime.Now.ToString("yyyyMMddHHmmss")), FileMode.OpenOrCreate);
        //    serializer.Serialize(file, sqlList);
        //    file.Close();
        //}

        static bool modify = false;
        static bool first = false;
        static bool queryold = false;

        static DateTime pacsdate = DateTime.Now;

        static void CopyFolder()
        {
            //PacsImage、OA
            //string type = dic["Type"];
            CopyType.LogNoTime();
            //目标路径
            String destPath = dic["DestPath"] + DestFolder;
            CreateDirectory(destPath);

            modify = (dic.ContainsKey("Modify") && "true".Equals(dic["Modify"]));
            first = (dic.ContainsKey("First") && "true".Equals(dic["First"]));
            queryold = (dic.ContainsKey("QueryOld") && "true".Equals(dic["QueryOld"]));

            pacsdate = DateTime.Now.Date.AddDays(-PacsImageDays);

            //源路径
            var sourcePaths = dic["SourcePath"].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string folderPath in sourcePaths)
            {
                if (!Directory.Exists(folderPath))
                {
                    ("源路径不存在：" + folderPath).Log();
                    ("源路径不存在：" + folderPath).Error();
                    badDirectories.Add(folderPath);
                    continue;
                }

                //获取源路径信息
                DirectoryInfo sourceDirectory = new DirectoryInfo(folderPath);

                SaveInfo save = new SaveInfo();

                Stopwatch t = new Stopwatch();
                t.Start();
                Console.WriteLine("正在复制文件夹：" + sourceDirectory.FullName);
                "---------------------------------".LocalNoTime();
                ("正在复制文件夹：" + sourceDirectory.FullName).Log();

                try
                {
                    //开始复制
                    CopyDirectoryAndFile(folderPath, destPath, true);

                    //日志
                    Console.WriteLine("成功！");
                    ("文件总数：" + fileCount).LogNoTime();
                    save.WenJianShu = fileCount;
                    //新增文件
                    save.XinZeng = newFileCount;
                    if (newFileCount > 0)
                    {
                        ("新增文件：" + newFileCount).LogNoTime();
                    }
                    else
                    {
                        ("新增文件：【无】").LogNoTime();
                    }
                    //更新文件
                    int updateCount = lastModifyFileCount + lengthModifyFileCount + bothModifyFileCount;
                    save.GengXin = updateCount;
                    if (updateCount > 0)
                    {
                        ("更新文件：" + updateCount).LogNoTime();

                        //修改时间不一样
                        save.ShiJian = lastModifyFileCount;
                        if (lastModifyFileCount > 0)
                        {
                            ("修改时间不一样：" + lastModifyFileCount).LogNoTime();
                        }
                        //文件大小不一样
                        save.DaXiao = lengthModifyFileCount;
                        if (lengthModifyFileCount > 0)
                        {
                            ("文件大小不一样：" + lengthModifyFileCount).LogNoTime();
                        }
                        //修改时间和文件大小都不一样
                        save.Both = bothModifyFileCount;
                        if (bothModifyFileCount > 0)
                        {
                            ("修改时间和文件大小都不一样：" + bothModifyFileCount).LogNoTime();
                        }
                    }
                    else
                    {
                        ("更新文件：【无】").LogNoTime();
                    }
                    //string.Format("文件总数：{0}\r\n" +
                    //              "新增文件：{1}\r\n" +
                    //              "更新文件：{5}\r\n" +
                    //              "修改时间不一样：{2}\r\n" + 
                    //              "文件大小不一样：{3}\r\n" +
                    //              "修改时间和文件大小都不一样：{4}",
                    //              fileCount,
                    //              newFileCount,
                    //              lastModifyFileCount,
                    //              lengthModifyFileCount,
                    //              bothModifyFileCount,
                    //              lastModifyFileCount + lengthModifyFileCount + bothModifyFileCount).LogNoTime();
                    save.YongShi = TimeTransform(t.ElapsedMilliseconds);
                    ("成功！用时：" + save.YongShi).Log();
                    save.WanChengShiJian = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    if (newFileList.Count > 0)
                    {
                        "新增：".DetailNoTime();
                        foreach (var file in newFileList)
                        {
                            file.DetailNoTime();
                            file.Folder新增();
                        }
                    }
                    if (lengthModifyFileList.Count > 0)
                    {
                        "文件大小：".DetailNoTime();
                        foreach (var file in lengthModifyFileList)
                        {
                            file.DetailNoTime();
                            file.Folder大小();
                        }
                    }
                    if (lastModifyFileList.Count > 0)
                    {
                        "修改时间：".DetailNoTime();
                        foreach (var file in lastModifyFileList)
                        {
                            file.DetailNoTime();
                            file.Folder时间();
                        }
                    }
                    if (bothModifyFileList.Count > 0)
                    {
                        "文件大小和修改时间：".DetailNoTime();
                        foreach (var file in bothModifyFileList)
                        {
                            file.DetailNoTime();
                            file.Folder时间大小();
                        }
                    }
                    if (skipFolderList.Count > 0)
                    {
                        "跳过的文件夹：".DetailNoTime();
                        foreach (var folder in skipFolderList)
                        {
                            folder.DetailNoTime();
                            folder.Folder跳过();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("失败！");
                    ("失败！").Log();
                    ex.Message.LogNoTime();

                    ("CopyFolder：复制文件夹失败\n" + ex.Message).Error();
                }
            }
        }
    }
}
