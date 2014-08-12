using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using SevenZip;

namespace Mygod.Skylark.Updater
{
    public static class Program
    {
        private struct Entry
        {
            public Entry(string fileName, int index)
            {
                FileName = fileName;
                Index = index;
            }

            public readonly string FileName;
            public readonly int Index;
        }

        private static void Main(string[] args)
        {
            var id = args[0];
            using (var log = new StreamWriter("Update\\" + id + ".log", true) { AutoFlush = true })
                try
                {
                    log.WriteLine("[{0}] 更新开始。下载更新包中……", DateTime.UtcNow);
                    var zipPath = "Update\\" + id + ".zip";
                    new WebClient().DownloadFile("https://github.com/Mygod/Skylark/archive/master.zip", zipPath);
                    log.WriteLine("[{0}] 下载完成。解压中……", DateTime.UtcNow);
                    var extractor = new SevenZipExtractor(zipPath, InArchiveFormat.Zip);
                    var i = -1;
                    var list = new LinkedList<Entry>();
                    foreach (var file in extractor.ArchiveFileData)
                    {
                        i++;
                        if (file.IsDirectory || file.FileName == null || file.FileName.Length <= 15) continue;
                        var path = file.FileName.Substring(15);
                        if (string.IsNullOrWhiteSpace(path) || id != "deploy"
                            && (path.StartsWith("Files\\", true, CultureInfo.InvariantCulture)
                            || path.StartsWith("Data\\", true, CultureInfo.InvariantCulture))) continue;
                        var dir = Path.GetDirectoryName(path);
                        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                        list.AddLast(new Entry(path, i));
                    }
                    var retries = 5;
                    while (list.Count > 0 && retries-- > 0)
                    {
                        var current = list.First;
                        while (current != null)
                        {
                            try
                            {
                                using (var stream = new FileStream(current.Value.FileName, FileMode.Create,
                                    FileAccess.Write, FileShare.Read))
                                    extractor.ExtractFile(current.Value.Index, stream);
                                var temp = current;
                                current = current.Next;
                                list.Remove(temp);
                            }
                            catch (Exception exc)
                            {
                                log.WriteLine("[{0}] 解压文件失败：{1}{2}错误详细信息：{3}", DateTime.UtcNow,
                                    current.Value.FileName, Environment.NewLine, exc.GetMessage());
                                current = current.Next;
                            }
                        }
                        if (list.Count <= 0) continue;
                        if (retries == 0) log.WriteLine("[{0}] 重试次数过多，果断弃坑。");
                        else
                        {
                            log.WriteLine("[{0}] 将于 5 秒后重试。", DateTime.UtcNow);
                            Thread.Sleep(5000);
                        }
                    }
                    log.WriteLine("[{0}] 更新配置文件中……", DateTime.UtcNow);
                    foreach (var config in new[] { "Web.config", @"bin\Skylark.dll.config" })
                        File.WriteAllText(config, File.ReadAllText(config)
                            .Replace("targetFramework=\"4.0\"",
                                     "targetFramework=\"4.0\" tempDirectory=\"" + Path.GetTempPath() + '"'));
                    log.WriteLine("[{0}] 更新完成。", DateTime.UtcNow);
                }
                catch (Exception exc)
                {
                    log.WriteLine("[{0}] 更新失败，出现错误：{1}", DateTime.UtcNow, exc.GetMessage());
                }
        }

        /// <summary>
        /// 用于将错误转化为可读的字符串。
        /// </summary>
        /// <param name="e">错误。</param>
        /// <returns>错误字符串。</returns>
        private static string GetMessage(this Exception e)
        {
            var result = new StringBuilder();
            GetMessage(e, result);
            return result.ToString();
        }

        private static void GetMessage(Exception e, StringBuilder result)
        {
            while (e != null && !(e is AggregateException))
            {
                result.AppendFormat("({0}) {1}{2}{3}{2}", e.GetType(), e.Message, Environment.NewLine, e.StackTrace);
                e = e.InnerException;
            }
            var ae = e as AggregateException;
            if (ae != null) foreach (var ex in ae.InnerExceptions) GetMessage(ex, result);
        }
    }
}
