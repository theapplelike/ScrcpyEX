using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
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
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ScrcpyEX.View
{
    /// <summary>
    /// ShowImg.xaml 的交互逻辑
    /// </summary>
    public partial class ShowImg : Window
    {
        //public ShowImg(ScriptInfo info)
        //{
        //    InitializeComponent();
        //
        //    Grid.SetColumnSpan(gOrig, 2);
        //    imgOrig.Source = MyGM.GetImage(info.ImgName);
        //    tbkOrigParam.Text = 
        //        $"起始坐标：({info.CropRect.X},{info.CropRect.Y})，" +
        //        $"图片宽高：({info.CropRect.Width},{info.CropRect.Height})。";
        //}

        /// <summary>
        /// 显示单张图片。
        /// </summary>
        /// <param name="source">图片源。</param>
        /// <param name="rect">图片的宽高和位置。</param>
        public ShowImg(ImageSource source, Rect rect)
        {
            InitializeComponent();

            Grid.SetColumnSpan(gOrig, 3);
            sepV.Visibility = Visibility.Collapsed;
            imgOrig.Source = source;
            tbkOrigParam.Text =
                $"起始坐标：({rect.X:F2}，{rect.Y:F2})--" +
                $"图片宽高：({rect.Width:F2}，{rect.Height:F2})";
        }

        //public ShowImg(ScriptInfo info, BitmapSource bitmap)
        //{
        //    InitializeComponent();
        //
        //    double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{info.ImgName}", bitmap);
        //    Image img1 = new() { Margin = new Thickness(5), Source = bitmap };
        //
        //    //ufg.Children.Add(img1);
        //    imgOrig.Source = MyGM.GetImage(info.ImgName);
        //    tbkOrigParam.Text =
        //        $"起始坐标：({info.CropRect.X},{info.CropRect.Y})，" +
        //        $"图片宽高：({info.CropRect.Width},{info.CropRect.Height})，" +
        //        $"相似程度：{ssim:F8}。";
        //    
        //}

        /// <summary>
        /// 显示组脚本的图片和对应区域截图的相似度。
        /// </summary>
        /// <param name="infos">一组脚本信息。</param>
        /// <param name="bmps">和组脚本对应的临时截图。</param>
        public ShowImg(List<ScriptInfo> infos, List<BitmapSource> bmps)
        {
            InitializeComponent();

            gOrig.Visibility = Visibility.Collapsed;
            int i = 0;
            foreach (ScriptInfo si in infos)
            {
                gMain.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) });

                if (si.ImgName == "")
                {
                    NoImage(i, 0);
                    NoImage(i, 2);
                }
                else
                {
                    AddImageGroup(i, 0, si.CropRect, MyGM.GetImage(si.ImgName));
                    AddImageGroup(i, 3, si.CropRect, bmps[i]);
                    ShowSimilarity(i, si.ImgName, bmps[i]);
                }

                DrawLine(i);
                i++;
            }
        }

        /// <summary>
        /// 没有图像时添加文字说明。
        /// </summary>
        /// <param name="row">所在行。</param>
        /// <param name="col">所在列。</param>
        private void NoImage(int row, int col)
        {
            TextBlock tbk = new()
            {
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = $"没有图像",
            };

            Grid.SetRow(tbk, row);
            Grid.SetColumn(tbk, col);
            gMain.Children.Add(tbk);
        }

        /// <summary>
        /// 添加图片及其参数信息到指定行和列中。
        /// </summary>
        /// <param name="row">所在行。</param>
        /// <param name="col">所在列。</param>
        /// <param name="rect">图片的矩形信息。</param>
        /// <param name="bmp">要显示的图片、</param>
        private void AddImageGroup(int row, int col, Rect rect, BitmapSource bmp)
        {
            Grid g = new();
            g.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) });
            g.RowDefinitions.Add(new() { Height = GridLength.Auto });

            Image img = new()
            {
                Margin = new Thickness(5),
                Source = bmp,
            };

            TextBlock tbk = new()
            {
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = $"起始坐标：({rect.X:F2}，{rect.Y:F2})--" +
                       $"图片宽高：({rect.Width:F2}，{rect.Height:F2})",
            };
            Grid.SetRow(tbk, 1);

            g.Children.Add(img);
            g.Children.Add(tbk);
            Grid.SetRow(g, row);
            Grid.SetColumn(g, col);

            gMain.Children.Add(g);
        }

        /// <summary>
        /// 显示相似度信息。
        /// </summary>
        /// <param name="row">所在行。</param>
        /// <param name="ImgName">原始图片的名称。</param>
        /// <param name="bmp">临时截取的图片。</param>
        private void ShowSimilarity(int row, string ImgName, BitmapSource bmp)
        {
            double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{ImgName}", bmp);

            TextBlock tbk = new()
            {
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = $"相似程度：\n{ssim:P8}",
                Background = Brushes.WhiteSmoke,
            };
            Grid.SetRow(tbk, row);
            Grid.SetColumnSpan(tbk, 3);
            gMain.Children.Add(tbk);
        }

        /// <summary>
        /// 绘制底部的分隔线。
        /// </summary>
        /// <param name="row">所在行。</param>
        private void DrawLine(int row)
        {
            Rectangle rect = new()
            {
                Style = (Style)FindResource("sepH")
            };
            Grid.SetRow(rect, row);
            Grid.SetColumnSpan(rect, 3);
            gMain.Children.Add(rect);
        }


    }
}
