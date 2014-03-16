using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using LibGit2Sharp;

namespace Mygod.Skylark.Deployer
{
    public static class Program
    {
        private static void ReadKey()
        {
            Console.Write("(按任意键继续)");
            Console.ReadKey(true);
            Console.WriteLine();
        }
        private static char GetChar()
        {
            var ch = Console.ReadKey(true).KeyChar;
            Console.WriteLine();
            return ch;
        }

        private static string Api(string address, string data = null, string token = null, string method = "POST")
        {
            var request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = method;
            request.Accept = "application/json";
            request.ContentType = "application/x-www-form-urlencoded";
            if (token != null) request.Headers["Authorization"] = "BEARER " + token;
            if (data != null)
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                request.ContentLength = bytes.LongLength;
                using (var stream = request.GetRequestStream()) stream.Write(bytes, 0, bytes.Length);
            }
            using (var stream = request.GetResponse().GetResponseStream()) return new StreamReader(stream).ReadToEnd();
        }

        public static void Main()
        {
            var assemblyName = Assembly.GetEntryAssembly().GetName();
            Console.Title = assemblyName.Name + " V" + assemblyName.Version;
            Console.OutputEncoding = Encoding.Unicode;
            Console.Write("需要提示吗？(Y/N)");
            var hint = Console.ReadKey(true).Key != ConsoleKey.N;
            Console.WriteLine();
            if (!hint) goto step1;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("NPC: 欢迎使用{0}！你以为这会是一个很难用的程序，事实上你会发现这是个坑爹的游戏。",
                              Console.Title);
            Console.WriteLine("NPC: 您将要部署属于您自己的 云雀™，不过不必担心！Mygod 工作室™ 深深地知道如今广大用户都喜欢做任务拿经验，因此要部署 云雀™ 只需完成简简单单的一个任务即可！");
            Console.Write("NPC: 准备好做你的任务了吧？快按下任意键啊你个混蛋等死我了。");
            ReadKey();
            Console.WriteLine("NPC: 你这家伙反应真慢，看我说话比你快多了。");
            Console.WriteLine("NPC: 好吧我们来做你的任务吧。");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("获得任务: 登录/注册 AppHarbor 并授权");
            Console.WriteLine("任务详情: 登录/注册 AppHarbor 并给一个叫 NPC 的神秘人物授权。在这个过程中你可能会遇到一些十分简单的英语，不必恐慌。");
            Console.WriteLine("任务奖励：经验值 x 42, 金币 x 314, 您崭新的 云雀™ x 1");
            var eUnlocked = false;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("请选择动作：\n  A 接受任务\n  B “经验值有毛用？”\n  C “金币又有毛用？”\n  D “授权？好可怕，会有什么后果？”");
                if (eUnlocked) Console.Write("\n  E “邪恶的代码？听起来好恐怖，那是啥？”");
                Console.ForegroundColor = ConsoleColor.Yellow;
                switch (char.ToLower(GetChar()))
                {
                    case 'a':
                        goto step1;
                    case 'b':
                        Console.WriteLine("NPC: 大概可以升到 Lv. 2 吧？谁知道呢。");
                        break;
                    case 'c':
                        Console.WriteLine("NPC: 或许可以买些增强功能包？天知道。");
                        break;
                    case 'd':
                        Console.WriteLine("NPC: Mygod 工作室™ 将使用您的账号进行以下不可见人的操作，您的信息将会在完成任务后销毁：\n· 获得您账户的 token；\n· 创建新的应用；\n· 部署我们邪恶的代码；\n· ???\n· PROFIT!");
                        eUnlocked = true;
                        break;
                    case 'e':
                        if (!eUnlocked) goto default;
                        Process.Start("https://github.com/Mygod/Skylark/");
                        break;
                    default:
                        Console.WriteLine("NPC: 嗯？");
                        break;
                }
            }

        step1:
            Console.ForegroundColor = ConsoleColor.Green;
            Process.Start("https://appharbor.com/user/authorizations/new?client_id=84ddc329-6a2a-48f9-bfb7-d9c27ff09dd4&redirect_uri=http%3A%2F%2Fmygod.tk%2Fskylark%2Foauth%2F");
            Console.Write("NPC: 请将你获得的认证代码粘贴到这里：（如果你不会粘贴，试试右击窗口标题，编辑，粘贴）");
            var code = Console.ReadLine();
            string result = Api("https://appharbor.com/tokens", "client_id=84ddc329-6a2a-48f9-bfb7-d9c27ff09dd4&client_secret=404fda0f-5e4f-466d-bdab-02d388e9728f&code=" + code), token = HttpUtility.ParseQueryString(result)["access_token"];
            if (string.IsNullOrEmpty(token))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("AppHarbor: " + result);
                Console.WriteLine("NPC: 不好意思，目测出了什么问题，请重试。");
                ReadKey();
                goto step1;
            }
            var match = Regex.Match(result = Api("https://appharbor.com/user", token: token, method: "GET"),
                                    "\"username\": \"(.*?)\"");
            if (!match.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("AppHarbor: " + result);
                Console.WriteLine("NPC: 不好意思，目测出了什么问题，请重试。");
                ReadKey();
                goto step1;
            }
            Console.ForegroundColor = ConsoleColor.White;
            if (hint)
            {
                Console.WriteLine("[系统消息] 欢迎用户 {0} 成功登入", match.Groups[1].Value);
                Console.WriteLine("[系统消息] 经验值 + 42");
                Console.WriteLine("[系统消息] 你已经升级到 Lv. 2");
                Console.WriteLine("[系统消息] 你已经升级到 Lv. 3");
                Console.WriteLine("[系统消息] 你已经升级到 Lv. 4");
                Console.WriteLine("[系统消息] 你已经升级到 Lv. 5");
                Console.WriteLine("[系统消息] 金币 + 314");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("NPC: 勇敢的年轻人你干得很好。为了贯彻党中央的法令，我们需要实名认证所有的玩家并将它们列入防沉迷系统。因此请认真地对待下面的内容。");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine("说明：域名将为 {去空格后的名称}.apphb.com 后缀，如“Skylark Test”。如果你输入了一个已经存在的域名，它将会被覆盖。");
            Console.Write("请选择你的 云雀™ 名称：");
            var name = Console.ReadLine();
            string location;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("请选择你的 云雀™ 出生地：0 美国东部 1 欧洲西部");
            while (true) switch (GetChar())
                {
                    case '0':
                        location = "us-east-1";
                        goto step2;
                    case '1':
                        location = "eu-west-1";
                        goto step2;
                }
        step2:
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("NPC: 干得不错，最后一步，请输入你的密码以便我们为您的 App 推送我们邪恶的代码：");
            var password = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        goto step3;
                    case ConsoleKey.Backspace:
                        password.Remove(password.Length - 1, 1);
                        if (Console.CursorLeft == 0)
                        {
                            Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
                            Console.Write(' ');
                            Console.SetCursorPosition(Console.BufferWidth - 1, Console.CursorTop - 1);
                        }
                        else Console.Write("\b \b");
                        break;
                    default:
                        password.Append(key.KeyChar);
                        Console.Write("*");
                        break;
                }
            }
        step3:
            Api("https://appharbor.com/applications",
                string.Format("name={0}&region_identifier=amazon-web-services::{1}", name, location), token);
            name = name.ToLower().Replace(" ", string.Empty);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("请在您应用程序的配置界面中点击 ENABLE FILE SYSTEM WRITE ACCESS。");
            Process.Start(string.Format("https://appharbor.com/applications/{0}/edit", name));
            ReadKey();
            Console.Write("请在您应用程序的配置界面中点击 DISABLE PRECOMPILATION。");
            Process.Start(string.Format("https://appharbor.com/applications/{0}/edit", name));
            ReadKey();
            Console.ForegroundColor = ConsoleColor.Blue;
            //if (Directory.Exists("Content")) Directory.Delete("Content", true);
            //Directory.CreateDirectory("Content");
            //ZipFile.Open("Skylark.zip", ZipArchiveMode.Read).ExtractToDirectory("Content");
            using (var repo = new Repository(Repository.Init("Content")))
            {
                repo.Index.Stage(Directory.EnumerateFiles("Content", "*", SearchOption.AllDirectories)
                                          .Select(path => path.Substring(8))
                                          .Where(path => !path.StartsWith(".git", true, CultureInfo.InvariantCulture)),
                    new ExplicitPathsOptions { OnUnmatchedPath = msg => Console.WriteLine("git add: {0}", msg) });
                repo.Commit("Skylark Deployer Commit");
                var remoteName = "appharbor";
                if (repo.Network.Remotes[remoteName] != null)
                {
                    var i = 0;
                    while (repo.Network.Remotes[remoteName + i] != null) i++;
                    remoteName += i;
                }
                var remote = repo.Network.Remotes.Add(remoteName,
                                    string.Format("https://{0}@appharbor.com/{1}.git", match.Groups[1].Value, name));
                repo.Network.Push(remote,
                                  remote.RefSpecs.Select(refSpec => refSpec.Specification.Replace("/*", "/master")),
                                  new PushOptions
                {
                    Credentials = new Credentials { Username = match.Groups[1].Value, Password = password.ToString() },
                    OnPackBuilderProgress = (stage, current, total) =>
                    {
                        Console.WriteLine("git push Pack Builder Progress: {0}/{1}, Stage: {2}",
                                          current, total, stage);
                        return true;
                    },
                    OnPushStatusError = errors => Console.WriteLine("git push Status Error: {0} ({1})",
                                                                    errors.Message, errors.Reference),
                    OnPushTransferProgress = (current, total, bytes) =>
                    {
                        Console.WriteLine("git push Transfer Progress: {0}/{1}, Bytes: {2}", current, total, bytes);
                        return true;
                    }
                });
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("NPC: 完成。请静等一分钟，你的 云雀™ 即将准备就绪。");
            Thread.Sleep(60000);
            Process.Start(string.Format("http://{0}.apphb.com/Update/", name));
            Console.Write("NPC: 更新完成后按任意键启动你的 云雀™。");
            ReadKey();
            Process.Start(string.Format("http://{0}.apphb.com/View/readme.htm", name));
        }
    }
}
