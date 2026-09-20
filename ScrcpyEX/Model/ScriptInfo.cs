using ScrcpyEX.MyDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ScrcpyEX.Model
{
    /// <summary>
    /// 单脚本信息类，保存脚本相关信息。
    /// </summary>
    public class ScriptInfo : ScriptItem, ICloneable
    {
        /// <summary>
        /// true--进行有限次的循环；false--进行无限次的循环，直到手动停止。
        /// </summary>
        public bool IsLimited { get; set; } = false;

        /// <summary>
        /// 指定有限次循环的次数。
        /// </summary>
        public int CycleNumber { get; set; } = 10;

        /// <summary>
        /// 是否为计时脚本，true--指定时间后进行下一次脚本（计时脚本）；false--指定区域出现相似图片后进行下一次脚本（图像脚本）。
        /// </summary>
        public bool IsTime { get; set; } = true;

        /// <summary>
        /// 指定每次循环的间隔，单位秒。
        /// </summary>
        public int CycleInterval { get; set; } = 100;

        /// <summary>
        /// 用于对比的脚本图片的名称，默认为空字符串。
        /// </summary>
        public string ImgName { get; set; } = "";

        /// <summary>
        /// 说明局部图片位置大小的矩形。
        /// </summary>
        public Rect CropRect { get; set; } = new();

        /// <summary>
        /// 图像脚本是否使用自适应间隔。
        /// </summary>
        public bool IsAdaptive { get; set; } = true;

        /// <summary>
        /// 阈值相似度，范围限定在0~100。
        /// </summary>
        public double Similarity
        {
            get { return _similarity; }
            set
            {
                _similarity = value < 0 ? 0 : (value > 100 ? 100 : value);
            }
        }
        private double _similarity = 80.0;

        /// <summary>
        /// 录制的坐标点信息列表。
        /// </summary>
        public List<PointInfo> PointInfos { get; set; } = [];

        /// <summary>
        /// 脚本信息类默认构造函数。
        /// </summary>
        public ScriptInfo() { }

        /// <summary>
        /// 使用坐标点信息列表构造类。
        /// </summary>
        /// <param name="points">录制的坐标点信息列表。</param>
        public ScriptInfo(List<PointInfo> points)
        {
            Name = $"脚本{DateTime.Now:yyMMddhhmmss}";
            PointInfos = [.. points];//创建独立的数据副本
        }

        /// <summary>
        /// 使用坐标点信息列表构造类。
        /// </summary>
        /// <param name="points">录制的坐标点信息列表。</param>
        /// <param name="cropImg">。</param>
        /// <param name="cropRect">。</param>
        public ScriptInfo(List<PointInfo> points, CroppedBitmap cropImg, Rect cropRect)
        {
            Name = $"脚本{DateTime.Now:yyMMddhhmmss}";
            IsTime = false;
            ImgName = $"Pic{DateTime.Now:yyMMddhhmm_ssfff}.png";
            CropRect = cropRect;
            PointInfos = [.. points];

            MyGM.SavaImg(ImgName, cropImg);
        }

        object ICloneable.Clone() => Clone(); //显式接口实现

        /// <summary>
        /// 公共强类型方法，创建当前实例副本的新对象。
        /// </summary>
        /// <returns>返回此实例副本的新对象。</returns>
        public ScriptInfo Clone() => new()
        {
            Name = Name,
            IsLimited = IsLimited,
            CycleNumber = CycleNumber,
            IsTime = IsTime,
            CycleInterval = CycleInterval,
            ImgName = ImgName,
            CropRect = CropRect,
            IsAdaptive = IsAdaptive,
            Similarity = Similarity,
            PointInfos = PointInfos,
        };


    }
}
