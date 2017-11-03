using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using FC.MarkLocator;

namespace AutoAlignment2D
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        FC.MarkLocator.ProcessImage ProcessImage = new ProcessImage();
        ViewModel.MainViewModel ViewModel = new ViewModel.MainViewModel();
        HDevelopExport hdev = new HDevelopExport();

        public MainWindow()
        {
            InitializeComponent();
            InitializeEvents();

            this.Loaded += delegate {
                this.DataContext = ViewModel;
            };
        }

        /// <summary>
        /// 按钮事件
        /// </summary>
        private void InitializeEvents()
        {
            this.btnOpenImgFileU.Click += delegate
            {
                this.imgMark.Source = null;
                Mat markImage = GetBitmapFromFile();
                if (markImage == null) return;
                this.imgMark.Source = BitmapSourceConvert.ToBitmapSource(markImage);

                
            };

            this.btnOpenImgFileL.Click += delegate
            {
                Mat markImage = GetBitmapFromFile();
                if (markImage == null) return;
                this.imgProbe.Source = BitmapSourceConvert.ToBitmapSource(markImage);
            };

            this.btnFindMark.Click += delegate
            {
                if (ProcessImage.MarkAlignment(ProcessImage.InputManager.MarkImageFile))
                {
                    this.imgMark.Source = new BitmapImage(new Uri(ProcessImage.OutputManager.MarkImgFile));
                    this.rtxtOutput.AppendText(string.Format("\n[{0},{1},{2}]\n{3}",
                                                                           ProcessImage.OutputManager.AlignmentX,
                                                                           ProcessImage.OutputManager.AlignmentY,
                                                                           ProcessImage.OutputManager.AlignmentSita,
                                                                           ProcessImage.OutputManager.MarkImgFile));
                }
                else
                {
                    this.rtxtOutput.AppendText("\r\n靶标识别失败");
                }
            };

            this.btnLocateMarkL.Click += delegate
            {
                ProcessImage.InputManager.AreaStartX = 500;
                ProcessImage.InputManager.AreaEndX = 800;
                ProcessImage.InputManager.AreaStartY = 350;
                ProcessImage.InputManager.AreaEndY = 650;
                if (ProcessImage.FindProbeMark(ProcessImage.InputManager.MarkImageFile))
                {
                    this.imgProbe.Source = new BitmapImage(new Uri(ProcessImage.OutputManager.MarkImgFile));
                    this.rtxtOutput.AppendText(string.Format("[{0},{1},{2}]\r\n{3}",
                                                               ProcessImage.OutputManager.AlignmentX,
                                                               ProcessImage.OutputManager.AlignmentY,
                                                               ProcessImage.OutputManager.AlignmentSita,
                                                               ProcessImage.OutputManager.MarkImgFile));
                }
                else
                {
                    this.rtxtOutput.AppendText("\r\n靶标识别失败");
                }
            };
        }

        private Mat GetBitmapFromFile()
        {
            Mat img;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            DialogResult result = openFileDialog.ShowDialog();
            openFileDialog.Filter = " (*.jpg,*.png,*.jpeg,*.bmp,*.gif)| *.jgp; *.png; *.jpeg; *.bmp";
            if (result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes)
            {
                ProcessImage.InputManager.MarkImageFile = openFileDialog.FileName;
                img = CvInvoke.Imread(openFileDialog.FileName, ImreadModes.AnyColor);
            }
            else
            {
                img = null;
            }
            return img;
        }



    }
}
