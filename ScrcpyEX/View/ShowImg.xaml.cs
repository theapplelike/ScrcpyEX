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

namespace ScrcpyEX.View
{
    /// <summary>
    /// ShowImg.xaml 的交互逻辑
    /// </summary>
    public partial class ShowImg : Window
    {
        public ShowImg(RecInfo recInfo)
        {
            InitializeComponent();
            img.Source = MyGM.GetImage(recInfo.ImgName);
            tbk.Text = 
                $"起始点：({recInfo.CropRect.X},{recInfo.CropRect.Y})，" +
                $"图片宽高：({recInfo.CropRect.Width},{recInfo.CropRect.Height})。";
        }

        public ShowImg(RecInfo recInfo, BitmapSource bitmap)
        {
            InitializeComponent();

            //double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{recInfo.ImgName}", bitmap);
            //Console.WriteLine($"图像脚本完毕，相似度：{ssim:F8}");

            img.Source = MyGM.GetImage(recInfo.ImgName);
            tbk.Text =
                $"起始点：({recInfo.CropRect.X},{recInfo.CropRect.Y})，" +
                $"图片宽高：({recInfo.CropRect.Width},{recInfo.CropRect.Height})。";

            Image img1 = new() { Margin = new Thickness(5), Source = bitmap };
            ung.Children.Add(img1);
        }
    }
}
