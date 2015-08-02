using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management;
using YAHW.Interfaces;
using YAHW.Model;
using System.Net.NetworkInformation;
using YAHW.Constants;
using ATI.ADL;
using System.Runtime.InteropServices;

namespace YAHW.Services
{
    /// <summary>
    /// <para>
    /// Service for retrieving hardware information with WMI
    /// </para>
    /// 
    /// <para>
    /// Class history:
    /// <list type="bullet">
    ///     <item>
    ///         <description>1.0: First release, working (Steffen Steinbrecher).</description>
    ///     </item>
    /// </list>
    /// </para>
    /// 
    /// <para>Author: Steffen Steinbrecher</para>
    /// <para>Date: 12.07.2015</para>
    /// </summary>
    public class WmiHardwareInfoService : IHardwareInformationService
    {
        #region Mainboard

        /// <summary>
        /// Get mainboard information
        /// </summary>
        /// <returns></returns>
        public MainboardInformation GetMainboardInformation()
        {
            MainboardInformation result = null;
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_BaseBoard");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    result = new MainboardInformation();
                    result.Manufacturer = queryObj["Manufacturer"].ToString();
                    result.Product = queryObj["Product"].ToString();
                    result.SerialNumber = queryObj["SerialNumber"].ToString();
                    break;
                }
            }
            catch (ManagementException e)
            {
                DependencyFactory.Resolve<IExceptionReporterService>(ServiceNames.ExceptionReporterService).ReportException(e);
                // TODO: Logging
            }

            return result;
        }

        #endregion Mainboard

        #region CPU

        /// <summary>
        /// Try to get processor information
        /// </summary>
        /// <returns></returns>
        public ProcessorInformation GetProcessorInformation()
        {
            ProcessorInformation result = null;

            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_Processor");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    result = new ProcessorInformation();

                    result.Caption = queryObj["Caption"].ToString();
                    result.CurrentClockSpeed = (queryObj["CurrentClockSpeed"] != null) ? Convert.ToUInt32(queryObj["CurrentClockSpeed"]) : 0;
                    result.Description = queryObj["Description"].ToString();
                    result.ExtClock = (queryObj["ExtClock"] != null) ? Convert.ToUInt32(queryObj["ExtClock"]) : 0;
                    result.L2CacheSize = (queryObj["L2CacheSize"] != null) ? Convert.ToUInt32(queryObj["L2CacheSize"]) : 0;
                    result.L3CacheSize = (queryObj["L3CacheSize"] != null) ? Convert.ToUInt32(queryObj["L3CacheSize"]) : 0;
                    result.Manufacturer = queryObj["Manufacturer"].ToString();
                    result.MaxClockSpeed = (queryObj["MaxClockSpeed"] != null) ? (0.001 * (UInt32)(queryObj["MaxClockSpeed"])).ToString("0.00") + " GHz" : "n.a.";
                    result.Name = queryObj["Name"].ToString();
                    result.NumberOfCores = (queryObj["NumberOfCores"] != null) ? Convert.ToUInt32(queryObj["NumberOfCores"]) : 0;
                    result.NumberOfLogicalProcessors = (queryObj["NumberOfLogicalProcessors"] != null) ? Convert.ToUInt32(queryObj["NumberOfLogicalProcessors"]) : 0;
                }
            }
            catch (ManagementException e)
            {
                DependencyFactory.Resolve<IExceptionReporterService>(ServiceNames.ExceptionReporterService).ReportException(e);
                // TODO: Logging
            }

            return result;
        }

        #endregion CPU

        #region GPU

        /// <summary>
        /// Get GPU-Information
        /// </summary>
        /// <returns></returns>
        public GPUInformation GetGPUInformation()
        {
            GPUInformation result = null;

            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    result = new GPUInformation();

                    result.Caption = (queryObj["Caption"] != null) ? queryObj["Caption"].ToString() : "n.a.";
                    result.CurrentBitsPerPixel = (queryObj["CurrentBitsPerPixel"] != null) ? Convert.ToUInt32(queryObj["CurrentBitsPerPixel"]) : 0;
                    result.CurrentHorizontalResolution = (queryObj["CurrentHorizontalResolution"] != null) ? Convert.ToUInt32(queryObj["CurrentHorizontalResolution"]) : 0;
                    result.CurrentNumberOfColors = (queryObj["CurrentNumberOfColors"] != null) ? Convert.ToUInt64(queryObj["CurrentNumberOfColors"]) : 0;
                    result.CurrentRefreshRate = (queryObj["CurrentRefreshRate"] != null) ? Convert.ToUInt32(queryObj["CurrentRefreshRate"]) : 0;
                    result.CurrentVerticalResolution = (queryObj["CurrentVerticalResolution"] != null) ? Convert.ToUInt32(queryObj["CurrentVerticalResolution"]) : 0;
                    result.Description = (queryObj["Description"] != null) ? queryObj["Description"].ToString() : "n.a.";
                    //result.DriverDate = (queryObj["DriverDate"] != null) ? Convert.ToDateTime(queryObj["DriverDate"]) : null;
                    result.DriverVersion = (queryObj["DriverVersion"] != null) ? queryObj["DriverVersion"].ToString() : "n.a.";
                    result.InfFilename = (queryObj["InfFilename"] != null) ? queryObj["InfFilename"].ToString() : "n.a.";
                    result.InstalledDisplayDrivers = (queryObj["InstalledDisplayDrivers"] != null) ? queryObj["InstalledDisplayDrivers"].ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries) : null;
                    result.MaxRefreshRate = (queryObj["MaxRefreshRate"] != null) ? Convert.ToUInt32(queryObj["MaxRefreshRate"]) : 0;
                    result.MinRefreshRate = (queryObj["MinRefreshRate"] != null) ? Convert.ToUInt32(queryObj["MinRefreshRate"]) : 0;
                    result.Name = (queryObj["Name"] != null) ? queryObj["Name"].ToString() : "n.a.";
                    result.VideoModeDescription = (queryObj["VideoModeDescription"] != null) ? queryObj["VideoModeDescription"].ToString() : "n.a.";
                    result.VideoProcessor = (queryObj["VideoProcessor"] != null) ? queryObj["VideoProcessor"].ToString() : "n.a.";

                    break;
                }
            }
            catch (ManagementException e)
            {
                DependencyFactory.Resolve<IExceptionReporterService>(ServiceNames.ExceptionReporterService).ReportException(e);
                // TODO: Logging
            }

            int ADLRet = -1;
            int NumberOfAdapters = 0;
            int NumberOfDisplays = 0;

            if (null != ADL.ADL_Main_Control_Create)
                // Second parameter is 1: Get only the present adapters
                ADLRet = ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);
            if (ADL.ADL_SUCCESS == ADLRet)
            {
                if (null != ADL.ADL_Adapter_NumberOfAdapters_Get)
                {
                    ADL.ADL_Adapter_NumberOfAdapters_Get(ref NumberOfAdapters);
                }
                Console.WriteLine("Number Of Adapters: " + NumberOfAdapters.ToString() + "\n");

                if (0 < NumberOfAdapters)
                {
                    // Get OS adpater info from ADL
                    ADLAdapterInfoArray OSAdapterInfoData;
                    OSAdapterInfoData = new ADLAdapterInfoArray();

                    if (null != ADL.ADL_Adapter_AdapterInfo_Get)
                    {
                        IntPtr AdapterBuffer = IntPtr.Zero;
                        int size = Marshal.SizeOf(OSAdapterInfoData);
                        AdapterBuffer = Marshal.AllocCoTaskMem((int)size);
                        Marshal.StructureToPtr(OSAdapterInfoData, AdapterBuffer, false);

                        if (null != ADL.ADL_Adapter_AdapterInfo_Get)
                        {
                            ADLRet = ADL.ADL_Adapter_AdapterInfo_Get(AdapterBuffer, size);
                            if (ADL.ADL_SUCCESS == ADLRet)
                            {
                                OSAdapterInfoData = (ADLAdapterInfoArray)Marshal.PtrToStructure(AdapterBuffer, OSAdapterInfoData.GetType());
                                int IsActive = 0;

                                for (int i = 0; i < NumberOfAdapters; i++)
                                {
                                    // Check if the adapter is active
                                    if (null != ADL.ADL_Adapter_Active_Get)
                                        ADLRet = ADL.ADL_Adapter_Active_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref IsActive);

                                    /*ADLMemoryInfo ADL_Memory = new ADLMemoryInfo();

                                    if (null != ADL.ADL_Adapter_Memoryinfo_Get)
                                    {
                                        ADL.ADL_Adapter_Memoryinfo_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref ADL_Memory);

                                        double MemorySize = Convert.ToDouble(ADL_Memory.iMemorySize.ToString());
                                        MemorySize /= 1048576;//Mebibyte (MiB)     220 Byte = 1.048.576 Byte ( taken from wiki )

                                        Console.WriteLine("Memory Size    : " + MemorySize.ToString() + " MB");

                                    }*/

                                    if (ADL.ADL_SUCCESS == ADLRet)
                                    {
                                        Console.WriteLine("Adapter is   : " + (0 == IsActive ? "DISABLED" : "ENABLED"));
                                        Console.WriteLine("Adapter Index: " + OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex.ToString());
                                        Console.WriteLine("Adapter UDID : " + OSAdapterInfoData.ADLAdapterInfo[i].UDID);
                                        Console.WriteLine("Bus No       : " + OSAdapterInfoData.ADLAdapterInfo[i].BusNumber.ToString());
                                        Console.WriteLine("Driver No    : " + OSAdapterInfoData.ADLAdapterInfo[i].DriverNumber.ToString());
                                        Console.WriteLine("Function No  : " + OSAdapterInfoData.ADLAdapterInfo[i].FunctionNumber.ToString());
                                        Console.WriteLine("Vendor ID    : " + OSAdapterInfoData.ADLAdapterInfo[i].VendorID.ToString());
                                        Console.WriteLine("Adapter Name : " + OSAdapterInfoData.ADLAdapterInfo[i].AdapterName);
                                        Console.WriteLine("Display Name : " + OSAdapterInfoData.ADLAdapterInfo[i].DisplayName);
                                        Console.WriteLine("Present      : " + (0 == OSAdapterInfoData.ADLAdapterInfo[i].Present ? "No" : "Yes"));
                                        Console.WriteLine("Exist        : " + (0 == OSAdapterInfoData.ADLAdapterInfo[i].Exist ? "No" : "Yes"));
                                        Console.WriteLine("Driver Path  : " + OSAdapterInfoData.ADLAdapterInfo[i].DriverPath);
                                        Console.WriteLine("Driver Path X: " + OSAdapterInfoData.ADLAdapterInfo[i].DriverPathExt);
                                        Console.WriteLine("PNP String   : " + OSAdapterInfoData.ADLAdapterInfo[i].PNPString);

                                        // Obtain information about displays
                                        ADLDisplayInfo oneDisplayInfo = new ADLDisplayInfo();

                                        if (null != ADL.ADL_Display_DisplayInfo_Get)
                                        {
                                            IntPtr DisplayBuffer = IntPtr.Zero;
                                            int j = 0;

                                            // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                            ADLRet = ADL.ADL_Display_DisplayInfo_Get(OSAdapterInfoData.ADLAdapterInfo[i].AdapterIndex, ref NumberOfDisplays, out DisplayBuffer, 1);
                                            if (ADL.ADL_SUCCESS == ADLRet)
                                            {
                                                List<ADLDisplayInfo> DisplayInfoData = new List<ADLDisplayInfo>();
                                                for (j = 0; j < NumberOfDisplays; j++)
                                                {
                                                    oneDisplayInfo = (ADLDisplayInfo)Marshal.PtrToStructure(new IntPtr(DisplayBuffer.ToInt32() + j * Marshal.SizeOf(oneDisplayInfo)), oneDisplayInfo.GetType());
                                                    DisplayInfoData.Add(oneDisplayInfo);
                                                }
                                                Console.WriteLine("\nTotal Number of Displays supported: " + NumberOfDisplays.ToString());
                                                Console.WriteLine("\nDispID  AdpID  Type OutType  CnctType Connected  Mapped  InfoValue DisplayName ");

                                                for (j = 0; j < NumberOfDisplays; j++)
                                                {
                                                    int InfoValue = DisplayInfoData[j].DisplayInfoValue;
                                                    string StrConnected = (1 == (InfoValue & 1)) ? "Yes" : "No ";
                                                    string StrMapped = (2 == (InfoValue & 2)) ? "Yes" : "No ";
                                                    int AdpID = DisplayInfoData[j].DisplayID.DisplayLogicalAdapterIndex;
                                                    string StrAdpID = (AdpID < 0) ? "--" : AdpID.ToString("d2");

                                                    Console.WriteLine(DisplayInfoData[j].DisplayID.DisplayLogicalIndex.ToString() + "        " +
                                                                         StrAdpID + "      " +
                                                                         DisplayInfoData[j].DisplayType.ToString() + "      " +
                                                                         DisplayInfoData[j].DisplayOutputType.ToString() + "      " +
                                                                         DisplayInfoData[j].DisplayConnector.ToString() + "        " +
                                                                         StrConnected + "        " +
                                                                         StrMapped + "      " +
                                                                         InfoValue.ToString("x4") + "   " +
                                                                         DisplayInfoData[j].DisplayName.ToString());
                                                }
                                                Console.WriteLine();
                                            }
                                            else
                                            {
                                                Console.WriteLine("ADL_Display_DisplayInfo_Get() returned error code " + ADLRet.ToString());
                                            }
                                            // Release the memory for the DisplayInfo structure
                                            if (IntPtr.Zero != DisplayBuffer)
                                                Marshal.FreeCoTaskMem(DisplayBuffer);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("ADL_Adapter_AdapterInfo_Get() returned error code " + ADLRet.ToString());
                            }
                        }
                        // Release the memory for the AdapterInfo structure
                        if (IntPtr.Zero != AdapterBuffer)
                            Marshal.FreeCoTaskMem(AdapterBuffer);
                    }
                }
                if (null != ADL.ADL_Main_Control_Destroy)
                    ADL.ADL_Main_Control_Destroy();
            }
            else
            {
                Console.WriteLine("ADL_Main_Control_Create() returned error code " + ADLRet.ToString());
                Console.WriteLine("\nCheck if ADL is properly installed!\n");
            }

            Console.WriteLine("Press ENTER to EXIT");
            Console.ReadLine();
      

            return result;
        }

        #endregion GPU

        #region Physical Memory (RAM)

        /// <summary>
        /// Get physical memory information
        /// </summary>
        /// <returns></returns>
        public IList<RAMInformation> GetPhysicalMemoryInformation()
        {
            IList<RAMInformation> result = new List<RAMInformation>();

            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    var rb = new RAMInformation();

                    rb.BankLabel = (queryObj["BankLabel"] != null) ? queryObj["BankLabel"].ToString() : "n.a.";
                    rb.Capacity = (queryObj["Capacity"] != null) ? Convert.ToUInt64(queryObj["Capacity"]) : default(UInt64);
                    rb.DataWidth = (queryObj["DataWidth"] != null) ? Convert.ToUInt16(queryObj["DataWidth"]) : default(UInt16);
                    rb.DeviceLocator = (queryObj["DeviceLocator"] != null) ? queryObj["DeviceLocator"].ToString() : "n.a.";
                    rb.FormFactor = (queryObj["FormFactor"] != null) ? Convert.ToUInt16(queryObj["FormFactor"]) : default(UInt16);
                    rb.Manufacturer = (queryObj["Manufacturer"] != null) ? queryObj["Manufacturer"].ToString() : "n.a.";
                    rb.MemoryType = (queryObj["MemoryType"] != null) ? Convert.ToUInt16(queryObj["MemoryType"]) : default(UInt16);
                    rb.Model = (queryObj["Model"] != null) ? queryObj["Model"].ToString() : "n.a.";
                    rb.Name = (queryObj["Name"] != null) ? queryObj["Name"].ToString() : "n.a.";
                    rb.PartNumber = (queryObj["PartNumber"] != null) ? queryObj["PartNumber"].ToString() : "n.a.";
                    rb.SerialNumber = (queryObj["SerialNumber"] != null) ? queryObj["SerialNumber"].ToString() : "n.a.";
                    rb.Speed = (queryObj["Speed"] != null) ? Convert.ToUInt32(queryObj["Speed"]) : default(UInt32);
                    rb.TotalWidth = (queryObj["TotalWidth"] != null) ? Convert.ToUInt16(queryObj["TotalWidth"]) : default(UInt16);
                    rb.TypeDetail = (queryObj["TypeDetail"] != null) ? Convert.ToUInt16(queryObj["TypeDetail"]) : default(UInt16);

                    result.Add(rb);
                }
            }
            catch (ManagementException e)
            {
                DependencyFactory.Resolve<IExceptionReporterService>(ServiceNames.ExceptionReporterService).ReportException(e);
                // TODO: Logging
            }

            return result;
        }

        #endregion Physical Memory (RAM)

        #region HDD

        /// <summary>
        /// Get HDD SMART Information for installed drives
        /// Initial version from here: http://www.know24.net/blog/C+WMI+HDD+SMART+Information.aspx
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, HDD> GetHddSmartInformation()
        {
            // retrieve list of drives on computer (this will return both HDD's and CDROM's and Virtual CDROM's)                    
            var result = new Dictionary<int, HDD>();

            try
            {
                var wdSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

                // extract model and interface information
                int iDriveIndex = 0;
                foreach (ManagementObject drive in wdSearcher.Get())
                {
                    var hdd = new HDD();
                    hdd.Model = drive["Model"].ToString().Trim();
                    hdd.Type = drive["InterfaceType"].ToString().Trim();
                    hdd.Firmware = drive["FirmwareRevision"].ToString().Trim();
                    hdd.TotalSize = Convert.ToUInt64(drive["Size"]);
                    result.Add(iDriveIndex, hdd);
                    iDriveIndex++;

                    // Get Partitions
                    string DiskName = "Disk " + drive["Index"].ToString() + " : " + drive["Caption"].ToString().Replace(" ATA Device", "") +
                        " (" + Math.Round(Convert.ToDouble(drive["Size"]) / 1073741824, 1) + " GB)";

                    int ObjCount = Convert.ToInt16(drive["Partitions"]);
                    ManagementObjectSearcher partitions = new ManagementObjectSearcher("Select * From Win32_DiskPartition Where DiskIndex='" + drive["Index"].ToString() + "'");

                    foreach (ManagementObject part in partitions.Get())
                    {
                        HDDPartition p = new HDDPartition();

                        p.PartitionName = "Partition " + part["Index"].ToString();

                        string PartName = part["DeviceID"].ToString();
                        if (part["Bootable"].ToString() == "True" && part["BootPartition"].ToString() == "True")
                        {
                            p.DiskName = "Recovery";
                            p.TotalSpace = Convert.ToUInt64(part["Size"]);
                        }
                        else
                        {
                            ManagementObjectSearcher getdisks = new ManagementObjectSearcher("Select * From Win32_LogicalDiskToPartition Where  ");
                            p.DriveLetter = this.GetPartitionName(PartName);
                            GetFreeSpace(p.DriveLetter, ref p);
                            p.DiskName = "Local Disk (" + p.DriveLetter + ")";
                        }
                        
                        hdd.Partitions.Add(p);
                    }
                }

                var pmsearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

                // retrieve hdd serial number
                iDriveIndex = 0;
                foreach (ManagementObject drive in pmsearcher.Get())
                {
                    // because all physical media will be returned we need to exit
                    // after the hard drives serial info is extracted
                    if (iDriveIndex >= result.Count)
                        break;

                    result[iDriveIndex].Serial = drive["SerialNumber"] == null ? "None" : drive["SerialNumber"].ToString().Trim();
                    iDriveIndex++;
                }

                // get wmi access to hdd 
                var searcher = new ManagementObjectSearcher("Select * from Win32_DiskDrive");
                searcher.Scope = new ManagementScope(@"\root\wmi");

                // check if SMART reports the drive is failing
                searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictStatus");
                iDriveIndex = 0;
                foreach (ManagementObject drive in searcher.Get())
                {
                    result[iDriveIndex].IsOK = (bool)drive.Properties["PredictFailure"].Value == false;
                    iDriveIndex++;
                }

                // retrive attribute flags, value worste and vendor data information
                // MSStorageDriver_ATAPISmartData
                // MSStorageDriver_FailurePredictData
                searcher.Query = new ObjectQuery("Select * from MSStorageDriver_ATAPISmartData");
                iDriveIndex = 0;
                foreach (ManagementObject data in searcher.Get())
                {
                    Byte[] bytes = (Byte[])data.Properties["VendorSpecific"].Value;
                    for (int i = 0; i < 30; ++i)
                    {
                        try
                        {
                            int id = bytes[i * 12 + 2];

                            int flags = bytes[i * 12 + 4]; // least significant status byte, +3 most significant byte, but not used so ignored.
                            //bool advisory = (flags & 0x1) == 0x0;
                            bool failureImminent = (flags & 0x1) == 0x1;
                            //bool onlineDataCollection = (flags & 0x2) == 0x2;

                            int value = bytes[i * 12 + 5];
                            int worst = bytes[i * 12 + 6];
                            int vendordata = BitConverter.ToInt32(bytes, i * 12 + 7);
                            if (id == 0) continue;

                            var attr = result[iDriveIndex].Attributes[id];
                            attr.Current = value;
                            attr.Worst = worst;
                            attr.Data = vendordata;
                            attr.IsOK = failureImminent == false;
                        }
                        catch
                        {
                            // given key does not exist in attribute collection (attribute not in the dictionary of attributes)
                        }
                    }
                    iDriveIndex++;
                }

                // retreive threshold values foreach attribute
                searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictThresholds");
                iDriveIndex = 0;
                foreach (ManagementObject data in searcher.Get())
                {
                    Byte[] bytes = (Byte[])data.Properties["VendorSpecific"].Value;
                    for (int i = 0; i < 30; ++i)
                    {
                        try
                        {

                            int id = bytes[i * 12 + 2];
                            int thresh = bytes[i * 12 + 3];
                            if (id == 0) continue;

                            var attr = result[iDriveIndex].Attributes[id];
                            attr.Threshold = thresh;
                        }
                        catch
                        {
                            // given key does not exist in attribute collection (attribute not in the dictionary of attributes)
                        }
                    }

                    iDriveIndex++;
                }
            }
            catch (ManagementException ex)
            {
                DependencyFactory.Resolve<IExceptionReporterService>(ServiceNames.ExceptionReporterService).ReportException(ex);
                // TODO: Logging
            }

            return result;
        }

        /// <summary>
        /// Get free space of a partition
        /// </summary>
        /// <param name="inp"></param>
        /// <returns></returns>
        private String GetFreeSpace(String inp)
        {
            String totalspace = "", freespace = "", freepercent = "";
            Double sFree = 0, sTotal = 0, sEq = 0;
            ManagementObjectSearcher getspace = new ManagementObjectSearcher("Select * from Win32_LogicalDisk Where DeviceID='" + inp + "'");
            foreach (ManagementObject drive in getspace.Get())
            {
                if (drive["DeviceID"].ToString() == inp)
                {
                    freespace = drive["FreeSpace"].ToString();
                    totalspace = drive["Size"].ToString();
                    sFree = Convert.ToDouble(freespace);
                    sTotal = Convert.ToDouble(totalspace);
                    sEq = sFree * 100 / sTotal;
                    freepercent = (Math.Round((sTotal - sFree) / 1073741824, 2)).ToString() + " (" + Math.Round(sEq, 0).ToString() + " %)";
                    return freepercent;
                }
            }

            return "";
        }

        private void GetFreeSpace(string inp, ref HDDPartition partition)
        {
            ManagementObjectSearcher getspace = new ManagementObjectSearcher("Select * from Win32_LogicalDisk Where DeviceID='" + inp + "'");
            foreach (ManagementObject drive in getspace.Get())
            {
                if (drive["DeviceID"].ToString() == inp)
                {
                    partition.FreeSpace = Convert.ToUInt64(drive["FreeSpace"]);
                    partition.TotalSpace = Convert.ToUInt64(drive["Size"].ToString());
                    partition.FreeSapceInPercent = partition.FreeSpace * 100 / partition.TotalSpace;
                }
            }
        }

        /// <summary>
        /// Get partition name
        /// </summary>
        /// <param name="inp"></param>
        /// <returns></returns>
        private String GetPartitionName(String inp)
        {
            //MessageBox.Show(inp);
            String Dependent = "", ret = "";
            ManagementObjectSearcher LogicalDisk = new ManagementObjectSearcher("Select * from Win32_LogicalDiskToPartition");
            foreach (ManagementObject drive in LogicalDisk.Get())
            {
                if (drive["Antecedent"].ToString().Contains(inp))
                {
                    Dependent = drive["Dependent"].ToString();
                    ret = Dependent.Substring(Dependent.Length - 3, 2);
                    break;
                }
            }

            return ret;
        }

        #endregion HDD

        #region Network

        public void GetNetworkAdapterInformation()
        {
            string[] s = new string[0];
            int i = 0;

            NetworkInterface[] nicArr = NetworkInterface.GetAllNetworkInterfaces();
            for (int iNet = 0; iNet < nicArr.Length; iNet++)
            {
                if (nicArr[iNet].NetworkInterfaceType != NetworkInterfaceType.Tunnel
                    && nicArr[i].NetworkInterfaceType != NetworkInterfaceType.Loopback
                    && nicArr[iNet].OperationalStatus == OperationalStatus.Up
                    && !nicArr[iNet].Name.Contains("Loopback"))
                {
                    Array.Resize(ref s, i + 1);
                    s[i] = nicArr[iNet].Name;
                    i++;
                }
            }
        }

        public string GetNetworkAdapter(ManagementObject m)
        {
            RegistryKey rK = Registry.LocalMachine;
            string s = "";
            s = m["SettingID"].ToString();
            RegistryKey rSub = rK.OpenSubKey("SYSTEM\\CurrentControlSet\\Control" +
                        "\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" +
                        s + "\\Connection");
            s = rSub.GetValue("Name").ToString();
            return s;
        }

        #endregion Network
    }
}