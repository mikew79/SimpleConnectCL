using ControlTechniques.Comms.Discovery;
using ControlTechniques.CommsServer;
using ControlTechniques.Parameters.ParameterFile;
using ControlTechniques.Parameters.RawParameterFile;
using NLog;
using System;
using System.Net;

using System.Threading;
using System.IO;
using System.Xml;

namespace SimpleConnectV2
{
    public enum DownloadType
    {
        NONE,
        PARAMETERS,
        MACRO,
        ONBOARD,
        FIRMWARE,
        MCI
    }

    class SimpleConnect
    {
        //Get instance of logger
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public SimpleConnect()
        {
            logger.Trace("Created a SimpleConnect object");
            //uploadParameter("192.168.1.100", 18, 12);
            //processParamaterFile();
            //saveParameterFile("192.168.1.100");
        }

        /// <summary>
        /// This Method will scan all networks on the PC to disover any drives that are available
        /// </summary>
        public void DiscoverDrives()
        {
            // use this to use a specified interface
            //NetworkExplorerEthernetArguments nweEArgs = new NetworkExplorerEthernetArguments(nwi);
            // this will use all up Ethermet connections
            NetworkExplorerEthernetArguments nweEArgs = new NetworkExplorerEthernetArguments();

            NetworkExplorerArguments nweArgs = new NetworkExplorerArguments(nweEArgs);
            
            logger.Trace("Start Discovery");
            NetworkExplorer nwExplorer = new NetworkExplorer();
            nwExplorer.Go(nweArgs);
            Thread.Sleep(1000); // Need this here to give the Network exploter time to start

            while (nwExplorer.Running) // Wait for network explorer to return
            {
                Thread.Sleep(1000);
            }

            if (nwExplorer.DiscoveredNodes.Count <= 0)
                logger.Info("No Nodes Found");

            foreach (DiscoveredNode node in nwExplorer.DiscoveredNodes)
            {
                DiscoveredDrive dev = (DiscoveredDrive)node;
               // DiscoveredDevice dev = new DiscoveredDevice((DiscoveredDrive)node);
                logger.Info(dev.Address);
                logger.Info(dev.DriveName);
                logger.Info(dev.DriveType);
                logger.Info(dev.DriveType);
                logger.Info(dev.SerialNumber);
                //logger.Info(dev.IsUnreachable.ToString());
                //logger.Info(dev.UnreachableReason.ToString());
                logger.Info(dev.Address.IpAddress.ToString());
            }
            logger.Trace("End Discovery");
        }

        /// <summary>
        /// Download One Paramter to the Drive
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="menuNumber"></param>
        /// <param name="parameterNumber"></param>
        /// <param name="parameterValue"></param>
        /// <param name="decimalPoints"></param>
        public void downloadParameter(String ipAddress, int menuNumber, int parameterNumber, int parameterValue, int decimalPoints)
        {
            BlockingCommsUser commsUser = new BlockingCommsUser();
            // Set the IP address using value from command Line
            CommsAddress address = new CommsAddress(IPAddress.Parse(ipAddress), 0);


            //ParameterValue Number = new ParameterValue(50);
            // Set the menu and parameter to write
            ECMPWriteRequest writeReq = new ECMPWriteRequest();

            ParameterIDAndValueAndDPs param = new ParameterIDAndValueAndDPs(new ParameterID(menuNumber, parameterNumber), new ParameterValue(parameterValue), decimalPoints);
            writeReq.Parameters.Add(param);

            // Set communication protocol
            T_STATUS State = commsUser.Go(Protocol.EthernetProtocol());

            ECMPWriteResponse response; // Define variable to recieve response

            //Execute the Write
            T_RESPONSE_STATUS Status = commsUser.Write(address, writeReq, out response);
            logger.Info("Download Status : "+ Status);
            logger.Info("Response : " + response.OverallStatus.ToString());
        }

        /// <summary>
        /// Upload One Parameter from the drive
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="menuNumber"></param>
        /// <param name="parameterNumber"></param>
        public void uploadParameter(String ipAddress, int menuNumber, int parameterNumber)
        {
            BlockingCommsUser commsUser = new BlockingCommsUser();

            // Set the IP address using value from command Line
            CommsAddress address = new CommsAddress(IPAddress.Parse(ipAddress), 0);

            ECMPReadRequest readReq = new ECMPReadRequest(18, 11);

            // Set communication protocol
            T_STATUS State = commsUser.Go(Protocol.EthernetProtocol());

            ECMPReadResponse response;
            T_RESPONSE_STATUS Status = commsUser.Read(address, readReq, out response);
            logger.Info("Upload Status : " + Status);
            logger.Info("Response : " + response.ParameterValues.ToString());
        }

        public void processParamaterFile()
        {
            ParameterFile parameterFile = new ParameterFile();
            parameterFile.Load(@"c:\Temp\demo.parfile");
            logger.Info(parameterFile.Model.Name);
            logger.Info(parameterFile.Model.FrameSize.ToString());
            logger.Info(parameterFile.Classifier.Product.ToString());
        }

        public void saveParameterFile(IPAddress ipAddress)
        {
            //ParameterFile parameterFile = new ParameterFile();
            //parameterFile.SaveAs(@"C:\Temp\Demo2.parfile");

            BlockingCommsUser commsUser = new BlockingCommsUser();

            CommsAddress address = new CommsAddress(ipAddress, 4); //Changed to opt 4 to test option slot reading writing

            // Set communication protocol
            T_STATUS State = commsUser.Go(Protocol.EthernetProtocol());

           FileReadDiskMessageRequest readDiskReq = new FileReadDiskMessageRequest("/par/all", "C:\\test_par.bin");
            //FileReadDiskMessageRequest readDiskReq = new FileReadDiskMessageRequest("/par/macro", "C:\\test_macro.bin");
            //FileReadDiskMessageRequest readDiskReq = new FileReadDiskMessageRequest("/par/diff", "C:\\test_diff.bin");

            FileReadDiskMessageResponse response;
            T_RESPONSE_STATUS Status = commsUser.FileReadDisk(address, readDiskReq, out response);
            logger.Info("Upload Status : " + Status);

            RawParameterFile rawParams = new RawParameterFile("C:\\test_par.bin");
            //RawParameterFile rawParams = new RawParameterFile("C:\\test_macro.bin");
            //RawParameterFile rawParams = new RawParameterFile("C:\\test_diff.bin");

            //ParameterSerialisationOptions serOpts = new ParameterSerialisationOptions();

            rawParams.ConvertToParameterFile("C:\\Demo4.parfile");
        }

        public void downloadFile(IPAddress ipAddress, string filePath, DownloadType fileType)
        {
            try
            {
                BlockingCommsUser commsUser = new BlockingCommsUser();
                CommsAddress address = new CommsAddress(ipAddress, 0);

                // Set communication protocol
                T_STATUS State = commsUser.Go(Protocol.EthernetProtocol());

                if (State != T_STATUS.OK)
                {
                    logger.Error("Failed To Connect : Status Code : " + State.ToString());
                    return;
                }

                logger.Debug("Connected to Drive");

                string destination = null;
                switch (fileType)
                {
                    case DownloadType.PARAMETERS:
                        //destination = "/par/all"; // can only read
                        destination = "par/macro";
                        break;
                    case DownloadType.MACRO:
                        destination = "/par/macro";
                        break;
                    case DownloadType.ONBOARD:
                        destination = "/sys/prog/user";
                        break;
                }

                string rawFilePath = filePath;
                string parameterFilePath = filePath;

                if (fileType == DownloadType.PARAMETERS)
                {
                    //Change the parameter file into a macro file
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);
                    XmlNodeList aNodes = doc.SelectNodes("/ParameterFile");
                    if (aNodes.Count != 1)
                    {
                        //There should only be one node by this name
                        throw new Exception("ParameterFile Node missing, or incorrectly formated Paarmeter File");
                    }
                    if (aNodes[0].Attributes == null)
                    {
                        throw new Exception("ParameterFile Node incorrectly formated, missing Attributes");
                    }

                    if (aNodes[0].Attributes["type"] == null)
                    {
                        throw new Exception("ParameterFile Node incorrectly formated, missing type Attribute");
                    }
                    aNodes[0].Attributes["type"].Value = "Macro"; //Change the file to a macro file 

                    parameterFilePath = Path.ChangeExtension(filePath, "tmp_par");

                    doc.Save(parameterFilePath);
                    if (!File.Exists(parameterFilePath))
                    {
                        throw new Exception("Error Processing Parameter File Save Macro");
                    }

                }

                if (fileType == DownloadType.PARAMETERS || fileType == DownloadType.MACRO)
                {
                    //Load the parameter file
                    ParameterFile parameterFile = new ParameterFile();
                    parameterFile.Load(parameterFilePath);

                    //convert the parameter file to a raw file for download
                    RawParameterFile rawParams = RawParameterFile.CreateFromParameterFile(parameterFile);

                    rawFilePath = parameterFilePath.Replace(Path.GetExtension(parameterFilePath), ".bin"); //Replace the original filename with the bin file
                    rawParams.Write(rawFilePath, true);
                    if (!File.Exists(rawFilePath))
                    {
                        logger.Error("Error writing intermediate raw file " + rawFilePath);
                        return;
                    }
                    
                }

                FileWriteDiskMessageRequest writeDiskReq = new FileWriteDiskMessageRequest(rawFilePath, destination, FileWriteOptions.None);

                FileWriteDiskMessageResponse response;
                logger.Debug("Downloading File");
                T_RESPONSE_STATUS Status = commsUser.FileWriteDisk(address, writeDiskReq, out response);
                logger.Info("Download Complete");
                //logger.Info("Download Complete - Time Taken : " + response.TimeTaken + " Status : " + response.Status.ToString());

                if (Status != T_RESPONSE_STATUS.OK)
                    logger.Error("Download Failed - Status: " + Status.ToString());

                //Delete the intermediate raw file if created
                if (fileType == DownloadType.PARAMETERS || fileType == DownloadType.MACRO)
                    File.Delete(rawFilePath);

            } catch (Exception ex)
            {
                logger.Error("Execption : " + ex.ToString());
                return;
            }
        }
    }
}
