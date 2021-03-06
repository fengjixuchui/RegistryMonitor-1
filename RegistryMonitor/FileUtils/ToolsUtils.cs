﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RegistryMonitor.Files;
using RegistryMonitor.Structs;
using RegistryMonitor.Utils;

namespace RegistryMonitor.FileUtils
{
    public class ToolsUtils
    {
        private const string TOOLS_FILE_NAME = "tools.json";

        public static void WriteToolsSettings(IEnumerable<LoadedTools> tools)
        {
            string toolJsonFile = Path.Combine(Directory.GetCurrentDirectory(), TOOLS_FILE_NAME);

            try
            {
                using (StreamWriter file = File.CreateText(toolJsonFile))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    foreach (var tool in tools)
                    {
                        var jsonTool = JsonConvert.SerializeObject(tool);
                        writer.WriteRaw(jsonTool);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Constants.ToolMessages.ErrorWritingFile}{ex}", 
                    Constants.ToolMessages.ErrorWritingFileCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        public static List<LoadedTools> ReadToolsSettings()
        {
            var tools = new List<LoadedTools>();

            string toolJsonFile = Path.Combine(Directory.GetCurrentDirectory(), TOOLS_FILE_NAME);

            if (!File.Exists(toolJsonFile))
            {
                tools = GetGenericTools();
                WriteToolsSettings(tools);
            }
            else
            {
                try
                {
                    using (StreamReader file = File.OpenText(toolJsonFile))
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        var tool = new LoadedTools();

                        reader.SupportMultipleContent = true;

                        while (reader.Read())
                        {
                            JObject o3 = (JObject) JToken.ReadFrom(reader);
                            foreach (var child in o3.Children())
                            {
                                AddPropertyToTool(tool, child.Path, child.First.ToString());
                            }
                            tools.Add(tool);
                            tool = new LoadedTools();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Constants.ToolMessages.ErrorWritingFile}{ex}", 
                        Constants.ToolMessages.ErrorWritingFileCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw;
                }
            }
            return tools;
        }

        private static LoadedTools AddPropertyToTool(LoadedTools loadedTool, string propertyName, string propertyValue)
        {
            switch (propertyName)
            {
                case FileConstants.Tools.ID:
                    loadedTool.ID = Guid.Parse(propertyValue);
                    break;
                case FileConstants.Tools.NAME:
                    loadedTool.Name = propertyValue;
                    break;
                case FileConstants.Tools.FILE_LOCATION:
                    loadedTool.FileLocation = propertyValue;
                    break;
                case FileConstants.Tools.HOTKEY:
                    loadedTool.HotKey = propertyValue;
                    break;
            }
            return loadedTool;
        }

        public static List<LoadedTools> GetGenericTools()
        {
            var fileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Documents\\AutoIT Scripts\\SetWindowPositions.exe");
            return new List<LoadedTools>
            {
                new LoadedTools {ID = Guid.NewGuid(), Name = "Set Windows Positions", FileLocation = fileLocation, HotKey = "W"}
            };
        }

        public static void SaveCurrentTool(LoadedSettings loadedSettings, ListBox toolsListBox, ToolStruct tool)
        {
            var currentTool = loadedSettings.Tools.First(t => t.ID == tool.ID);

            if (tool.ID == Guid.Empty) return;

            if (currentTool.Name != tool.Name)
                currentTool.Name = tool.Name;
            if (currentTool.FileLocation != tool.FileLocation)
                currentTool.FileLocation = tool.FileLocation;
            if (currentTool.HotKey != tool.HotKey)
                currentTool.HotKey = tool.HotKey;

            ListboxUtils.RepopulateListBox(false, toolsListBox, loadedSettings, tool.ID);
            ListboxUtils.SetCurrentOrderFromListBoxAndSave(false, toolsListBox, loadedSettings);

            MessageBox.Show($"{currentTool.Name} {Constants.Messages.SavedSuccessfully}",
                            $"Tool {Constants.Messages.SavedSuccessfullyCaption}",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
