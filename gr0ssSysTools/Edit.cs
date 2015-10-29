﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using gr0ssSysTools.FileUtils;
using gr0ssSysTools.Properties;
using gr0ssSysTools.Utils;
using Microsoft.Win32;

namespace gr0ssSysTools
{
    public partial class Edit : Form
    {
        private Settings _settings;

        public Edit()
        {
            InitializeComponent();
        }

        public Edit(Settings settings, bool env)
        {
            InitializeComponent();
            
            _settings = settings;

            tabControl.SelectedTab = env ? tabEnvironments : tabTools;
        }

        private void Edit_Load(object sender, EventArgs e)
        {
            SetupButtonImages();
                        
            if (tabControl.SelectedTab == tabEnvironments)
            {
                environmentsList.Items.Clear();
                RepopulateSelectedTabsListbox(tabControl.SelectedTab == tabEnvironments);
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabGeneral)
            {
                SetupButtonEnabled(false);
                RegistryKeyMethods.PopulateRootCombo(rootCombo);
            }
            else
            {
                SetupButtonEnabled(true);
                RepopulateSelectedTabsListbox(tabControl.SelectedTab == tabEnvironments);
            }
        }

#region Setup and Populate

        private void RepopulateSelectedTabsListbox(bool env)
        {
            if (env)
            {
                environmentsList.Items.Clear();

                foreach (var key in _settings.Environments)
                {
                    environmentsList.Items.Add(key.Name);
                }
            }
            else
            {
                toolsList.Items.Clear();

                foreach (var key in _settings.Tools)
                {
                    toolsList.Items.Add(key.Name);
                }
            }
        }
        
        private void SetupButtonEnabled(bool enabled)
        {
            /* Save button is always enabled. All other *
             * buttons are dependent on the current tab */
            addButton.Enabled = enabled;
            removeButton.Enabled = enabled;
            moveUpButton.Enabled = enabled;
            moveDownButton.Enabled = enabled;
        }
        
        private void SetupButtonImages()
        {
            addButton.Image = Resources.Add;
            removeButton.Image = Resources.Delete;
            saveButton.Image = Resources.Save;

            var arrowUpPicture = Resources.Move_Arrow;
            arrowUpPicture.RotateFlip(RotateFlipType.Rotate90FlipY);
            moveUpButton.Image = arrowUpPicture;

            var arrowDownPicture = Resources.Move_Arrow;
            arrowDownPicture.RotateFlip(RotateFlipType.Rotate270FlipY);
            moveDownButton.Image = arrowDownPicture;
        }

        private void PopulateHotkeyCombo()
        {
            if (tabControl.SelectedTab == tabEnvironments)
            {
                hotkeyCombo.Items.Clear();
                foreach (var c in NameTextbox.Text.Where(c => c.ToString() != " "))
                    hotkeyCombo.Items.Add(c);
            }
            else if (tabControl.SelectedTab == tabTools)
            {
                hotkeyToolsCombo.Items.Clear();
                foreach (var c in toolsNameTextbox.Text.Where(c => c.ToString() != " "))
                    hotkeyToolsCombo.Items.Add(c);
            }
        }

        private void PopulateIconColorCombo()
        {
            iconColorCombo.Items.Clear();
            
            iconColorCombo.Items.Add(new ColorDropDownItem("Dark Gray", Brushes.DarkGray));
            iconColorCombo.Items.Add(new ColorDropDownItem("Blue", Brushes.Blue));
            iconColorCombo.Items.Add(new ColorDropDownItem("Brown", Brushes.Brown));
            iconColorCombo.Items.Add(new ColorDropDownItem("Coral", Brushes.Coral));
            iconColorCombo.Items.Add(new ColorDropDownItem("Red", Brushes.Red));
            iconColorCombo.Items.Add(new ColorDropDownItem("Aqua", Brushes.Aqua));
            iconColorCombo.Items.Add(new ColorDropDownItem("Violet", Brushes.Violet));
            iconColorCombo.Items.Add(new ColorDropDownItem("Green", Brushes.Green));
            iconColorCombo.Items.Add(new ColorDropDownItem("Yellow", Brushes.Yellow));
            iconColorCombo.Items.Add(new ColorDropDownItem("Magenta", Brushes.Magenta));
            iconColorCombo.Items.Add(new ColorDropDownItem("Wheat", Brushes.Wheat));
            iconColorCombo.Items.Add(new ColorDropDownItem("Orange", Brushes.Orange));
        }

        private void NameTextbox_Leave(object sender, EventArgs e)
        {
            PopulateHotkeyCombo();
        }
#endregion Setup and Populate

        private void addButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabEnvironments)
            {
                var addName = new AddName(true, _settings);
                addName.Closing += NameAdded_EventHandler;
                addName.Show();
            }
            else if (tabControl.SelectedTab == tabTools)
            {
                var addTextName = new AddName(false, _settings);
                addTextName.Closing += NameAdded_EventHandler;
                addTextName.Show();
            }
        }

        private void NameAdded_EventHandler(object sender, CancelEventArgs e)
        {
            RepopulateSelectedTabsListbox(tabControl.SelectedTab == tabEnvironments);

            if (tabControl.SelectedTab == tabEnvironments)
            {
                var index = environmentsList.Items.Count - 1;
                environmentsList.SelectedIndex = index;
            }
            else if (tabControl.SelectedTab == tabTools)
            {
                var toolIndex = toolsList.Items.Count - 1;
                toolsList.SelectedIndex = toolIndex;
            }

            // Remove Event handler
            //Events.Dispose();
        }
        
        private void removeButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabEnvironments)
            {
                if (environmentsList.SelectedIndex != -1)
                {
                    var allEnvironments = _settings.Environments;
                    var environmentToRemove = allEnvironments.FirstOrDefault(env => env.ID == Guid.Parse(guidLabel.Text));
                    if (environmentToRemove.ID != Guid.Empty)
                    {
                        allEnvironments.Remove(environmentToRemove);
                        _settings.Environments = allEnvironments;
                        ClearEnvironmentFields();
                    }
                    else
                        MessageBox.Show("Error retrieving environment", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("You need to select an environment before you can delete one...", "Duh", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
            else if (tabControl.SelectedTab == tabTools)
            {
                if (toolsList.SelectedIndex != -1)
                {
                    var allTools = _settings.Tools;
                    var toolToRemove = allTools.FirstOrDefault(tool => tool.ID == Guid.Parse(guidToolsLabel.Text));
                    if (toolToRemove != null && toolToRemove.ID != Guid.Empty)
                    {
                        allTools.Remove(toolToRemove);
                        _settings.Tools = allTools;
                        ClearToolFields();
                    }
                    else
                        MessageBox.Show("Error retrieving tool", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("You need to select a tool before you can delete one...", "Duh", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            RepopulateSelectedTabsListbox(tabControl.SelectedTab == tabEnvironments);
        }

#region Clear Methods
        private void ClearEnvironmentFields()
        {
            NameTextbox.Text = "";
            registryValueTextbox.Text = "";
            hotkeyCombo.Items.Clear();
            hotkeyCombo.Text = "";
            iconDisplayTextbox.Text = "";
            iconColorCombo.SelectedIndex = -1;
            guidLabel.Text = "";
        }

        private void ClearToolFields()
        {
            toolsNameTextbox.Text = "";
            DirectoryPathTextbox.Text = "";
            hotkeyToolsCombo.Items.Clear();
            hotkeyToolsCombo.Text = "";
            guidToolsLabel.Text = "";
        }
#endregion Clear Methods

#region Save button
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabGeneral)
                SaveNewRegistryKey();
            else
            {
                if (tabControl.SelectedTab == tabEnvironments)
                    SetCurrentOrderOfEnvironments();
                else if (tabControl.SelectedTab == tabTools)
                    SetCurrentOrderOfTools();
            }
        }

        private void SetCurrentOrderOfEnvironments()
        {
            var environmentsOrdered = (from object item 
                                       in environmentsList.Items
                                       select _settings.Environments.FirstOrDefault(env => env.Name == item.ToString()))
                                       .ToList();
            _settings.Environments = environmentsOrdered;
        }

        private void SetCurrentOrderOfTools()
        {
            var toolsOrdered = (from object item 
                                in toolsList.Items
                                select _settings.Tools.FirstOrDefault(tool => tool.Name == item.ToString()))
                                .ToList();
            _settings.Tools = toolsOrdered;
        }
#endregion Save button

#region Move buttons
        private void moveUpButton_Click(object sender, EventArgs e)
        {
            TurnOffListEventHandlers();
            MoveItem(-1);
            TurnOnListEventHandlers();
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            TurnOffListEventHandlers();
            MoveItem(1);
            TurnOnListEventHandlers();
        }

        private void environmentsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (environmentsList.SelectedIndex != -1)
            {
                var curItem = environmentsList.SelectedItem.ToString();
                var itemToLoad = _settings.Environments.FirstOrDefault(env => env.Name == curItem);

                guidLabel.Text = itemToLoad.ID.ToString();
                NameTextbox.Text = curItem;
                registryValueTextbox.Text = itemToLoad.SubkeyValue;

                PopulateHotkeyCombo();
                hotkeyCombo.SelectedIndex = MiscUtils.GetIndexOfHotkey(itemToLoad.Name, itemToLoad.HotKey);

                iconDisplayTextbox.Text = itemToLoad.IconLabel;

                PopulateIconColorCombo();
                
                var colorIndex = MiscUtils.GetColorIndex(itemToLoad.IconColor);
                iconColorCombo.SelectedItem = iconColorCombo.Items[colorIndex];
            }
        }

        private void toolsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolsList.SelectedIndex != -1)
            {
                var curItem = toolsList.SelectedItem.ToString();
                var itemToLoad = _settings.Tools.FirstOrDefault(tool => tool.Name == curItem);

                guidToolsLabel.Text = itemToLoad.ID.ToString();
                toolsNameTextbox.Text = curItem;
                DirectoryPathTextbox.Text = itemToLoad.FileLocation;

                PopulateHotkeyCombo();
                hotkeyToolsCombo.SelectedIndex = MiscUtils.GetIndexOfHotkey(itemToLoad.Name, itemToLoad.HotKey);
            }
        }

        private void TurnOffListEventHandlers()
        {
            environmentsList.SelectedIndexChanged -= environmentsList_SelectedIndexChanged;
            toolsList.SelectedIndexChanged -= toolsList_SelectedIndexChanged;
        }

        private void TurnOnListEventHandlers()
        {
            environmentsList.SelectedIndexChanged += environmentsList_SelectedIndexChanged;
            toolsList.SelectedIndexChanged += toolsList_SelectedIndexChanged;
        }

        private void MoveItem(int direction)
         {
            if (tabControl.SelectedTab == tabEnvironments)
            {
                // Checking selected item
                if (environmentsList.SelectedItem == null || environmentsList.SelectedIndex < 0)
                    return; // No selected item - nothing to do

                // Calculate new index using move direction
                int newIndex = environmentsList.SelectedIndex + direction;

                // Checking bounds of the range
                if (newIndex < 0 || newIndex >= environmentsList.Items.Count)
                    return; // Index out of range - nothing to do

                object selected = environmentsList.SelectedItem;

                // Removing removable element
                environmentsList.Items.Remove(selected);
                // Insert it in new position
                environmentsList.Items.Insert(newIndex, selected);
                // Restore selection
                environmentsList.SetSelected(newIndex, true);
                // Save the new order
                SetCurrentOrderOfEnvironments();
            }
            else if (tabControl.SelectedTab == tabTools)
            {
                // Checking selected item
                if (toolsList.SelectedItem == null || toolsList.SelectedIndex < 0)
                    return; // No selected item - nothing to do

                // Calculate new index using move direction
                int newIndex = toolsList.SelectedIndex + direction;

                // Checking bounds of the range
                if (newIndex < 0 || newIndex >= toolsList.Items.Count)
                    return; // Index out of range - nothing to do

                object selected = toolsList.SelectedItem;

                // Removing removable element
                toolsList.Items.Remove(selected);
                // Insert it in new position
                toolsList.Items.Insert(newIndex, selected);
                // Restore selection
                toolsList.SetSelected(newIndex, true);
                // Save the new order
                SetCurrentOrderOfTools();
            }
        }
#endregion Move buttons

#region Registry Key Methods
        private void checkButton_Click(object sender, EventArgs e)
        {
            var rootValue = RegistryKeyMethods.GetCurrentRoot(rootCombo, rootCombo2, rootCombo3);

            MessageBox.Show($"The current registry key selected is:\n{rootValue}\\{fieldTextBox.Text}\n\nIt has a value of:\n{GetCurrentKeyValue()}", @"Current Value of Key",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveNewRegistryKey()
        {
            if (GetCurrentKeyValue() == string.Empty)
                MessageBox.Show("You must first select a valid key.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            else
            {
                var confirmMessage = MessageBox.Show("If we save the new registry key, you must restart the program for the new key to take effect.\nAre you sure this is what you want to do?", 
                    "Continue With Save", MessageBoxButtons.YesNo, MessageBoxIcon.Hand);

                if (confirmMessage != DialogResult.Yes) return;

                var newRegistryKey = new Files.RegistryKey
                {
                    Root = RegistryKeyMethods.GetCurrentRoot(rootCombo, rootCombo2, rootCombo3),
                    Subkey = fieldTextBox.Text
                };
                _settings.RegistryKey = newRegistryKey;
            }
        }

        private void RootCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            rootCombo2.Items.Clear();
            rootCombo2.Text = "";
            rootCombo3.Items.Clear();
            rootCombo3.Text = "";
            RegistryKeyMethods.PopulateRootCombo2(rootCombo, rootCombo2);
        }

        private void RootCombo2_SelectedIndexChanged(object sender, EventArgs e)
        {
            rootCombo3.Items.Clear();
            rootCombo3.Text = "";

            RegistryKeyMethods.PopulateRootCombo3(rootCombo, rootCombo2, rootCombo3);
        }

        private string GetCurrentKeyValue()
        {
            return (string) Registry.GetValue(RegistryKeyMethods.GetCurrentRoot(rootCombo, rootCombo2, rootCombo3).ToString(), fieldTextBox.Text, "");
        }
#endregion Registry Key Methods

        
    }
}
