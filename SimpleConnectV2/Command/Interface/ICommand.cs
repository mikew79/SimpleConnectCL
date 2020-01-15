
using System.Collections.Generic;

namespace SimpleConnectV2
{
    interface ICommand
    {
        string name { get; }            //The command name for this command object
        string[] reqArgs { get; }      //A List of required arguments for this command
        string[] optArgs { get; }      //A List of optional arguments for this command

        
        /// <summary>
        /// Checks if the argumements passed to this command are valid fro the command
        /// </summary>
        /// <returns>TRUE if the Arguments match the required argument and the optional arguments</returns>
        bool isArgsValid(Dictionary<string, string> cmdLineArgs);

        /// <summary>
        /// Returns the last error 
        /// </summary>
        /// <returns>Last Error</returns>
        string getLastError();

        /// <summary>
        /// The main method to eexecute this command object
        /// </summary>
        /// <param name="cmdLineArgs">Dicitionary of command line arguments passed to the command</param>
        void DoAction(Dictionary<string, string> cmdLineArgs, SimpleConnect scApp);


    }
}
