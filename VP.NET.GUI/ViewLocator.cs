using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.ComponentModel;
using VP.NET.GUI.ViewModels;

namespace VP.NET.GUI
{
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object? data)
        {
            if (data != null)
            {
                var name = data.GetType().FullName!.Replace("ViewModel", "View");
#pragma warning disable IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
                var type = Type.GetType(name);
#pragma warning restore IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.

                if (type != null)
                {
                    return (Control)Activator.CreateInstance(type)!;
                }
                else
                {
                    return new TextBlock { Text = "Not Found: " + name };
                }
            }
            else
            {
                return new TextBlock { Text = "Not Found" };
            }
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}