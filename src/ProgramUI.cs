using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.ColorPicker;

namespace tasks;

public partial class TasksProgram
{
    VerticalStackPanel rootContainer;
    const string tablesPath = "tables/";

    void CreateUI()
    {
        rootContainer = new();

        HorizontalStackPanel fillerPanel = new() {
            Width = Screen.X,
            Height = Screen.Y - bottomBarRect.Height,
        };

        HorizontalStackPanel bottomPanel = new() {
            Width = bottomBarRect.Width - cardBinRect.Width,
            Height = bottomBarRect.Height,
        };

        TextButton saveButton, loadButton, generateButton;
        Point btnSize = new(200,50);

        saveButton = new() {
            Text = "Save",
            Width = btnSize.X,
            Height = btnSize.Y,
        };
        loadButton = new() {
            Text = "Load",
            Width = btnSize.X,
            Height = btnSize.Y,
        };
        generateButton = new() {
            Text = "Generate",
            Width = btnSize.X,
            Height = btnSize.Y,
        };

        saveButton.Click += (s, e) => CreateFileDialog("Save", "Save as", Save, "Failed to save file: ");
        loadButton.Click += (s, e) => CreateLoadDialog();
        generateButton.Click += (s, e) => GenerateRandomCards();
        
        bottomPanel.AddChild(saveButton);
        bottomPanel.AddChild(loadButton);
        bottomPanel.AddChild(generateButton);

        rootContainer.AddChild(fillerPanel);
        rootContainer.AddChild(bottomPanel);

        desktop.Root = rootContainer;
    }

    protected void CreateFileDialog(string title, string labelText, Func<string, bool> handler, string failedMessage)
    {
        VerticalStackPanel panel = new();
        TextBox tb;
        Label label;

        tb = new() {
            HintText = "Enter path",
            TextColor = Color.White
        };

        label = new() {
            Text = labelText,
            TextColor = Color.White
        };

        panel.Widgets.Add(label);
        panel.Widgets.Add(tb);

        var click = (string text) => {
            bool success = handler.Invoke(text);

            if(!success) {
                Dialog dialog = Dialog.CreateMessageBox("Error", failedMessage + text);
                dialog.ShowModal(desktop);
            }
        };

        Dialog dialog = Dialog.CreateMessageBox(title, panel);
        dialog.ButtonOk.Click += (s, e) => click.Invoke(tb.Text);
        dialog.ShowModal(desktop);
    }

    void CreateLoadDialog()
    {
        VerticalStackPanel panel = new();

        DirectoryInfo tablesDir = new(tablesPath);

        foreach(FileInfo file in tablesDir.GetFiles())
        {
            TextButton button = new() {
                Text = file.Name
            };

            button.Click += (s, e) => Load(file.Name);
            panel.Widgets.Add(button);
        }

        Dialog dialog = Dialog.CreateMessageBox("Load", panel);
        dialog.ButtonOk.Click += (s, e) => {};
        dialog.ShowModal(desktop);
    }

    public void CreateColorWheelDialog(UICard card)
    {
        VerticalStackPanel panel = new();
        TextBox tb;
        Label label;

        tb = new() {
            HintText = "Enter color",
            TextColor = Color.White
        };

        label = new() {
            Text = "Color (X,X,X):",
            TextColor = Color.White
        };

        panel.Widgets.Add(label);
        panel.Widgets.Add(tb);

        var onAccept = (UICard card, object obj) => 
        {
            if(obj is Dialog dialog) {
                card.UpdateColor((dialog.Content as ColorPickerPanel).Color);
            }

            //Needs regex
            //byte[] rgb = strColor.Split(",").Select(val => byte.Parse(val)).ToArray();
            //Color color = new(rgb[0], rgb[1], rgb[2]);
            //card.UpdateColor(colorDialog.Color);
        };

        //Dialog dialog = Dialog.CreateMessageBox("Color", panel);
        var dialog = Dialog.CreateMessageBox("Choose card color", new ColorPickerPanel());
        //dialog.Title = "Choose card color";
        //dialog.Content = new ColorPickerPanel();
        dialog.ButtonOk.Click += (s, e) => onAccept.Invoke(card, dialog);
        dialog.ShowModal(desktop);
    }
}