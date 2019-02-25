using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Copy.Properties;

namespace Copy
{
    public static class EString
    {
        public static void Detail(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.detailFile);
                sw2.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + msg);
                sw2.Flush();
                sw2.Close();
            }
        }

        public static void DetailNoTime(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.detailFile);
                sw2.WriteLine(msg);
                sw2.Flush();
                sw2.Close();
            }
        }

        public static void Folder新增(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.newFile);
                sw2.WriteLine(msg);
                sw2.Flush();
                sw2.Close();
            }
        }
        public static void Folder大小(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.sizeFile);
                sw2.WriteLine(msg);
                sw2.Flush();
                sw2.Close();
            }
        }
        public static void Folder时间(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.timeFile);
                sw2.WriteLine(msg);
                sw2.Flush();
                sw2.Close();
            }
        }
        public static void Folder时间大小(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.bothFile);
                sw2.WriteLine(msg);
                sw2.Flush();
                sw2.Close();
            }
        }
        public static void Folder跳过(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.skipFile);
                sw2.WriteLine(msg);
                sw2.Flush();
                sw2.Close();
            }
        }

        //创建一个文件，若存在则会清空里面的数据
        public static void Create(this string msg, string fileName)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.CreateText(fileName);
                sw2.Write(msg);
                sw2.Flush();
                sw2.Close();
            }
        }

        public static void Error(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.errorFile);
                sw2.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss："));
                sw2.WriteLine(msg);
                sw2.Flush();
                sw2.Close();

                try
                {
                    var sw = File.AppendText(Program.dic["DestPath"] + Program.dic["Type"] + "\\" + Program.errorFile);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + msg);
                    sw.Flush();
                    sw.Close();
                }
                catch (Exception ex)
                {
                    ("远程Error日志写入报错\n" + ex.Message).Local();
                }
            }
        }


        public static void Local(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.logFile);
                sw2.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + msg);
                sw2.Flush();
                sw2.Close();
            }
        }
        public static void LocalNoTime(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.logFile);
                sw2.WriteLine(msg);
                sw2.Flush();
                sw2.Close();
            }
        }

        public static void Log(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {

                var sw = File.AppendText(Program.dic["DestPath"] + Program.dic["Type"] + "\\" + Program.logFile);
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + msg);
                sw.Flush();
                sw.Close();

                try
                {
                    var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.logFile);
                    sw2.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + msg);
                    sw2.Flush();
                    sw2.Close();
                }
                catch (Exception ex)
                {
                    ("远程CopyLog日志写入报错\n" + ex.Message).Local();
                }
            }
        }

        public static void LogNoTime(this string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                try
                {
                    var sw = File.AppendText(Program.dic["DestPath"] + Program.dic["Type"] + "\\" + Program.logFile);
                    sw.WriteLine(msg);
                    sw.Flush();
                    sw.Close();
                }
                catch (Exception ex)
                {
                    ("远程CopyLog日志写入报错\n" + ex.Message).Local();
                }

                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.logFile);
                sw2.WriteLine(msg);
                sw2.Flush();
                sw2.Close();
            }
            else
            {
                try
                {
                    var sw = File.AppendText(Program.dic["DestPath"] + Program.dic["Type"] + "\\" + Program.logFile);
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                }
                catch (Exception ex)
                {
                    ("远程CopyLog日志写入报错\n" + ex.Message).Local();
                }

                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.logFile);
                sw2.WriteLine();
                sw2.Flush();
                sw2.Close();
            }
        }

        public static void Success(this string msg)
        {
            var sw = File.AppendText(Program.dic["DestPath"] + Program.dic["Type"] + "\\" + Program.successFile);
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + msg);
            sw.Flush();
            sw.Close();

            try
            {
                var sw2 = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + Program.successFile);
                sw2.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss：") + msg);
                sw2.Flush();
                sw2.Close();
            }
            catch (Exception ex)
            {
                ("远程Success日志写入报错\n" + ex.Message).Local();
            }
        }
    }
}
