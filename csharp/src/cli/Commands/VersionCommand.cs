#pragma warning disable CS0067

using System;
using System.Reflection;

namespace Hypar.Commands
{
    internal class VersionCommand : IHyparCommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            var args = (string[])parameter;

            if(args[0] != "version")
            {
                return false;
            }
            return true;
        }

        public void Execute(object parameter)
        {
            Version();
        }

        public void Help()
        {
            Logger.LogInfo("Show the version of hypar and the hypar CLI.");
            Logger.LogInfo("Usage: hypar version");
        }
        
        private void Version()
        {
            Logger.LogInfo($"Hypar Version {typeof(Hypar.Elements.Model).Assembly.GetName().Version.ToString()}");
            Logger.LogInfo($"Hypar CLI Version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            return;
        }
    }
}