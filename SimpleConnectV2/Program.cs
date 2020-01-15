using NLog;
using SimpleConnectV2.Command;
using System;
using System.Collections.Generic;

namespace SimpleConnectV2
{
    class Program
    {

        // List of valid commands
        private static ICommand[] commandList = {
            new CommandDownload("DOWNLOAD"),
            new CommandUpload("UPLOAD"),
            new CommandIdentify("IDENTIFY")
        };

        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            //Valid Commands are 
            // identify --protocol=ip                                               : List all drives available acroos the network
            // download --type=param --ip=192.168.1.100 --file=c:\test.parfile      : Download a parameter or Macro File
            // upload --ip=192.168.1.2 --file=c:\test.parfile                       : Upload a pramater set from the drive

            // to do
            // ConvertParFile                                                       : Convert a parameter File to a binary File
            // ConvertBinFile                                                       : Convert a binary File to a parameter File
            // Reset                                                                : Reset a drive
            // ChangeMode                                                           : change the mode of the drive RFC-A/S Open etc
            // GetFWVersion                                                         : Get Firmware versions of all fitted modules
            // GetSerialNo                                                          : Get serial Numbers of drive and all option slots


            //Global options are
            // -quiet      : Don't output anything if all is ok
            // -v          : Verbose output
            // -pause      : Wait for a key press before doing anything
            // -timeout=n  : Sets the timeouts to n milliseconds
            // -?          : Help

            //Serial comms
            // -port=n    : Sets the serial port number, default is 1
            // -baud = n  : Sets the serial port baud rate, default is 19200

            Console.WriteLine("Simple Connect V2.0.0 ");
            Console.WriteLine("Control Techniques - 2019");

            //First of all extracyt the command line to its fundamenetals
            logger.Trace("Processing Command Line");
            Dictionary<string, string> cmdLineArgs = null;
            ICommand command = processComandLine(args, out cmdLineArgs);
            
            if (command == null)
            {
                logger.Debug("command returned from processCommandLine() was null");
                Console.WriteLine("ERROR : Program Command was not valid");
                printHelp();
                return;
            }

            //Check command line arguments are valid
            if(!command.isArgsValid(cmdLineArgs))
            {
                logger.Debug("Command Line Arguments not found");
                Console.WriteLine("ERROR : " + command.getLastError());
                printHelp();
                Console.ReadKey();
                return;
            }

            logger.Trace("Create new SimpleConnectObject");
            SimpleConnect sc = new SimpleConnect();

            //run the actual command
            command.DoAction(cmdLineArgs, sc);


            Console.ReadKey();
        }

        private static void printHelp()
        {
            Console.WriteLine("Help");
        }

        private static ICommand processComandLine(string[] args, out Dictionary<string, string> cmdLineArgs)
        {
            //Check if we have at least the command argument
            if (args.Length < 1)
            {
                printHelp();
                cmdLineArgs = null;
                return null;
            }

            //Look for a command and see if it matches what we requested
            ICommand foundCommand = null;

            logger.Debug("Command is" + args[0].ToUpper());
            foreach (ICommand cmd in commandList)
            {
                logger.Debug("Checking command " + cmd);
                if (cmd.name.ToUpper().Equals(args[0].ToUpper()))
                {
                    logger.Debug("Found Command");
                    foundCommand = cmd;
                    break;
                }
            }

            //Not found a valid command so return nothing
            if (foundCommand == null)
            {
                cmdLineArgs = null;
                return null;
            }

            // process the other command line arguments for returning
            cmdLineArgs = new Dictionary<string, string>();
            for (int i = 1; i < args.Length; i++)
            {
                string[] data = args[i].Split('=');
                cmdLineArgs.Add(data[0].Replace("--", "").ToUpper(), data[1]);
                logger.Debug("Argument : " + data[0].Replace("--", "") + "     Value : " + data[1]);
            }

            //Return the macthing command object
            return foundCommand;
        }

    }
}
