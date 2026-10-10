using OpenCvSharp.ML;
using ScrcpyEX.ExtensionMethod;
using ScrcpyEX.Model;
using ScrcpyEX.MyDefinition;
using ScrcpyEX.MyDefinition.MyClass;
using ScrcpyEX.MyDefinition.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static ScrcpyEX.MyDefinition.MyEnum;
using static ScrcpyEX.MyDefinition.WindowsAPI.VK;
using DBrushes = System.Drawing.Brushes;
using DPoint = System.Drawing.Point;  //Windows Forms的Point
using DRectangle = System.Drawing.Rectangle;
using WBrushes = System.Windows.Media.Brushes;
using WPoint = System.Windows.Point;  //WPF的Point
using WRectangle = System.Windows.Shapes.Rectangle;

namespace ScrcpyEX.View
{
    public partial class MainWindow//code2
    {
        /// <summary>
        /// 代码设置界面控件的事件。
        /// </summary>
        private void SetControlEvent()
        {
            rdoLoc1.Click += (s, e) => Locate();
            rdoLoc2.Click += (s, e) => Locate();
            rdoLoc3.Click += (s, e) => Locate();
            rdoLoc4.Click += (s, e) => Locate();
        }

        /// <summary>
        /// 将SC的视频帧宽高信息显示到界面上。
        /// </summary>
        /// <param name="texture">说明视频帧宽高的字符串，第一个参数是宽，第二个参数是高。</param>
        public void ShowTextureInfo(string texture)
        {
            Regex reg = new(@"(\d+)x(\d+)");
            MatchCollection mc = reg.Matches(texture);

            _vm.Texture = GetTextureInfo(mc);
        }

        /// <summary>
        /// 获得分辨率和比值。
        /// </summary>
        private string GetTextureInfo(MatchCollection mc)
        {
            int.TryParse(mc[0].Groups[1].Value, out int a);
            int.TryParse(mc[0].Groups[2].Value, out int b);

            _ScFrameHt = b;
            _ScFrameWd = a;
            _ScAspectRatio = a > b ? a * 1.0 / b : b * 1.0 / a;
            //MDC_Hook.Enable = a < b;

            return $"投屏纹理 {a}*{b}，比值 {_ScAspectRatio:F2}";
        }

        /// <summary>
        /// 设置计时器。DispatcherTimer计时器有一个特性，如果已经启动，
        /// 再执行DispatcherTimer.Start()方法不会有任何效果，计时器不会重新开始计时，也不会重置间隔。
        /// </summary>
        private void SetTimer()
        {
            _resizeTimer.Tick += new EventHandler(Resize_Tick);
            _resizeTimer.Interval = TimeSpan.FromSeconds(1);

            _locationChangeTimer.Tick += new EventHandler(LocationChange_Tick);
            _locationChangeTimer.Interval = TimeSpan.FromSeconds(0.1);

            _timedHandler = async (sender, e) => await TimedScript_Tick(_scriptInfos);
            _imageHandler = async (sender, e) => await ImageScript_Tick(_scriptInfos);

            _countTimer.Elapsed += (sender, e) => Count_Tick();
            _countTimer.Interval = 1000;

            //_testTimer.Elapsed += Test_Tick;
            //_testTimer.Interval = 3000;
        }

        /// <summary>
        /// 程序大小改变后重新创建映射Popup。
        /// </summary>
        private void Resize_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("程序大小已改变，重新生成映射pop。");

            _resizeTimer.Stop();
            CreateSavedPop(btnMappingAdd.IsEnabled);
        }

        /// <summary>
        /// 程序位置改变后重新设置映射Popup的位置。
        /// </summary>
        private void LocationChange_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("程序位置已改变，重新显示映射pop。");

            _locationChangeTimer.Stop();
            if (_isAdjustPop)
            {
                HideKeyMapping();
                ShowKeyMapping();
            }
            else
            {
                _isAdjustPop = true;
            }
        }

        /// <summary>
        /// 执行脚本时，说明已产生的间隔。
        /// </summary>
        private void Count_Tick() => _vm.Running = (int)_stopwatch.Elapsed.TotalSeconds;

        /// <summary>
        /// 执行计时脚本。
        /// </summary>
        private async Task TimedScript_Tick(List<ScriptInfo> infos)
        {
            //Debug.WriteLine($"Timed进入新一轮，已产生的间隔：{_vm.Running}。");
            _vm.CurCycleNum++;
            _stopwatch.Restart();

            for (int i = 0; i < infos.Count; i++)
            {
                //最后一个脚本时判断是否应该退出
                await ExecuteClickScript(infos[i], ScHandle, i == infos.Count - 1);
            }
        }

        /// <summary>
        /// 执行图像脚本。
        /// </summary>
        private async Task ImageScript_Tick(List<ScriptInfo> infos)
        {
            // 防止重叠执行
            if (!await _imageScriptLock.WaitAsync(0))return;

            try
            {
                if (_scriptItem is ScriptGroup sg)
                {
                    if (sg.LinkMode == LinkMode.InTurn)
                    {
                        await ImageCompare(infos[_siOrder++ % infos.Count]);
                    }
                    else
                    {
                        int index = _siSuccess % infos.Count;
                        bool isAdd = sg.IsAloneAdd || index == infos.Count - 1;

                        //Console.WriteLine("- - - - - - - - - -");
                        //Console.WriteLine($"索引：{_siSuccess}，{index}，循环次数是否加一：{isAdd}");
                        if (await ImageCompare(infos[index], isAdd))
                        {
                            _vm.ScriptInterval = 5000;
                            _siSuccess++;
                            //Console.WriteLine($"索引下标加一：{_siSuccess}");
                        }
                    }
                }
                else
                {
                    await ImageCompare(infos[0]);
                }  
            }
            catch (Exception ex)
            {
                _vm.ScriptInterval = 5000;
                MyGM.RecordException(ex, "执行图像脚本时异常");
            }
            finally
            {
                //Debug.WriteLine($"自适应间隔:{_vm.ScriptInterval / 1000}秒。");
                _scriptTimer.Interval = _vm.ScriptInterval;
                _imageScriptLock.Release(); // 释放锁
            }
        }

        /// <summary>
        /// 使用脚本中的图像和当前Scrcpy的截图进行比对。
        /// </summary>
        /// <param name="info">脚本信息类实例。</param>
        /// <param name="isAddCount">对比成功后是否增加计数。</param>
        /// <returns>图形对比是否通过。</returns>
        private async Task<bool> ImageCompare(ScriptInfo info, bool isAddCount = true)
        {
            CroppedBitmap scWindow = await GetScriptImg(ScHandle, _mainHandle, info.CropRect);
            if (scWindow == null)
            {
                popMid.Show("获取Scrcpy脚本截图失败。");
                return false;
            }

            //double rgb = OpenCv.CalculateRgbHist($@"{MV.ScriptPath}\{info.ImgName}", scWindow);
            //double hsv = OpenCv.CalculateHsvHist($@"{MV.ScriptPath}\{info.ImgName}", scWindow);
            //double hu = OpenCv.CalculateHuMoment($@"{MV.ScriptPath}\{info.ImgName}", scWindow);
            //double ssim = OpenCv.CompareSSIM($@"{MV.ScriptPath}\{info.ImgName}", scWindow);
            //double sift = OpenCv.CalculateSIFT($@"{MV.ScriptPath}\{info.ImgName}", scWindow);
            double orb = OpenCv.CalculateORB($@"{MV.ScriptPath}\{info.ImgName}", scWindow);
            //double result = hsv > 0 ? (hsv + orb) / 2 : orb;
            double result = orb;

            //Console.WriteLine($"rgb:{rgb:F2},hsv:{hsv:F2},hu:{hu:F2},ssim:{ssim:F2},sift:{sift:F2},orb:{orb:F2}");
            //Console.WriteLine($"- - - - - - - - - - - - - - - -");
            _debug.Add($"orb:{orb:F2}，最终：{result:F2}，阈值：{info.Similarity / 100}，是否通过：{result >= info.Similarity / 100}");
            //Console.WriteLine($"hsv:{hsv:F2},orb:{orb:F2}，最终：{result:F2}，阈值：{info.Similarity / 100}，是否通过：{result >= info.Similarity / 100}");
            
            if (result >= info.Similarity / 100)
            {
                if (info.IsAdaptive)
                {
                    _vm.ScriptInterval = _vm.Running * 1000 * 0.9;//每次将上一次间隔的90%作为自适应间隔
                }
                if (isAddCount)
                {
                    _vm.CurCycleNum++;
                    _stopwatch.Restart();
                }
               
                await ExecuteClickScript(info, ScHandle);
                return true;
            }
            else
            {
                //每次未匹配上增加间隔时间，间隔达到12秒以上时重新从5秒间隔开始
                double interval = _vm.ScriptInterval * 1.5;
                _vm.ScriptInterval = interval > 12000 ? 5000 : interval;
                return false;
            }
        }

        /// <summary>
        /// 图像比对通过，进行脚本下一轮循环。
        /// </summary>
        /// <param name="info">脚本信息。</param>
        //private async Task NextImageScript(ScriptInfo info)
        //{
        //    if (info.IsAdaptive)
        //    {
        //        _vm.ScriptInterval = _vm.Running * 1000 * 0.9;//每次将上一次间隔的90%作为自适应间隔
        //    }
        //
        //    _vm.CycleNum++;
        //    _stopwatch.Restart();
        //    await ExecuteClickScript(info, ScHandle);
        //}

        /// <summary>
        /// 执行点击操作，点击的实际位置会根据ScRatioX/ScRatioY进行调整，但有黑边时仍会偏移。
        /// </summary>
        /// <param name="info">当前要执行的脚本。</param>
        /// <param name="scHandle">Scrcpy的句柄。</param>
        /// <param name="isCheckExit">是否需要检查退出。</param>
        private async Task ExecuteClickScript(ScriptInfo info, IntPtr scHandle, bool isCheckExit = true)
        {
            foreach (PointInfo pi in info.PointInfos)
            {
                if (pi.LastTime > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(pi.LastTime));
                }

                //保存的坐标数据都是以默认尺寸为准，使用时需要乘以当前尺寸和默认尺寸的比例
                int x = (int)(pi.Pt.X * ScRatioX);
                int y = (int)(pi.Pt.Y * ScRatioY);

                SendClick(scHandle, x, y);
                //Debug.WriteLine($"比例:({ScRatioX},{ScRatioY})，实际点击:({x},{y})。");
            }

            //如果为有限次循环，检查是否到循环次数
            if (isCheckExit && info.IsLimited && info.CycleNumber <= _vm.CurCycleNum)
            {
                await Dispatcher.BeginInvoke(StopScriptAndRecord);
            }
        }

        /// <summary>
        /// 获得脚本的截图，Scrcpy嵌入在WPF窗体中，所以要判断主窗体是否最小化，
        /// 最小化时要显示窗口但不激活，否则最小化时窗口不会渲染。
        /// </summary>
        /// <param name="scHandle">Scrcpy的句柄。</param>
        /// <param name="mainHandle">承载Scrcpy程序的句柄。</param>
        /// <param name="rect">脚本图片的裁剪区域。</param>
        /// <returns>返回System.Windows.Media.Imaging.CroppedBitmap类型的裁剪图。</returns>
        private async Task<CroppedBitmap> GetScriptImg(IntPtr scHandle, IntPtr mainHandle, Rect originalRect)
        {
            //判断主窗口是否最小化
            if (DLL.IsIconic(mainHandle))
            {
                DLL.ShowWindow(mainHandle, SW.SW_SHOWNOACTIVATE);
                await Task.Delay(50);
            }

            //获得Scrcpy窗口截图，并计算截取的区域
            BitmapSource scBitmap = MyGM.GetHandleImage(scHandle);
            Rect cropRect = new(
                originalRect.X * ScRatioX,
                originalRect.Y * ScRatioY,
                originalRect.Width * ScRatioX,
                originalRect.Height * ScRatioY);

            //Debug.WriteLine($"非工作区：{SystemParameters.WindowNonClientFrameThickness.Left},{SystemParameters.WindowNonClientFrameThickness.Right}，" + 
            //    $"起点：{cropRect.X},{cropRect.Y}，" +
            //    $"宽高：{cropRect.Width},{cropRect.Height}，" +
            //    $"终点：{cropRect.X + cropRect.Width},{cropRect.Y + cropRect.Height}，" +
            //    $"图片大小：{scBitmap.Width},{scBitmap.Height}，" +
            //    $"程序大小：{MV.MainWin.Width},{MV.MainWin.Height}");

            //检查范围，获得最终的截图区域
            CheckRect(ref cropRect, scBitmap.Width, scBitmap.Height);
            CroppedBitmap temp = new(scBitmap, MyGM.ConvertToInt32Rect(cropRect));

            return temp;
        }

        /// <summary>
        /// 如果要截取的区域超出sc图片的范围，则将值限定在范围内。
        /// 用户的截图不能超出Scrcpy的范围，因为脚本临时图片始终会被限制在Scrcpy的窗口当中。
        /// </summary>
        /// <param name="rect">说明截图范围的矩形。</param>
        /// <param name="maxWidth">最大宽度。</param>
        /// <param name="maxHeight">最大高度。</param>
        private void CheckRect(ref Rect rect, double maxWidth, double maxHeight)
        {
            if (rect.X + rect.Width > maxWidth)
            {
                rect.Width -= rect.X + rect.Width - maxWidth;
            }
            if (rect.Y + rect.Height > maxHeight)
            {
                rect.Height -= rect.Y + rect.Height - maxHeight;
            }
            if (rect.X < 0)
            {
                rect.X = 0;
            }
            if (rect.Y < 0)
            {
                rect.Y = 0;
            }
        }

        /// <summary>
        /// 在一个窗口中显示脚本和对比图像。
        /// </summary>
        /// <param name="infos">当前执行的脚本。</param>
        private async void ImgCompare(List<ScriptInfo> infos)
        {
            try
            {
                List<BitmapSource> bmps = [];
                foreach (ScriptInfo si in infos)
                {
                    if (si.ImgName == "")
                    {
                        bmps.Add(null);
                    }
                    else
                    {
                        bmps.Add(await GetScriptImg(ScHandle, _mainHandle, si.CropRect));
                    }
                }

                ShowImg showImg = new(infos, bmps, _debug);
                showImg.ShowDialog();
            }
            catch (Exception ex)
            {
                MyGM.ShwError($"尝试对比图像时异常：{ex}");
            }
        }

        /// <summary>
        /// 转发SC相关的快捷键，Alt+键。Alt按下及之后的键都应该是系统键，除了最后的Alt抬起。
        /// </summary>
        /// <param name="key">键值。</param>
        private void PostShortcutsTwo(uint key)
        {
            if (ScHandle == IntPtr.Zero) return;
            DLL.PostMessage(ScHandle, WM.SYSKEYDOWN, (IntPtr)VK_LMENU, IntPtr.Zero);
            DLL.PostMessage(ScHandle, WM.SYSKEYDOWN, (IntPtr)key, IntPtr.Zero);
            DLL.PostMessage(ScHandle, WM.SYSKEYUP, (IntPtr)key, IntPtr.Zero);
            DLL.PostMessage(ScHandle, WM.KEYUP, (IntPtr)VK_LMENU, IntPtr.Zero);
        }

        /// <summary>
        /// 转发SC相关的快捷键，Alt+Shift+键。
        /// </summary>
        /// <param name="key">键值。</param>
        private void PostShortcutsThree(uint key)
        {
            if (ScHandle == IntPtr.Zero) return;
            DLL.PostMessage(ScHandle, WM.SYSKEYDOWN, (IntPtr)VK_LMENU, IntPtr.Zero);
            DLL.PostMessage(ScHandle, WM.SYSKEYDOWN, (IntPtr)VK_SHIFT, IntPtr.Zero);
            DLL.PostMessage(ScHandle, WM.SYSKEYDOWN, (IntPtr)key, IntPtr.Zero);
            DLL.PostMessage(ScHandle, WM.SYSKEYUP, (IntPtr)key, IntPtr.Zero);
            DLL.PostMessage(ScHandle, WM.SYSKEYUP, (IntPtr)VK_SHIFT, IntPtr.Zero);
            DLL.PostMessage(ScHandle, WM.KEYUP, (IntPtr)VK_LMENU, IntPtr.Zero);
        }


    }
}
