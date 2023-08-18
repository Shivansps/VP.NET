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
                var type = Type.GetType(name);

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