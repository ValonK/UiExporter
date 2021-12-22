using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using UiExporter.Exceptions;
using UiExporter.Models;

namespace UiExporter.Services
{
    internal class ElementService : IElementService
    {
        private IList<ControlType> _controlTypes = new List<ControlType>
        {
            ControlType.DataItem,
            ControlType.Pane,
            ControlType.HeaderItem,
            ControlType.ListItem,
            ControlType.MenuItem,
            ControlType.TabItem,
            ControlType.TreeItem,
            ControlType.Tree,
            ControlType.Button,
            ControlType.CheckBox,
            ControlType.DataGrid,
            ControlType.Document,
            ControlType.Group,
            ControlType.Hyperlink,
            ControlType.Image,
            ControlType.Tab,
            ControlType.Text,
            ControlType.ToolTip,
            ControlType.TitleBar,
            ControlType.MenuBar,
            ControlType.StatusBar,
            ControlType.Edit,
            ControlType.List,
            ControlType.Table,
            ControlType.Custom
        };

        public IList<Element> Analyze(Application application)
        {
            if (application.Hwnd == IntPtr.Zero)
            {
                throw new HwndZeroException();
            }

            var elementList = new List<Element>();
            var rootAutomationElement = AutomationElement.FromHandle(application.Hwnd);
            if (rootAutomationElement == null)
            {
                throw new ApplicationHandleFailedException();
            }

            foreach (var controlType in _controlTypes)
            {
                var item = FindItems(rootAutomationElement, controlType);
                if (item.Count > 0)
                {
                    foreach (AutomationElement childElement in item)
                    {
                        if (string.IsNullOrEmpty(childElement.Current.Name))
                        {
                            continue;
                        }
                        var element = new Element
                        {
                            ControlType = controlType,
                            Data = new List<string>(),
                            Name = nameof(controlType)
                        };

                        element.Data.Add(childElement.Current.Name);
                        Debug.WriteLine("Item found: " + childElement.Current.Name);
                        elementList.Add(element);
                    }
                }
            }

            return elementList;
        }

        public string GetApplicationInfo(Application application)
        {
            if (application.Hwnd == IntPtr.Zero)
            {
                throw new HwndZeroException();
            }

            var rootAutomationElement = AutomationElement.FromHandle(application.Hwnd);
            if (rootAutomationElement == null)
            {
                throw new ApplicationHandleFailedException();
            }
            
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Application Information: ");
            stringBuilder.AppendLine(string.Empty);

            if (rootAutomationElement.Current.LocalizedControlType == "pane")
            {
                stringBuilder.AppendLine($"Is Native Application:  False (Build with Web Tech)");
            }
            else
            {
                stringBuilder.AppendLine($"Is Native Application:  True");
            }

            stringBuilder.AppendLine($"Location:  {application.Location}");
            stringBuilder.AppendLine($"Framework Id:  {rootAutomationElement.Current.FrameworkId}");

            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(application.Location);
                if (!string.IsNullOrEmpty(versionInfo?.FileVersion))
                {
                    stringBuilder.AppendLine($"Version:  {versionInfo?.FileVersion}");
                }

                if (!string.IsNullOrEmpty(versionInfo?.CompanyName))
                {
                    stringBuilder.AppendLine($"Company:  {versionInfo?.CompanyName}");
                }

                if (!string.IsNullOrEmpty(versionInfo?.OriginalFilename))
                {
                    stringBuilder.AppendLine($"Original Filename:  {versionInfo?.OriginalFilename}");
                }
                
                if (!string.IsNullOrEmpty(versionInfo?.InternalName))
                {
                    stringBuilder.AppendLine($"Internal Name:  {versionInfo?.InternalName}");
                }

                if (!string.IsNullOrEmpty(versionInfo?.Comments))
                {
                    stringBuilder.AppendLine($"Comments:  {versionInfo?.Comments}");
                }

                if (!string.IsNullOrEmpty(versionInfo?.FileDescription))
                {
                    stringBuilder.AppendLine($"Description:  {versionInfo?.FileDescription}");
                }

                if (!string.IsNullOrEmpty(versionInfo?.Language))
                {
                    stringBuilder.AppendLine($"Language:  {versionInfo?.Language}");
                }

                if (!string.IsNullOrEmpty(versionInfo?.LegalCopyright))
                {
                    stringBuilder.AppendLine($"Legal Copyright:  {versionInfo?.LegalCopyright}");
                }

                stringBuilder.AppendLine($"DEBUG Mode:  {versionInfo?.IsDebug}");
                stringBuilder.AppendLine($"Is Patched:  {versionInfo?.IsPatched}");
                stringBuilder.AppendLine($"Is Pre Release:  {versionInfo?.IsPreRelease}");
                stringBuilder.AppendLine($"Is Special Build:  {versionInfo?.IsSpecialBuild}");
            }
            catch
            {
                // ignored
            }


            return stringBuilder.ToString();
        }

        private AutomationElementCollection FindItems(AutomationElement element, ControlType controlType)
        {
            var condition = new PropertyCondition(AutomationElement.ControlTypeProperty, controlType);
            return element.FindAll(TreeScope.Descendants, condition);
        }
    }
}
