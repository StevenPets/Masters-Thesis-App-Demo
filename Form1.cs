using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace AmenoSubaruboshinomikoto
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// 
        /// =======================================================================================
        /// ==========================================   ==========================================
        /// ========================================       ========================================
        /// ======================================   NOTES   ======================================
        /// ========================================       ========================================
        /// ==========================================   ==========================================
        /// =======================================================================================
        /// 
        /// 1) Finished.
        ///                                 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// 
        /// </summary>





        //========================================================================================//
        //========================================================================================//
        //=============================                              =============================//
        //=============================   DELCERATION OF VARIABLES   =============================//
        //=============================                              =============================//
        //========================================================================================//
        //========================================================================================//



        Bitmap img; //==== the image to be proccessed ====//
        Bitmap imgProcessed; //==== the resulting image after an image processing function is excecuted ====//
        Bitmap imgProcessedClone = null; //==== the proccesed image clone, used in brightness/contrast adjustments ====//
        Bitmap imgClone; //==== the image clone, used on the Reset Image Button and in brightness/contrast adjustments ====//

        int mousePosX; //==== the X coordinate of the cursor's starting position ====//
        int mousePosY; //==== the Y coordinate of the cursor's starting position ====//

        bool fileDropDownCheck; //==== variable that checks whether or not the drop down menu of the "File" Menu Strip Item is open ====//
        bool editDropDownCheck; //==== variable that checks whether or not the drop down menu of the "Processing" Menu Strip Item is open ====//
        bool helpDropDownCheck; //==== variable that checks whether or not the drop down menu of the "Help" Menu Strip Item is open ====//

        bool fileMouseCheck; //==== variable that determines whether or not the mouse cursor is on the "File" Menu Strip Item ====//
        bool editMouseCheck; //==== variable that determines whether or not the mouse cursor is on the "Processing" Menu Strip Item ====//
        bool helpMouseCheck; //==== variable that determines whether or not the mouse cursor is on the "Help" Menu Strip Item ====//

        bool cancelNumericUD_ResetButton; //==== variable that checks if the reset button is clicked ====//
        bool cancelTrackBar_ValueChanged; //==== variable that stops unnecessary loops between numericUD and trackbar events ====//

        int imgWidth; //==== declaration of the width of the resized image ====//
        int imgHeight; //==== decleration of the height of the resized image ====//
        double zoomFactor = 0; //==== decleration of the zoom factor (keeps track of what zoom "step" the image is in) ====//
        double zoomScale; //==== decleration of the actual zoom in/zoom out value ====//
        int zoomState = 0; //==== decleration of the variable that defines the state of zooming (0 = zoom out, 1 = zoom in)
        int imvalMax; //==== decleration of the variable that represents the maximum tone of gray of the image ====//
        int imvalMin; //==== decleration of the variable that represents the minimum tone of gray of the image ====//







        //=======================================================================================//
        //=======================================================================================//
        //====================================               ====================================//
        //====================================   MAIN FORM   ====================================//
        //====================================               ====================================//
        //=======================================================================================//
        //=======================================================================================//



        public Form1()
        {
            InitializeComponent();
        }

        //
        // ==== Load an image through the Open tool strip item ====
        //
        public void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            OpenFileDialog ofd = new OpenFileDialog
            {
                //==== image filters ====//
                Filter = "Images|*.png; *.bmp; *.jpeg; *.jpg"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (PictureBox.Image != null)
                {
                    //==== dispose FH and the previous image in the picture box & ready the forms for the next image ====//
                    PictureBox.Image.Dispose();

                    //==== nullify the previous processed image ====//
                    imgProcessed = null;
                    imgProcessedClone = null;

                    //==== reset controls for the new image ====//
                    zoomFactor = 0;
                    zoomState = 0;
                    TextBox_ZoomFactor.Text = "Zoom Factor: x1";

                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    NumericUpDown_Brightness.Value = 0;
                    NumericUpDown_Contrast.Value = 0;

                    TrackBar_Brightness.Value = 5;
                    TrackBar_Contrast.Value = 5;

                    PictureBox.Location = new Point(0, 0);
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                }

                //==== open image using filestream (helps with saving files later) ====// 
                using (FileStream imgFs = new FileStream(ofd.FileName, FileMode.Open))
                {
                    //==== open and display image in picture box ====//
                    PictureBox.Image = Image.FromStream(imgFs);
                    imgFs.Close();//==== close the file stream, (helps with saving the file) ====//
                }

                //==== Bitmap to be used for processing, converted to Grayscale ====//
                img = ToGrayscale((Bitmap)PictureBox.Image);

                //==== Save imvalMax and imvalMin for later use in image processing ====//
                var (minPixel, maxPixel) = MinMaxPixel(img);
                imvalMax = maxPixel;
                imvalMin = minPixel;

                //==== create image clone for Reset Image button ====//
                imgClone = new Bitmap(img);

                //==== enable controls ====//
                TrackBar_Brightness.Enabled = true;
                TrackBar_Contrast.Enabled = true;
                NumericUpDown_Brightness.Enabled = true;
                NumericUpDown_Contrast.Enabled = true;
                Button_ZoomIn.Enabled = true;
                Button_ResetBrightnessContrast.Enabled = true;
                Button_ClearImage.Enabled = true;
                Button_ResetImage.Enabled = true;

                //==== enable Processes ====//
                ProcessingToolStripMenuItem.Enabled = true;

                //==== hide "Click to Open File" & "Move Image..." Text Boxes ====//
                MaskedTextBox_OpenFile.Visible = false;
                MaskedTextBox_MoveImage.Visible = false;

                //==== zoom factor text box ====//
                TextBox_ZoomFactor.Text = "Zoom Factor: x1";

                //==== image file path ====//
                TextBox_FilePath_Var.Text = ofd.FileName;

                //==== image size (width x height) ====//
                FileInfo file = new FileInfo(ofd.FileName);
                string imHeight = Convert.ToString(img.Height);
                string imWidth = Convert.ToString(img.Width);
                TextBox_ImageDimenstions_Var.Text = imWidth + " x " + imHeight;

                //==== image size (in KB) text box ====//
                TextBox_FileSize_Var.Text = Convert.ToString(file.Length / 1024) + " KB";

                //==== image name text box ====//              
                TextBox_FileName_Var.Text = Path.GetFileNameWithoutExtension(ofd.FileName);

                //==== image type (extension) text box ====//
                TextBox_FileType_Var.Text = Path.GetExtension(ofd.FileName);

                //==== loads the dimensions of the image after the aspect ratio check ====//
                var imgDimensions = AspectRatioCheck();

                //==== save image width & height for later use ====//
                imgWidth = imgDimensions.Width; imgHeight = imgDimensions.Height;

                //==== resize image to fit picture box and place it in the center ====//
                PictureBox.Image = ResizeImage(img, imgDimensions.Width, imgDimensions.Height);
                PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

            }
            else
            {
                //==== indicate background processes stopped running ====//
                ToolStripStatusLabel_ReadyBusy.Text = "Ready";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
                return;
            }
            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;

        }

        //
        // ==== Save the image in the picture box ====
        //
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            this.BackgroundWorker.RunWorkerAsync();
            while (this.BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            if (PictureBox.Image != null)
            {
                //==== initate save image process, define saving parameters ====//
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Images|*.png; *.bmp; *.jpeg; *.jpg";
                ImageFormat imgFormat = ImageFormat.Png;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    //==== get the extension of the file ====//
                    string extension = Path.GetExtension(sfd.FileName);

                    //==== set the extension of the file to be saved the same as the file that was opened ====//
                    switch (extension)
                    {
                        case ".png":
                            imgFormat = ImageFormat.Png;
                            break;
                        case ".jpeg":
                            imgFormat = ImageFormat.Jpeg;
                            break;
                        case ".jpg":
                            imgFormat = ImageFormat.Jpeg;
                            break;
                        case ".bmp":
                            imgFormat = ImageFormat.Bmp;
                            break;
                    }

                    if (imgProcessed != null)
                    {
                        //==== save image ====//
                        imgProcessed.Save(sfd.FileName, imgFormat);
                    }
                    else
                    {
                        //==== save image ====//
                        img.Save(sfd.FileName, imgFormat);
                    }
                }
                else
                {
                    //==== indicate background processes stopped running ====//
                    ToolStripStatusLabel_ReadyBusy.Text = "Ready";
                    ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
                    return;
                }
            }
            else
            {
                MessageBox.Show("No Image found to Save." +
                    " Click on the Image Box or use the Open File option to load an Image");

            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Events after left-clicking the picture box (either load a image OR zoom in/out existing image) ====
        //
        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {

            //==== Checks if there is an image loaded and if the Ctrl key is held ====//
            if (PictureBox.Image != null && ModifierKeys == Keys.Control)
            {

                //==== zoom in with Ctrl + Left Click ====//
                if (e.Button == MouseButtons.Left && zoomFactor == 0)
                {
                    Button_ZoomIn.PerformClick();
                }

                //==== zoom out with Ctrl + Right Click ====//
                else if (e.Button == MouseButtons.Right && zoomState == 1)
                {
                    Button_ZoomOut.PerformClick();
                }

                //==== calculates cursor's position to begin moving the picture box containing the image ====//
                else if (e.Button == MouseButtons.Middle)
                {
                    mousePosX = e.X; mousePosY = e.Y;
                }
            }

            //==== Loads file by clicking the picture box ====//
            else if (PictureBox.Image == null && e.Button == MouseButtons.Left)
            {
                OpenToolStripMenuItem.PerformClick();
            }
        }


        //
        // ==== Simulates the movement of the image by moving the picture box it is in ====
        //
        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            Control c = sender as Control;
            if (ModifierKeys == Keys.Control && e.Button == MouseButtons.Middle)
            {
                c.Top = e.Y + c.Top - mousePosY;
                c.Left = e.X + c.Left - mousePosX;
            }
        }


        //
        // ==== Close app with the "Exit" Menu Strip Item under "File" ====
        //
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        //
        // ==== Keyboard Shortcuts ====
        //
        private void Form_KeyUp(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                Button_ClearImage.PerformClick();
            }
            else if (e.KeyCode == Keys.R)
            {
                Button_ResetImage.PerformClick();
            }
        }


        //
        // ==== Shows the directory path of the file through a tool tip in case it doesn't fit in the text box ====
        //
        public void TextBoxFilePath_MouseHover(object sender, EventArgs e)
        {
            ToolTip.SetToolTip(TextBox_FilePath_Var, TextBox_FilePath_Var.Text);
        }


        //
        // ==== Zoom in Button ====
        //
        private void ButtonZoomIn_Click(object sender, EventArgs e)
        {
            //==== controls the availability of the zoom buttons ====//
            if (!Button_ZoomOut.Enabled) { Button_ZoomOut.Enabled = true; }
            if (zoomFactor == 0) { Button_ZoomIn.Enabled = false; }

            zoomState = 1; //==== sets state to zoom in ====//
            zoomFactor++;

            //==== methods that calculate Size, Dimentions and Location parameters after zooming in ====//
            PictureBox.Size = ZoomPictureBox(zoomFactor);
            PictureBox.Location = PlacePictureBox(zoomState);
            PictureBox.Image = ZoomImage(zoomFactor);

            //==== display the zoom factor in the zoom factor text box ====//
            TextBox_ZoomFactor.Text = "Zoom Factor: x" + Convert.ToString(zoomScale);
        }


        //
        // ==== Zoom out Button ====
        //
        private void ButtonZoomOut_Click(object sender, EventArgs e)
        {
            //==== controls the availability of the zoom buttons ====//
            if (!Button_ZoomIn.Enabled) { Button_ZoomIn.Enabled = true; }
            if (zoomFactor == 1) { Button_ZoomOut.Enabled = false; }

            zoomState = 0; //==== sets state to zoom out ====//
            zoomFactor--;

            //==== methods that calculate Size, Dimentions and Location parameters after zooming out ====//
            PictureBox.Size = ZoomPictureBox(zoomFactor);
            PictureBox.Location = PlacePictureBox(zoomState);
            PictureBox.Refresh(); //==== refresh at this specific point is essential. Smooths out the zooming out of the image ====//
            PictureBox.Image = ZoomImage(zoomFactor);

            //==== display the zoom factor in the zoom factor text box ====//
            TextBox_ZoomFactor.Text = "Zoom Factor: x" + Convert.ToString(zoomScale);
        }


        //
        // ==== Tool Tip for the Zoom In Button ====
        //
        private void ButtonZoomIn_MouseHover(object sender, EventArgs e)
        {
            ToolTip.SetToolTip(Button_ZoomIn, "Zoom in Shortcut (CTRL + Left Click)");
        }


        //
        // ==== Tool Tip for the Zoom Out Button ====
        //
        private void ButtonZoomOut_MouseHover(object sender, EventArgs e)
        {
            ToolTip.SetToolTip(Button_ZoomOut, "Zoom in Shortcut (CTRL + Right Click)");
        }


        //
        // ==== Resets the image in the picture box to its original values ====
        //
        private void ButtonResetImage_Click(object sender, EventArgs e)
        {
            //==== nullify imgPorcessed & imgProcessedClone discarding all changes that happened with any process ====//
            imgProcessed = null;
            imgProcessedClone = null;

            //==== cancel numeric up/down event ====//
            cancelNumericUD_ResetButton = true;

            //==== reset size of picture box, relocate it to be centered and replace image with the a cloned image of the original ====//
            PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
            PictureBox.Location = new Point(0, 0);
            PictureBox.Image = imgClone;

            if (PictureBox.Image != null)
            {
                //==== loads the dimensions of the image after the aspect ratio check ====// 
                var imgDimensions = AspectRatioCheck();

                //==== save image width & height for later use ====//
                imgWidth = imgDimensions.Width; imgHeight = imgDimensions.Height;

                //==== resize image to fit picture box and place it in the center ====//
                PictureBox.Image = ResizeImage(imgClone, imgDimensions.Width, imgDimensions.Height);
                PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            }

            //==== reset brightness/contrast values ====//
            TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;

            //==== resets the zoom variables and enables the zoom buttons ====//
            zoomFactor = 0; zoomScale = 1; zoomState = 0;
            TextBox_ZoomFactor.Text = "Zoom Factor: x1";
            Button_ZoomIn.Enabled = true;
            Button_ZoomOut.Enabled = false;

            //==== resume numeric up/down event ====//
            cancelNumericUD_ResetButton = false;
        }


        //
        // ==== Removes the image currently in the picture box ====
        //
        private void ButtonClearImage_Click(object sender, EventArgs e)
        {
            //==== cancel numeric up/down event ====//
            cancelNumericUD_ResetButton = true;

            //==== currently loaded image and all instances of it ====//
            PictureBox.Image.Dispose();
            PictureBox.Image = null;
            imgClone = null;
            img = null;
            imgProcessedClone = null;
            imgProcessed = null;

            //==== show "Click to Open File" & "Move Image..." Text Boxes ====//
            MaskedTextBox_OpenFile.Visible = true;
            MaskedTextBox_MoveImage.Visible = true;

            //==== resets all file info text-boxes back to empty ====//
            TextBox_ImageDimenstions_Var.Text = null;
            TextBox_FilePath_Var.Text = null;
            TextBox_FileSize_Var.Text = null;
            TextBox_FileName_Var.Text = null;
            TextBox_FileType_Var.Text = null;
            TextBox_ZoomFactor.Text = "Zoom Factor: --";

            //==== resets values ====//
            NumericUpDown_Brightness.Value = 0;
            TrackBar_Brightness.Value = 5;
            NumericUpDown_Contrast.Value = 0;
            TrackBar_Contrast.Value = 5;
            zoomFactor = 0;
            zoomScale = 1;

            //==== disable controls ====//
            TrackBar_Brightness.Enabled = false;
            TrackBar_Contrast.Enabled = false;
            NumericUpDown_Brightness.Enabled = false;
            NumericUpDown_Contrast.Enabled = false;
            Button_ZoomIn.Enabled = false;
            Button_ResetBrightnessContrast.Enabled = false;
            Button_ClearImage.Enabled = false;
            Button_ResetImage.Enabled = false;
            Button_ZoomIn.Enabled = false;
            Button_ZoomOut.Enabled = false;

            //==== disable all Processes ====//
            ProcessingToolStripMenuItem.Enabled = false;

            //==== resets the size and position of the picture box ====//
            PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
            PictureBox.Location = new Point(0, 0);

            //==== resume numeric up/down event ====//
            cancelNumericUD_ResetButton = false;
        }


        //
        // ==== Resets the brightness and contrast values ====
        //
        private void ButtonResetBrightnessContrast_Click(object sender, EventArgs e)
        {
            TrackBar_Contrast.Value = 5; TrackBar_Brightness.Value = 5;
        }


        //
        // ==== Controls the contrast value through the track bar ====
        //
        private void TrackBar_Contrast_ValueChanged(object sender, EventArgs e)
        {
            if (cancelTrackBar_ValueChanged)
            {
                return;
            }
            else
            {
                NumericUpDown_Contrast.Value = (TrackBar_Contrast.Value - 5) * 20;
            }
        }

        //
        // ==== Controls the slider's distance traveled per wheel scroll (curently one slider position per wheel scroll) ====
        //
        private void TrackBar_Contrast_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true; //==== disable default mouse wheel ====//
            if (e.Delta > 0)
            {
                if (TrackBar_Contrast.Value < TrackBar_Contrast.Maximum)
                {
                    TrackBar_Contrast.Value++;
                }
            }
            else
            {
                if (TrackBar_Contrast.Value > TrackBar_Contrast.Minimum)
                {
                    TrackBar_Contrast.Value--;
                }
            }
        }


        //
        // ==== Controls the contrast value through the numeric up/down ====
        //
        public void NumericUpDown_Contrast_ValueChanged(object sender, EventArgs e)
        {
            //==== stop the trackbar event from running ====//
            cancelTrackBar_ValueChanged = true;

            //==== change the trackbar's value accordingly ====//
            TrackBar_Contrast.Value = (int)Math.Floor(NumericUpDown_Contrast.Value / 20 + 5);

            //==== reset trackbar event handler's flow ====//
            cancelTrackBar_ValueChanged = false;

            if (cancelNumericUD_ResetButton)
            {
                return;
            }
            else
            {
                //==== adjust Contrast ====//
                img = AdjustBrightnessContrast(
                    imgClone,
                    imgProcessedClone,
                    (float)NumericUpDown_Brightness.Value,
                    (float)NumericUpDown_Contrast.Value);

                //==== calculate image and draw image seperatly bacause otherwise image drops in resolution during zoom in/out ====//
                PictureBox.Image = img;

                //==== loads the dimensions of the image after the aspect ratio check ====// 
                var imgDimensions = AspectRatioCheck();

                //==== resize image to fit picture box and place it in the center ====//
                PictureBox.Image = ResizeImage(img, imgDimensions.Width, imgDimensions.Height);
                PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }


        //
        // ==== Controls the brightness value through the track bar  ====
        //
        private void TrackBar_Brightness_ValueChanged(object sender, EventArgs e)
        {
            if (cancelTrackBar_ValueChanged)
            {
                return;
            }
            else
            {
                NumericUpDown_Brightness.Value = (TrackBar_Brightness.Value - 5) * 20;
            }
        }


        //
        // ==== Controls the slider's distance traveled per wheel scroll (curently one slider position per wheel scroll) ====
        //
        private void TrackBar_Brightness_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true; //==== disable default mouse wheel ====//
            if (e.Delta > 0)
            {
                if (TrackBar_Brightness.Value < TrackBar_Brightness.Maximum)
                {
                    TrackBar_Brightness.Value++;
                }
            }
            else
            {
                if (TrackBar_Brightness.Value > TrackBar_Brightness.Minimum)
                {
                    TrackBar_Brightness.Value--;
                }
            }
        }

        //
        // ==== Controls the brightness value through the numeric up/down  ====
        //
        public void NumericUpDown_Brightness_ValueChanged(object sender, EventArgs e)
        {
            //==== stop the trackbar event from running ====//
            cancelTrackBar_ValueChanged = true;

            //==== change the trackbar's value accordingly ====//
            TrackBar_Brightness.Value = (int)Math.Floor(NumericUpDown_Brightness.Value / 20 + 5);

            //==== reset trackbar event handler flow ====//
            cancelTrackBar_ValueChanged = false;

            if (cancelNumericUD_ResetButton)
            {
                return; //==== cancels this event if the numeric up/down's values are reset (through the reset image button) ====//
            }
            else
            {
                //==== adjust Brightness ====//
                img = AdjustBrightnessContrast(
                    imgClone,
                    imgProcessedClone,
                    (float)NumericUpDown_Brightness.Value,
                    (float)NumericUpDown_Contrast.Value);

                //==== calculate image and draw image seperatly bacause otherwise image drops in resolution during zoom in/out ====//
                PictureBox.Image = img;

                //==== loads the dimensions of the image after the aspect ratio check ====// 
                var imgDimensions = AspectRatioCheck();

                //==== resize image to fit picture box and place it in the center ====//
                PictureBox.Image = ResizeImage(img, imgDimensions.Width, imgDimensions.Height);
                PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            }

        }


        //
        // ==== Defines font colors of the "File" Tool Strip Item and the colors of the Drop Down Items ====
        //
        private void FileToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            FileToolStripMenuItem.ForeColor = Color.Black;
            MenuStrip.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
            fileMouseCheck = true;
        }


        //
        // ==== Defines font colors of the "File" Tool Strip Item and the colors of the Drop Down Items ====
        //
        private void FileToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            fileMouseCheck = false;
            if (fileDropDownCheck)
            {
                FileToolStripMenuItem.ForeColor = Color.Black;
            }
            else
            {
                FileToolStripMenuItem.ForeColor = Color.FromArgb(241, 241, 241);
            }
        }


        //
        // ==== Sets "File" to black if the drop down menu is open ====
        //
        private void FileToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            FileToolStripMenuItem.ForeColor = Color.Black;
            fileDropDownCheck = true;
        }


        //
        // ==== Sets "File" to RGB(241, 241, 241) if the drop down menu is closed ====
        //
        private void FileToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            fileDropDownCheck = false;
            if (fileMouseCheck)
            {
                FileToolStripMenuItem.ForeColor = Color.Black;
            }
            else
            {
                FileToolStripMenuItem.ForeColor = Color.FromArgb(241, 241, 241);
            }
        }


        //
        // ==== Defines font colors of the "Processing" Tool Strip Item and the colors of the Drop Down Items ====
        //
        private void EditToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            ProcessingToolStripMenuItem.ForeColor = Color.Black;
            MenuStrip.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
            editMouseCheck = true;
        }


        //
        // ==== Defines font colors of the "Processing" Tool Strip Item and the colors of the Drop Down Items ====
        //
        private void EditToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            editMouseCheck = false;
            if (editDropDownCheck)
            {
                ProcessingToolStripMenuItem.ForeColor = Color.Black;
            }
            else
            {
                ProcessingToolStripMenuItem.ForeColor = Color.FromArgb(241, 241, 241);
            }
        }


        //
        // ==== Sets "Processing" to black if the drop down menu is open ====
        //
        private void EditToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            ProcessingToolStripMenuItem.ForeColor = Color.Black;
            editDropDownCheck = true;
        }


        //
        // ==== Sets "Processing" to RGB(241, 241, 241) if the drop down menu is closed ====
        //
        private void EditToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            editDropDownCheck = false;
            if (editMouseCheck)
            {
                ProcessingToolStripMenuItem.ForeColor = Color.Black;
            }
            else
            {
                ProcessingToolStripMenuItem.ForeColor = Color.FromArgb(241, 241, 241);
            }
        }


        //
        // ==== Defines font colors of the "Mass Processing" Tool Strip Item and the colors of the Drop Down Items ====
        //
        private void MassProcessingToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            MassProcessingToolStripMenuItem.ForeColor = Color.Black;
            MenuStrip.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
        }


        //
        // ==== Defines font colors of the "Mass Processing" Tool Strip Item and the colors of the Drop Down Items ====
        //
        private void MassProcessingToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            MassProcessingToolStripMenuItem.ForeColor = Color.FromArgb(241, 241, 241);
            MenuStrip.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
        }


        //
        // ==== Defines font colors of the "Help" Tool Strip Item and the colors of the Drop Down Items ====
        //
        private void HelpToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            HelpToolStripMenuItem.ForeColor = Color.Black;
            MenuStrip.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
            helpMouseCheck = true;
        }


        //
        // ==== Defines font colors of the "Help" Tool Strip Item and the colors of the Drop Down Items ====
        //
        private void HelpToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            helpMouseCheck = false;
            if (helpDropDownCheck)
            {
                HelpToolStripMenuItem.ForeColor = Color.Black;
            }
            else
            {
                HelpToolStripMenuItem.ForeColor = Color.FromArgb(241, 241, 241);
            }
        }


        //
        // ==== Sets "Help" to black if the drop down menu is open ====
        //
        private void HelpToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            HelpToolStripMenuItem.ForeColor = Color.Black;
            helpDropDownCheck = true;
        }


        //
        // ==== Sets "Help" to RGB(241, 241, 241) if the drop down menu is closed ====
        //
        private void HelpToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            helpDropDownCheck = false;
            if (helpMouseCheck)
            {
                HelpToolStripMenuItem.ForeColor = Color.Black;
            }
            else
            {
                HelpToolStripMenuItem.ForeColor = Color.FromArgb(241, 241, 241);
            }
        }


        //
        // ==== Real time positioning of a number of controls as the form changes size ====
        //
        public void Panel_PictureBox_SizeChanged(object sender, EventArgs e)
        {
            //==== resize picture box to fit the panel that it's in ====//
            PictureBox.Location = new Point(0, 0);
            PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);

            if (PictureBox.Image != null) //==== check paramters to resize image to fit the picture box ====//
            {

                //==== loads the dimensions of the image after the aspect ratio check ====// 
                var imgDimensions = AspectRatioCheck();

                //==== save image width & height for later use ====//
                imgWidth = imgDimensions.Width; imgHeight = imgDimensions.Height;

                //==== resets zoom state ====//
                zoomFactor = 0;
                TextBox_ZoomFactor.Text = "Zoom Factor: x1";
                Button_ZoomIn.Enabled = true;
                Button_ZoomOut.Enabled = false;

                if (imgProcessed != null)
                {
                    //==== resize image to fit picture box and place it in the center ====//
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                }
                else
                {
                    //==== resize image to fit picture box and place it in the center ====//
                    PictureBox.Image = ResizeImage(img, imgDimensions.Width, imgDimensions.Height);
                }

                PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                //==== relocate "Click to Load Image" & "Move Image..." text boxes ====//
                MaskedTextBox_OpenFile.Location =
                    new Point(
                    (Panel_PictureBox.Width / 2) - (MaskedTextBox_OpenFile.Width / 2),
                    (Panel_PictureBox.Height / 2) - (MaskedTextBox_OpenFile.Height / 2) - 15);

                MaskedTextBox_MoveImage.Location =
                    new Point(
                        (Panel_PictureBox.Width / 2) - (MaskedTextBox_MoveImage.Width / 2),
                        (Panel_PictureBox.Height / 2) - (MaskedTextBox_MoveImage.Height) + 15);
            }
        }


        //
        // ==== Resizes the image to fit the picture box ====
        //
        public static Bitmap ResizeImage(Image imgOriginal, int width, int height)
        {

            //==== empty rectangle and image, to be resized ====//
            var newRectangle = new Rectangle(0, 0, width, height);
            var newImage = new Bitmap(width, height);

            //==== set resolution for the resized image ====//
            newImage.SetResolution(imgOriginal.HorizontalResolution, imgOriginal.VerticalResolution);

            //==== set graphics for the image ====//
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;               //
                graphics.CompositingQuality = CompositingQuality.HighQuality;        //
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;   //==== image properties ====//
                graphics.SmoothingMode = SmoothingMode.HighQuality;                  //
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;              //

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY); //==== set wrap mode ====//

                    graphics.DrawImage(           //
                        imgOriginal,              //
                        newRectangle,             //
                        0, 0,                     //==== draw the new image ====//
                        imgOriginal.Width,        //
                        imgOriginal.Height,       //
                        GraphicsUnit.Pixel,       //
                        wrapMode);                //
                }
            }
            return newImage;
        }


        //
        // ==== Adjusts image brightness and contrast ====
        //
        private Bitmap AdjustBrightnessContrast(Bitmap originalImage, Bitmap processedImage, float numericUDBrightness, float numericUDContrast)
        {
            if (processedImage != null)
            {
                originalImage = processedImage;
            }

            Bitmap adjustedImage = new Bitmap(originalImage.Width, originalImage.Height);

            float brightness =
                0.002f * numericUDBrightness    //
                +                               //==== calculate brightness ====//
                1.0f;                           //

            float gamma =
                0.00001f * numericUDContrast * numericUDContrast    //
                +                                                   //
                0.002f * numericUDContrast                          //==== calculate gamma ====//
                +                                                   //
                1.0f;                                               //


            //==== create matrix that changes the brightness and contrast of the image ====//
            float[][] ptsArray ={
            new float[] {brightness, 0, 0, 0, 0}, //==== RED     ====// //====
            new float[] {0, brightness, 0, 0, 0}, //==== GREEN   ====// //==== adjust RGB values to adjust brightness ====//
            new float[] {0, 0, brightness, 0, 0}, //==== BLUE    ====// //====
            new float[] {0, 0, 0, gamma, 0},      //==== GAMMA   ====// //==== through gamma, adjust contrast ====//
            new float[] {0, 0, 0, 0, 1}};         //==== SCALING ====// //==== constant ====//

            //==== generate image attributes ====//
            ImageAttributes imageAttributes = new ImageAttributes();

            //==== clear the color matrix ====//
            imageAttributes.ClearColorMatrix();
            
            //==== copy values from the ptsArray to the color matrix ====//
            imageAttributes.SetColorMatrix(
                new ColorMatrix(ptsArray),
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);
            
            //==== set image gamma ====//
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
            
            //==== create graphics for image drawing ====//
            Graphics g = Graphics.FromImage(adjustedImage);
            
            //==== draw image ====//
            g.DrawImage(
                originalImage,
                new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height),
                0,
                0,
                originalImage.Width, originalImage.Height,
                GraphicsUnit.Pixel, imageAttributes);

            return adjustedImage;
        }


        //
        // ==== Finds the maximum and minimum value of the image (imvalMax and imvalMin) ====
        //
        (int maxPixel, int minPixel) MinMaxPixel(Bitmap img)
        {
            int minPixel = 255;
            int maxPixel = 0;

            //==== loop for finding imval (image's pixel with the biggest value) ====//
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    //==== convert pixel color (aka value of the pixel) to integer ====//
                    int currentPixel = img.GetPixel(x, y).R;

                    //==== conditions for finding imval, (the pixel with the biggest value) ====//
                    if (currentPixel > maxPixel)
                    {
                        maxPixel = currentPixel;
                    }
                    else if (currentPixel < maxPixel)
                    {
                        minPixel = currentPixel;
                    }
                    else
                    {
                        continue;
                    }

                }
            }

            return (minPixel, maxPixel);
        }


        //
        // ==== Convert image from rgb to grayscale, easier to process ====
        //
        public Bitmap ToGrayscale(Bitmap RGBBitmap)
        {
            //==== create a blank bitmap the same size as original ====//
            Bitmap GrayBitmap = new Bitmap(RGBBitmap.Width, RGBBitmap.Height);

            //==== get a graphics object from the new image ====//
            using (Graphics g = Graphics.FromImage(GrayBitmap))
            {

                //==== create the grayscale ColorMatrix ====//
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
             new float[] {.3f, .3f, .3f, 0, 0},      //
             new float[] {.59f, .59f, .59f, 0, 0},   //
             new float[] {.11f, .11f, .11f, 0, 0},   //==== Grayscale Matrix ====//
             new float[] {0, 0, 0, 1, 0},            //
             new float[] {0, 0, 0, 0, 1}             //
                   });

                //==== create some image attributes ====//
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //==== set the color matrix attribute ====//
                    attributes.SetColorMatrix(colorMatrix);

                    //==== draw the original image on the new image ====//
                    //==== using the grayscale color matrix ====//
                    g.DrawImage(RGBBitmap, new Rectangle(0, 0, RGBBitmap.Width, RGBBitmap.Height),
                                0, 0, RGBBitmap.Width, RGBBitmap.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return GrayBitmap;
        }


        //
        // ==== Checks the aspect ratio of the image, returns the new dimentions of the image ====
        //
        (int Width, int Height) AspectRatioCheck()
        {
            int imgWidthResized;
            int imgHeightResized;

            if (WindowState == FormWindowState.Minimized) //==== when minimized dont calculate new img dimention ====//
            {
                imgWidthResized = img.Width;
                imgHeightResized = img.Height;
                return (imgWidthResized, imgHeightResized);
            }
            else
            {
                float imgRatio = (float)Decimal.Divide(img.Width, img.Height);
                float boxRatio = (float)Decimal.Divide(PictureBox.Width, PictureBox.Height);

                if (imgRatio >= boxRatio) //==== if the image's ratio is bigger than the picture-box's ratio ====//
                {
                    imgWidthResized = PictureBox.Width;
                    float imgHeightTemp = imgWidthResized / imgRatio;
                    imgHeightResized = (int)imgHeightTemp;
                }
                else //==== if the image's ratio is smaller than the box's ratio ====//
                {
                    imgHeightResized = PictureBox.Height;
                    float imgWidthTemp = imgHeightResized * imgRatio;
                    imgWidthResized = (int)imgWidthTemp;
                }

                return (imgWidthResized, imgHeightResized);
            }
        }


        //
        // ==== Calculates the Size of the picturebox ====
        //
        private Size ZoomPictureBox(double zoomFactor)
        {
            //==== calculation of the actual zoom value that will be used and some secondary actions ====//
            zoomScale = zoomFactor / 2 + 1;

            Size s = new Size(                                  //
                (int)(Panel_PictureBox.Width * zoomScale),      //==== zoom picture box ====//
                (int)(Panel_PictureBox.Height * zoomScale));    //

            return s;
        }


        //
        // ==== Calculates the zoomed image ====
        //
        private Bitmap ZoomImage(double zoomFactor)
        {
            Bitmap imgZoomed;

            zoomScale = zoomFactor / 2 + 1;

            imgZoomed = new Bitmap(
                PictureBox.Image,                 //
                (int)(imgWidth * zoomScale),      //==== zoom the currently displayed image in the picture box ====//
                (int)(imgHeight * zoomScale));    //

            return imgZoomed;
        }


        //
        // ==== Calculates the Location of the picturebox ====
        //
        private Point PlacePictureBox(int zoomState)
        {
            if (zoomState == 1) //==== zoom in Picture Box ====//
            {
                //==== horizontal distance between mouse and picture box center ====//
                int dx =
                    (int)
                    Math.Abs(
                        Math.Round(
                            (decimal)(
                            (PictureBox.Location.X - Panel_PictureBox.Width / 2) / 2)));

                //==== vertical distance between mouse and picture box center ====//
                int dy =
                    (int)
                    Math.Abs(
                        Math.Round(
                            (decimal)(
                            (PictureBox.Location.Y - Panel_PictureBox.Height / 2) / 2)));
                
                //==== new picture box position based on dx and dy ====//
                Point newP =
                    new Point(
                        PictureBox.Location.X - dx,
                        PictureBox.Location.Y - dy);

                return newP;
            }
            else //==== zoom out Picture Box ====//
            {
                //==== horizontal distance between mouse and picture box center ====//
                int dx =
                    (int)
                    Math.Abs(
                        Math.Floor(
                            (decimal)(
                            (PictureBox.Location.X - Panel_PictureBox.Width / 2) / 3)));

                //==== vertical distance between mouse and picture box center ====//
                int dy =
                    (int)
                    Math.Abs(
                        Math.Floor(
                            (decimal)(
                            (PictureBox.Location.Y - Panel_PictureBox.Height / 2) / 3)));

                //==== new picture box position based on dx and dy ====//
                Point newP =
                    new Point(
                        PictureBox.Location.X + dx,
                        PictureBox.Location.Y + dy);

                return newP;
            }
        }






        //======================================================================================//
        //======================================================================================//
        //================================                      ================================//
        //================================   IMAGE PROCESSING   ================================//
        //================================                      ================================//
        //======================================================================================//
        //======================================================================================//



        //
        // ==== Initiate Linear Imaging Function ====
        //
        private void ToolStripMenuItem_Linear_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }

            //==== create Linear Imaging form instance and transfer necessary data ====//
            using (FormA1_Linear fLinear = new FormA1_Linear())
            {
                fLinear.TransferData(imgClone, imvalMax); //==== this to pass Data ====// 
                if (fLinear.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fLinear.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;

        }


        //
        // ==== Initiate Optimum Imaging Function ====
        //
        private void ToolStripMenuItem_Optimum_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create Optimum Imaging form instance and transfer necessary data ====//
            using (FormA2_Optimum fOptimum = new FormA2_Optimum())
            {
                fOptimum.TransferData(imgClone, imvalMin, imvalMax); //==== this to pass Data ====//  
                if (fOptimum.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fOptimum.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate Simple Window ====
        //
        private void ToolStripMenuItem_SimpleW_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create Simple Window Imaging form instance and transfer necessary data ====//
            using (FormA3_SimpleW fSimpleW = new FormA3_SimpleW())
            {
                fSimpleW.TransferData(imgClone); //==== this to pass Data ====// 
                if (fSimpleW.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fSimpleW.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate Broken Window ====
        //
        private void ToolStripMenuItem_BrokenW_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create Broken Window Imaging form instance and transfer necessary data ====//
            using (FormA4_BrokenW fBrokenW = new FormA4_BrokenW())
            {
                fBrokenW.TransferData(imgClone, imvalMax); //==== this to pass Data ====// 
                if (fBrokenW.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fBrokenW.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate HCDF ====
        //
        private void ToolStripMenuItem_HCDF_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create HCDF Imaging form instance and transfer necessary data ====//
            using (FormB1_HCDF fHCDF = new FormB1_HCDF())
            {
                fHCDF.TransferData(imgClone, false); //==== this to pass Data ====//
                if (fHCDF.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fHCDF.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate CLAHE ====//
        //
        private void ToolStripMenuItem_CLAHE_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }

            //==== create HCDF Imaging form instance and transfer necessary data ====//
            using (FormB2_CLAHE fCLAHE = new FormB2_CLAHE())
            {
                fCLAHE.TransferData(imgClone, false); //==== this to pass Data ====// 
                if (fCLAHE.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fCLAHE.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate 3x3 Convolution Filtering ====
        //
        private void ToolStripMenuItem_3x3Conv_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create 3x3 Convolution Filtering form instance and transfer necessary data ====//
            using (FormC1_3x3Conv f3x3Conv = new FormC1_3x3Conv())
            {
                f3x3Conv.TransferData(imgClone, imvalMax); //==== this to pass Data ====// 
                if (f3x3Conv.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = f3x3Conv.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate MxN Convolution Filtering ====
        //
        private void ToolStripMenuItem_MxNConv_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create MxN Convolution Filtering Imaging form instance and transfer necessary data ====//
            using (FormC2_MxNConv fMxNConv = new FormC2_MxNConv())
            {
                fMxNConv.TransferData(imgClone, imvalMax); //==== this to pass Data ====// 
                if (fMxNConv.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fMxNConv.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate 3x3 Unsharp Masking ====
        //
        private void ToolStripMenuItem_3x3Unsharp_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create HCDF Imaging form instance and transfer necessary data ====//
            using (FormC3_3x3Unsharp fUnsharp3x3 = new FormC3_3x3Unsharp())
            {
                fUnsharp3x3.TransferData(imgClone,imvalMax); //==== this to pass Data ====// 
                if (fUnsharp3x3.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fUnsharp3x3.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Inititate MxN Unsharp Masking ====
        //
        private void ToolStripMenuItem_MxNUnsharp_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create HCDF Imaging form instance and transfer necessary data ====//
            using (FormC4_MxNUnsharp fUnsharpMxN = new FormC4_MxNUnsharp())
            {
                fUnsharpMxN.TransferData(imgClone, imvalMax); //==== this to pass Data ====// 
                if (fUnsharpMxN.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fUnsharpMxN.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate MxN Median Filter ====
        //
        private void ToolStripMenuItem_MxNMedian_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create HCDF Imaging form instance and transfer necessary data ====//
            using (FormC5_MxNMedian fMedianMxN = new FormC5_MxNMedian())
            {
                fMedianMxN.TransferData(imgClone, imvalMax); //==== this to pass Data ====// 
                if (fMedianMxN.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fMedianMxN.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }

        
        //
        // ==== Initiate Gaussian Filter ====
        //
        private void ToolStripMenuItem_Gaussian_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create HCDF Imaging form instance and transfer necessary data ====//
            using (FormD2_Gaussian fGaussian = new FormD2_Gaussian())
            {
                fGaussian.TransferData(imgClone); //==== this to pass Data ====// 
                if (fGaussian.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image and the matrix FH back to the main form and display it ====//
                    imgProcessed = fGaussian.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate Wiener Filter ====
        //
        private void ToolStripMenuItem_Wiener_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== dispose the previous processed images ====//
            if (imgProcessed != null)
            {
                imgProcessed.Dispose(); imgProcessedClone.Dispose();
                imgProcessed = null; imgProcessedClone = null;
            }


            //==== create HCDF Imaging form instance and transfer necessary data ====//
            using (FormD1_Wiener fWiener = new FormD1_Wiener())
            {
                fWiener.TransferData(imgClone); //==== this to pass Data ====// 
                if (fWiener.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image and the matrix FH back to the main form and display it ====//
                    imgProcessed = fWiener.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====// 
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }

            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate Lucy-Richardson Deconvolution ====
        //
        private void ToolStripMenuItem_LucyRichardson_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }
            

            //==== create Lucy-Richardson Deconv form instance and transfer necessary data ====//
            using (FormE1_LucyRichardsonDeconv fLucyRichardson = new FormE1_LucyRichardsonDeconv())
            {
                if (imgProcessed != null) //==== pass the processed image ====//
                {
                    fLucyRichardson.TransferData(imgProcessed); //==== this to pass Data ====//
                }
                else //==== pass the original image ====//
                {
                    fLucyRichardson.TransferData(imgClone); //==== this to pass Data ====//
                }
                
                if (fLucyRichardson.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fLucyRichardson.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====//
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }
            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate Blind Deconvolution ====
        //
        private void ToolStripMenuItem_Blind_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }


            //==== create Blind Deconv form instance and transfer necessary data ====//
            using (FormE2_BlindDeconv fBlind = new FormE2_BlindDeconv())
            {
                if (imgProcessed != null) //==== pass the processed image ====//
                {
                    fBlind.TransferData(imgProcessed); //==== this to pass Data ====//
                }
                else //==== pass the original image ====//
                {
                    fBlind.TransferData(imgClone); //==== this to pass Data ====//
                }

                if (fBlind.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fBlind.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====//
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }
            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate Regularization Deconvolution ====
        //
        private void ToolStripMenuItem_Regularization_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }


            //==== create Regularization Deconv form instance and transfer necessary data ====//
            using (FormE3_Regularization fRegularization = new FormE3_Regularization())
            {
                if (imgProcessed != null) //==== pass the processed image ====//
                {
                    fRegularization.TransferData(imgProcessed); //==== this to pass Data ====//
                }
                else //==== pass the original image ====//
                {
                    fRegularization.TransferData(imgClone); //==== this to pass Data ====//
                }

                if (fRegularization.ShowDialog() == DialogResult.OK)
                {
                    //==== pass the processed image back to the main form and display it ====//
                    imgProcessed = fRegularization.ImgProcessed;
                    imgProcessedClone = imgProcessed;
                    PictureBox.Image = imgProcessed;

                    //==== reset size of picture box and relocate it to be centered ====//
                    PictureBox.Size = new Size(Panel_PictureBox.Width, Panel_PictureBox.Height);
                    PictureBox.Location = new Point(0, 0);

                    //==== place image in the center of the picture box and resized it to fit it while preserving its aspect ratio ====//
                    var imgDimensions = AspectRatioCheck();
                    PictureBox.Image = ResizeImage(imgProcessed, imgDimensions.Width, imgDimensions.Height);
                    PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

                    //==== reset zoom state, zoom factor and zoom scale ====//
                    zoomState = 0; zoomFactor = 0; zoomScale = 1;

                    //==== reset zoom buttons to their default sates (zoom in enabled, zoom out disabled) ====//
                    Button_ZoomIn.Enabled = true;
                    Button_ZoomOut.Enabled = false;

                    //==== reset brightness/contrast values ====//
                    TrackBar_Brightness.Value = 5; TrackBar_Contrast.Value = 5;
                }
            }
            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }


        //
        // ==== Initiate Mass Processing ====
        //
        private void MassProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //==== indicate background processes are running ====//
            BackgroundWorker.RunWorkerAsync();
            while (BackgroundWorker.IsBusy)
            {
                ToolStripStatusLabel_ReadyBusy.Text = "Working...";
                ToolStripStatusLabel_ReadyBusy.ForeColor = Color.Gold;
                Application.DoEvents();
            }

            //==== create Mass Processing form instance and transfer necessary data ====//
            using (FormF1_MassProcessing fMass = new FormF1_MassProcessing())
            {
                fMass.ShowDialog();
            }


            //==== indicate background processes stopped running ====//
            ToolStripStatusLabel_ReadyBusy.Text = "Ready";
            ToolStripStatusLabel_ReadyBusy.ForeColor = Color.SpringGreen;
        }






        //==========================================================================//
        //==========================================================================//
        //================================          ================================//
        //================================   HELP   ================================//
        //================================          ================================//
        //==========================================================================//
        //==========================================================================//


        //
        // ==== Show Help for the Linear Imaging and Windows processes ====
        //
        private void ToolStripMenuItem_LinearAndWindows_Help_Click(object sender, EventArgs e)
        {
            using(FormA_LinearAndWindowsHelp fHelp_A = new FormA_LinearAndWindowsHelp())
            {
                fHelp_A.ShowDialog();
            }
        }


        //
        // ==== Show Help for the Histogram Imaging processes ====
        //
        private void ToolStripMenuItem_Histogram_Help_Click(object sender, EventArgs e)
        {
            using(FormB_HistogramHelp fHelp_B = new FormB_HistogramHelp())
            {
                fHelp_B.ShowDialog();
            }
        }


        //
        // ==== Show Help for the Convolution Filtering processes ====
        //
        private void ToolStripMenuItem_Convolution_Help_Click(object sender, EventArgs e)
        {
            using(FormC_ConvolutionHelp fHelp_C = new FormC_ConvolutionHelp())
            {
                fHelp_C.ShowDialog();
            }
        }


        //
        // ==== Show Help for the Blurring Filters processes ====
        //
        private void ToolStripMenuItem_Blurring_Help_Click(object sender, EventArgs e)
        {
            using(FormD_BlurHelp fHelp_D = new FormD_BlurHelp())
            {
                fHelp_D.ShowDialog();
            }
        }


        //
        // ==== Show Help for the Image Restoration processes ====
        //
        private void ToolStripMenuItem_Restoration_Help_Click(object sender, EventArgs e)
        {
            using(FormE_RestorationHelp fHelp_E = new FormE_RestorationHelp())
            {
                fHelp_E.ShowDialog();
            }
        }


        //
        // ==== Show Help for the Mass Processing feature ====
        //
        private void ToolStripMenuItem_MassProcessing_Click(object sender, EventArgs e)
        {
            using(FormF_MassProcessingHelp fHelp_F = new FormF_MassProcessingHelp())
            {
                fHelp_F.ShowDialog();
            }
        }
    }


    //
    // ==== Class that defines the Menu Strip Items' colors ====
    //
    class MenuColorTable : ProfessionalColorTable
    {
        public MenuColorTable()
        {
            base.UseSystemColors = false;
        }

        public override Color MenuBorder
        {
            get { return Color.FromArgb(45, 45, 45); }
        }
        public override Color MenuItemBorder
        {
            get { return Color.FromArgb(45, 45, 45); }
        }
        public override Color MenuItemSelected
        {
            get { return Color.FromArgb(45, 45, 45); }
        }
        public override Color ToolStripDropDownBackground
        {
            get { return Color.FromArgb(24, 24, 25); }
        }
    }
}