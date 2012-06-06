using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace process_control
{
    class Program
    {
        static void Main(string[] args)
        {
            /*获取进程信息*/
            //创建Win32_Process WMI类
            var mgrcls = new ManagementClass("Win32_Process");
            //枚举所有实例
            int numProc = 0;
            Console.WriteLine("-------------------------运行中的进程---------------------------------");
            foreach (ManagementObject mobj in mgrcls.GetInstances())
            {
                //准备参数，长度是2
                object[] pas = new object[2];
                mobj.InvokeMethod("GetOwner", pas);
                Console.WriteLine("{0}", mobj["name"]);
                numProc ++;
                //Console.WriteLine("{0} {1}\\{2}", mobj["name"], pas[1], pas[0]);
            }
            Console.WriteLine("---------进程数：{0}", numProc);
            Console.WriteLine("--------------------------任意键继续----------------------------------");
            Console.ReadKey(true);

            Console.WriteLine("-------------------------对应每个进程的大小---------------------------");
            ManagementClass cimobject3 = new ManagementClass("Win32_PerfFormattedData_PerfProc_Process");
            ManagementObjectCollection moc3 = cimobject3.GetInstances();
            foreach (ManagementObject mo3 in moc3)
            {
                object[] pas2 = new object[2];
                Console.WriteLine("{0}", mo3["PercentUserTime"]);
            }
            Console.WriteLine("--------------------------任意键继续----------------------------------");
            Console.ReadKey(true);

            Console.WriteLine("-------------------------系统内存监测---------------------------------");
            //获取总物理内存大小
            ManagementClass cimobject1 = new ManagementClass("Win32_PhysicalMemory");
            ManagementObjectCollection moc1 = cimobject1.GetInstances();
            foreach (ManagementObject mo1 in moc1)
            {
                capacity += ((Math.Round(Int64.Parse(mo1.Properties["Capacity"].Value.ToString()) / 1024 / 1024 / 1024.0, 1)));
            }
            moc1.Dispose();
            cimobject1.Dispose();

            //获取内存可用大小
            ManagementClass cimobject2 = new ManagementClass("Win32_PerfFormattedData_PerfOS_Memory");
            ManagementObjectCollection moc2 = cimobject2.GetInstances();
            foreach (ManagementObject mo2 in moc2)
            {
                available += ((Math.Round(Int64.Parse(mo2.Properties["AvailableMBytes"].Value.ToString()) / 1024.0, 1)));

            }
            moc2.Dispose();
            cimobject2.Dispose();
            Console.WriteLine("MemoryCapacity=" + capacity.ToString() + "G");
            Console.WriteLine("MemoryAvailable=" + available.ToString() + "G");
            Console.WriteLine("MemoryUsed=" + ((capacity - available)).ToString() + "G," + (Math.Round((capacity - available) / capacity * 100, 0)).ToString() + "%");
            Console.WriteLine("--------------------------任意键继续----------------------------------");
            Console.ReadKey(true);


            /*创建一个进程*/
            Console.WriteLine("即将打开记事本，按任意键打开");
            Console.ReadKey(true);
            ManagementClass mc = new ManagementClass("Win32_Process");//Get the object on which the method will be invoked
            ManagementBaseObject obj = mc.GetMethodParameters("Create"); //Get an input parameters object for this method
            //Console.WriteLine("input process name:");
            //char procName;
            //procName = (char)Console.Read();
            obj["CommandLine"] = "notepad.exe";//Fill in input parameter values
            ManagementBaseObject outParams = mc.InvokeMethod("Create", obj, null);//Execute the method
            //Display results
            //Note: The return code of the method is provided in the "returnvalue" property of the outParams object
            Console.WriteLine("Creation of calculator process returned: " + outParams["returnvalue"]);
            Console.WriteLine("Process ID: " + outParams["processId"]);
            Console.WriteLine("open success!");
            mc.Dispose();
            Console.WriteLine("--------------------------任意键继续----------------------------------");
            Console.ReadKey(true);


            /*终止一个进程*/
            /*
            ManagementObject service =
            new ManagementObject("win32_service='QQ'");
            InvokeMethodOptions options = new InvokeMethodOptions();
            options.Timeout = new TimeSpan(0, 0, 0, 5);
            outParams = service.InvokeMethod("StopService", null, options);
            Console.WriteLine("Return Status = " + outParams["Returnvalue"]);
            */





            Console.WriteLine("--------------------------监视进程操作---------------------------------");
            /*监视进程操作*/
            //创建WQL事件查询，用于实例创建
            var qCreate = new WqlEventQuery("__InstanceCreationEvent",
                TimeSpan.FromSeconds(1),  //WHTHIN = 1
                "TargetInstance ISA 'Win32_Process'");

            //创建WQL事件查询，用于实例删除
            var qDelete = new WqlEventQuery("__InstanceDeletionEvent",
                TimeSpan.FromSeconds(1),  //WHTHIN = 1
                "TargetInstance ISA 'Win32_Process'");


            //创建事件查询的侦听器（ManagementEventWatcher）
            var wCreate = new ManagementEventWatcher(qCreate);
            var wDelete = new ManagementEventWatcher(qDelete);

            //事件注册代码
            wCreate.EventArrived += (sender, e) =>
            {
                Console.WriteLine("运行：{0}", GetInfo(e.NewEvent));
            };

            wDelete.EventArrived += (sender, e) =>
            {
                Console.WriteLine("关闭：{0}", GetInfo(e.NewEvent));
            };
            //异步开始侦听
            wCreate.Start();
            wDelete.Start();
            Console.WriteLine("按任意键停止监控");
            Console.ReadKey(true);



            Console.WriteLine("--------------------获取操作系统序列号---------------------------------");
            string result = "";
            ManagementClass mClass = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection moCollection = mClass.GetInstances();
            foreach (ManagementObject mObject in moCollection)
            {
                result += mObject["SerialNumber"].ToString();
            }
            Console.WriteLine(result.ToString());
            Console.WriteLine("--------------------------任意键继续----------------------------------");
            Console.ReadKey(true);


            Console.WriteLine("--------------------获取所有硬盘的使用情况-----------------------------");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root/CIMV2", "SELECT * FROM Win32_LogicalDisk");


            moCollection = searcher.Get();
            string result2 = "";
            foreach (ManagementObject mObject in moCollection)
            {
                //mObject["DriveType"]共有6中可能值，分别代表如下意义：
                //1:No type   2:Floppy disk   3:Hard disk
                //4:Removable drive or network drive   5:CD-ROM   6:RAM disk
                //本处只列出固定驱动器（硬盘分区）的情况
                if (mObject["DriveType"].ToString() == "3")
                {
                    result2 += string.Format("Name={0},FileSystem={1},Size={2},FreeSpace={3}\n", mObject["Name"].ToString(),
                        mObject["FileSystem"].ToString(), mObject["Size"].ToString(), mObject["FreeSpace"].ToString());
                    
                }
            }
            Console.WriteLine(result2.ToString());
            Console.WriteLine("--------------------------任意键继续----------------------------------");
            Console.ReadKey(true);


            Console.WriteLine("--------------------获取指定硬盘的使用情况----------------------------");
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            Console.WriteLine("Logical Disk Size      = " + disk["Size"] + " bytes");
            Console.WriteLine("Logical Disk FreeSpace = " + disk["FreeSpace"] + " bytes");
            Console.WriteLine("--------------------------任意键继续----------------------------------");
            Console.ReadKey(true);
            




        }





        //输出事件对应的ManagementBaseObject（本例中的Win32_Process实例）的信息
        static string GetInfo(ManagementBaseObject mobj)
        {
            var instance = (ManagementBaseObject)mobj["TargetInstance"];
            return string.Format("{0} - {1}", instance["Name"], DateTime.Now);

        }

        public static double capacity { get; set; }

        public static double available { get; set; }

    }
}
