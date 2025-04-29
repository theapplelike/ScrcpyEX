using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using ScrcpyEX.MyDefinition;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace ScrcpyEX.View
{
    public partial class RecManage//code
    {
        private void MyInit()
        {
            _vm.RecInfos = new(MyCfg.Cfg.RecInfos);
        }

        /// <summary>
        /// 每次编辑之前初始化ListBox的状态。
        /// </summary>
        /// <param name="obj">ListBox元素。</param>
        private void InitListBox(DependencyObject obj)
        {
            if (obj == null) return;
            if (obj is Button btn && btn.Name == "btnEdit")
            {
                btn.Content = "\ue8ac";
            }
            if (obj is TextBox txt && txt.Name == "txtEdit")
            {
                txt.Visibility = Visibility.Hidden;
            }
            if (obj is TextBlock tbk && tbk.Name == "tbkEdit")
            {
                tbk.Visibility = Visibility.Visible;
            }

            // 递归遍历子控件
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                InitListBox(child);
            }
        }

        /// <summary>
        /// 编辑脚本名称。
        /// </summary>
        /// <param name="sender">触发事件的按钮对象。</param>
        /// <param name="e">事件参数。</param>
        private void EditName(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            Grid parent = MyGM.GetParentObject<Grid>(obj);
            TextBox txt = MyGM.FindVisualChild<TextBox>(parent);
            TextBlock tbk = MyGM.FindVisualChild<TextBlock>(parent);

            if (txt.Visibility == Visibility.Hidden)
            {
                Console.WriteLine("进行编辑");
                //如果当前项要变成编辑状态，但已有其他项为编辑状态，则初始化所有项
                if (_isEditing)
                {
                    InitListBox(lst);
                }

                _isEditing = true;
                ((Button)sender).Content = "\uf78c";
                txt.Visibility = Visibility.Visible;
                tbk.Visibility = Visibility.Hidden;

                txt.Focus();
                txt.Select(txt.Text.Length, 0);
            }
            else
            {
                Console.WriteLine("编辑完成");
                _isEditing = false;
                ((Button)sender).Content = "\ue8ac";
                txt.Visibility = Visibility.Hidden;
                tbk.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 将指定脚本的数据赋值给界面。
        /// </summary>
        /// <param name="recInfo">指定的脚本实体类。</param>
        private void ValToUI(RecInfo recInfo)
        {
            rdoLimited.IsChecked = recInfo.Limited;
            rdoHandStop.IsChecked = recInfo.HandStop;
            txtCycleNum.Text = recInfo.CycleNumber.ToString();
            txtCycleInterval.Text = recInfo.CycleInterval.ToString();
        }

        /// <summary>
        /// 进行截图。
        /// </summary>
        private async void DoScreenshot()
        {
            Hide();
            await Task.Delay(500);//让当前窗体有足够时间隐藏

            Screenshot screen = new();
            if (screen.ShowDialog() == true)
            {
                img.Source = screen.Crop;
                //CaptureRegion(screen.SelectedRegion);

                using (FileStream fs = new FileStream("screenshot.png", FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(screen.Crop));
                    encoder.Save(fs);
                }
            }

            ShowDialog();
        }

        private void CaptureRegion(Rect region)
        {
            // 转换为整数像素坐标
            int x = (int)region.X*2;
            int y = (int)region.Y*2;
            int width = (int)region.Width * 2;
            int height = (int)region.Height * 2;

            // 使用System.Drawing截屏
            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
                }

                // 转换为WPF BitmapSource
                BitmapImage bitmapImage;
                using (MemoryStream memory = new MemoryStream())
                {
                    bmp.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }

                // 显示或保存截图
                img.Source = bitmapImage;

                // 保存到文件
                using (FileStream fs = new FileStream("screenshot.png", FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    encoder.Save(fs);
                }
            }
        }


    }
}
