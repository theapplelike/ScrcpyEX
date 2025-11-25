using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScrcpyEX.View
{
    public partial class RecManage//code1
    {
        /// <summary>
        /// 执行指定脚本。
        /// </summary>
        /// <param name="btn">包含脚本数据的按钮。</param>
        private void ExecuteScript(Button btn)
        {
            if (btn.CommandParameter is RecInfo recInfo)
            {
                //foreach (var info in recInfo.Infos)
                //{
                //    Console.WriteLine($"遍历：{info.LastTime}-{info.Pt.X},{info.Pt.Y}");
                //}
                //Console.WriteLine($"循环次数：{recInfo.CycleNumber}，循环间隔：{recInfo.CycleInterval}");
                TempRecInfo = recInfo;
                DialogResult = true;
            }
        }

        /// <summary>
        /// 进行指定脚本的编辑。
        /// </summary>
        /// <param name="btn">包含脚本数据的按钮。</param>
        private void EditScript(Button btn)
        {
            if (btn.CommandParameter is RecInfo recInfo)
            {
                ValToUI(recInfo);
                SetCurrentColor(btn);

                TempRecInfo = recInfo;
                _vm.SetParam(recInfo);
            }
        }

        /// <summary>
        /// 删除指定的脚本。
        /// </summary>
        /// <param name="btn">包含脚本数据的按钮。</param>
        private void DeleteScript(Button btn)
        {
            if (btn.CommandParameter is RecInfo recInfo)
            {
                TempRecInfo = null;
                _vm.DelRec(recInfo);
            }
        }

        /// <summary>
        /// 保存当前编辑的脚本。
        /// </summary>
        private void SaveScript()
        {
            int.TryParse(txtCycleNum.Text, out int num);
            int.TryParse(txtCycleInterval.Text, out int interval);
            double.TryParse(txtSimilarity.Text, out double similarity);

            RecInfo recInfo = new()
            {
                IsLimited = (bool)rdoLimited.IsChecked,
                CycleNumber = num,
                IsTime = (bool)rdoTime.IsChecked,
                CycleInterval = interval,
                ImgName = img.Source == null ? "" : $"Pic{DateTime.Now:yyMMddhhmmss}.png",
                Similarity = similarity,
                IsAdaptive = (bool)chkAdaptive.IsChecked,
                CropRect = _cropRect,
            };

            _vm.Save(recInfo, (BitmapSource)img.Source);
        }

        /// <summary>
        /// 设置当前编辑的脚本UI的背景颜色。
        /// </summary>
        /// <param name="btn">包含脚本数据的按钮。</param>
        private void SetCurrentColor(Button btn)
        {
            _g?.Background = null;

            _g = btn.FindVisualParent<Grid>();
            _g.Background = new SolidColorBrush(Color.FromArgb(128, 118, 238, 198));
        }


    }
}
