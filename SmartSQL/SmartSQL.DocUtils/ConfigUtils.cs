using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using ZetaLongPaths;

namespace SmartSQL.DocUtils
{
    /// <summary>
    /// 管理 配置的连接字符串
    /// </summary>
    public static class ConfigUtils
    {
        /// <summary>
        /// 当前应用程序的名称
        /// </summary>
        private static string ConfigFileName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName).Replace(".vshost", "");
        /// <summary>
        /// 定义配置存放的路径
        /// </summary>
        public static string AppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), ConfigFileName);

        /// <summary>
        /// sqlite db文件的存放路径
        /// </summary>
        private static string ConfigFilePath = string.Empty;

        /// <summary>
        /// 初始化静态数据
        /// 将sqlite数据库写入  C:\Users\用户名\AppData\Local\DBChm 目录中
        /// </summary>
        static ConfigUtils()
        {
            try
            {
                if (!ZlpIOHelper.DirectoryExists(AppPath))
                {
                    ZlpIOHelper.CreateDirectory(AppPath);
                }
                AddSecurityControll2Folder(AppPath);
                ConfigFilePath = Path.Combine(AppPath, ConfigFileName + ".db");
            }
            catch (Exception)
            {
                //LogManager.Error("ConfigUtils", ex);
            }
        }

        /// <summary>
        /// 判断磁盘路径下是否安装存在某个文件，最后返回存在某个文件的路径
        /// </summary>
        /// <param name="installPaths"></param>
        /// <param name="installPath"></param>
        /// <returns></returns>
        public static bool IsInstall(string[] installPaths, out string installPath)
        {
            installPath = string.Empty;
            var driInfos = DriveInfo.GetDrives();
            foreach (DriveInfo dInfo in driInfos)
            {
                if (dInfo.DriveType == DriveType.Fixed)
                {
                    foreach (string ipath in installPaths)
                    {
                        string path = Path.Combine(dInfo.Name, ipath);
                        if (File.Exists(path))
                        {
                            installPath = path;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 搜索获取软件安装目录
        /// </summary>
        /// <param name="orNames">软件名称 或 软件的主程序带exe的文件名</param>
        /// <returns>获取安装目录</returns>
        public static string SearchInstallDir(params string[] orNames)
        {
            //即时刷新注册表
            SHChangeNotify(0x8000000, 0, IntPtr.Zero, IntPtr.Zero);

            string installDir = null;

            var or_install_addrs = new List<string>
            {
                @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            var or_get_key_names = new List<string>
            {
                "InstallLocation",
                "InstallPath",
                "Install_Dir",
                "UninstallString"
            };


            Microsoft.Win32.RegistryKey regKey = null;
            try
            {
                regKey = Microsoft.Win32.Registry.LocalMachine;

                var arr_exe = orNames.Where(t => t.EndsWith(".exe")).ToList();
                var arr_name = orNames.Where(t => !t.EndsWith(".exe")).ToList();

                if (arr_exe.Any())
                {
                    foreach (var exe_name in arr_exe)
                    {
                        var name_node = regKey.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{exe_name}", false);
                        if (name_node != null)
                        {
                            var keyValue = name_node.GetValue("Path");

                            if (keyValue == null)
                            {
                                //取 (默认)
                                keyValue = name_node.GetValue("");
                            }

                            if (keyValue != null)
                            {
                                // 值 可能 带双引号，去除双引号
                                installDir = keyValue.ToString().Trim('"');
                                if (!Directory.Exists(installDir))
                                {
                                    // 可能是文件路径，取目录
                                    installDir = Path.GetDirectoryName(installDir);
                                }
                                return installDir;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var regAddr in or_install_addrs)
                    {
                        var regSubKey = regKey.OpenSubKey(regAddr, false);

                        foreach (var name in arr_name)
                        {
                            var name_node = regSubKey.OpenSubKey(name);

                            if (name_node != null)
                            {
                                foreach (var keyName in or_get_key_names)
                                {
                                    var keyValue = name_node.GetValue(keyName);

                                    if (keyValue != null)
                                    {
                                        // 值 可能 带双引号，去除双引号
                                        installDir = keyValue.ToString().Trim('"');
                                        if (!Directory.Exists(installDir))
                                        {
                                            // 可能是文件路径，取目录
                                            installDir = Path.GetDirectoryName(installDir);
                                        }
                                        return installDir;
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception)
            {
                //LogUtils.LogError(nameof(SearchInstallDir), Developer.SysDefault, ex);
            }
            finally
            {
                regKey?.Close();
            }
            return installDir;
        }


        [DllImport("shell32.dll")]

        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);



        /// <summary>
        ///为文件夹添加users，everyone用户组的完全控制权限
        /// </summary>
        /// <param name="dirPath"></param>
        public static void AddSecurityControll2Folder(string dirPath)
        {
            //获取文件夹信息
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (dir.Exists)
            {
                //获得该文件夹的所有访问权限
                System.Security.AccessControl.DirectorySecurity dirSecurity = dir.GetAccessControl(AccessControlSections.All);
                //设定文件ACL继承
                InheritanceFlags inherits = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
                //添加ereryone用户组的访问权限规则 完全控制权限
                FileSystemAccessRule everyoneFileSystemAccessRule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, inherits, PropagationFlags.None, AccessControlType.Allow);
                //添加Users用户组的访问权限规则 完全控制权限
                FileSystemAccessRule usersFileSystemAccessRule = new FileSystemAccessRule("Users", FileSystemRights.FullControl, inherits, PropagationFlags.None, AccessControlType.Allow);
                bool isModified = false;
                dirSecurity.ModifyAccessRule(AccessControlModification.Add, everyoneFileSystemAccessRule, out isModified);
                dirSecurity.ModifyAccessRule(AccessControlModification.Add, usersFileSystemAccessRule, out isModified);
                //设置访问权限
                dir.SetAccessControl(dirSecurity);
            }
        }
    }
}
