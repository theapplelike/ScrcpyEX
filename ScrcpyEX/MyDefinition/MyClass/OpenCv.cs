using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.Quality;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Resources.ResXFileRef;

namespace ScrcpyEX.MyDefinition.MyClass
{
    internal class OpenCv
    {
        /// <summary>
        /// BitmapSource 转 Mat 时的颜色模式。
        /// </summary>
        public enum ColorMode
        {
            /// <summary>单通道灰度图，供 SSIM 等使用。</summary>
            Gray,
            /// <summary>三通道 BGR 图，供 RgbHist 等使用。</summary>
            Bgr,
            /// <summary>四通道 BGRA 图（保留 Alpha）。</summary>
            Bgra
        }

        /// <summary>
        /// 使用结构相似性指标（SSIM）比较两张图片的相似性，速度比QualitySSIM更快。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="path2">第二张图片的路径。</param>
        /// <returns>SSIM值在0.0到1.0之间，值越大表示相似度越高。</returns>
        public static double CompareSSIM(string path1, string path2)
        {
            Stopwatch st = Stopwatch.StartNew();
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = Cv2.ImRead(path2, ImreadModes.Grayscale);
            double reult = CompareSSIM(img1, img2);

            Debug.WriteLine($"SSIM:{reult}，耗时：{st.ElapsedMilliseconds}ms**");
            return reult;
        }

        /// <summary>
        /// 使用结构相似性指标（SSIM）比较两张图片的相似性。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="bitmap">第二张图片。</param>
        /// <returns>SSIM值在0.0到1.0之间，值越大表示相似度越高。</returns>
        public static double CompareSSIM(string path1, BitmapSource bitmap)
        {
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = ConvertBitmapSourceToMat(bitmap);

            return CompareSSIM(img1, img2);
        }

        /// <summary>
        /// 使用结构相似性指标（SSIM）比较两张图片的相似性。
        /// </summary>
        /// <param name="img1">OpenCv的Mat类型。</param>
        /// <param name="img2">OpenCv的Mat类型。</param>
        /// <returns>SSIM值在0.0到1.0之间，值越大表示相似度越高。</returns>
        public static double CompareSSIM(Mat img1, Mat img2)
        {
            // 确保图像尺寸一致
            if (img1.Size() != img2.Size())
                Cv2.Resize(img2, img2, img1.Size());

            // 转换为 float 类型以提升计算精度
            img1.ConvertTo(img1, MatType.CV_32F);
            img2.ConvertTo(img2, MatType.CV_32F);

            // 高斯模糊参数
            Size gaussianSize = new(11, 11);
            double sigma = 1.5;

            // 计算均值 mu1 和 mu2
            Mat mu1 = new(), mu2 = new();
            Cv2.GaussianBlur(img1, mu1, gaussianSize, sigma);
            Cv2.GaussianBlur(img2, mu2, gaussianSize, sigma);

            // 计算方差和协方差
            Mat mu1Sq = mu1.Mul(mu1);
            Mat mu2Sq = mu2.Mul(mu2);
            Mat mu1Mu2 = mu1.Mul(mu2);

            Mat sigma1Sq = new(), sigma2Sq = new(), sigma12 = new();
            Cv2.GaussianBlur(img1.Mul(img1), sigma1Sq, gaussianSize, sigma);
            sigma1Sq -= mu1Sq; // σ1² = E(img1²) - μ1²

            Cv2.GaussianBlur(img2.Mul(img2), sigma2Sq, gaussianSize, sigma);
            sigma2Sq -= mu2Sq; // σ2² = E(img2²) - μ2²

            Cv2.GaussianBlur(img1.Mul(img2), sigma12, gaussianSize, sigma);
            sigma12 -= mu1Mu2; // σ12 = E(img1*img2) - μ1μ2

            // 计算常数项
            double c1 = Math.Pow(0.01 * 255, 2);
            double c2 = Math.Pow(0.03 * 255, 2);

            // 计算 SSIM 分子和分母
            Mat numerator = (2 * mu1Mu2 + new Scalar(c1)).Mul(2 * sigma12 + new Scalar(c2));
            Mat denominator = (mu1Sq + mu2Sq + new Scalar(c1)).Mul(sigma1Sq + sigma2Sq + new Scalar(c2));

            // 计算 SSIM 均值
            Mat ssimMap = new();
            Cv2.Divide(numerator, denominator, ssimMap);
            Scalar ssimScalar = Cv2.Mean(ssimMap);

            return ssimScalar.Val0; // 返回 SSIM 平均值
        }

        /// <summary>
        /// 将BitmapSource类型及其派生类型的图片转换为OpenCv可以处理的Mat类型。
        /// </summary>
        /// <param name="bitmap">BitmapSource类型及其派生类型的图片。</param>
        /// <param name="mode">目标 Mat 的颜色模式，默认为灰度。</param>
        /// <returns>Mat类型。</returns>
        public static Mat ConvertBitmapSourceToMat(BitmapSource bitmap, ColorMode mode = ColorMode.Gray)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));
            BitmapSource source = bitmap;

            // 1. 转换为 BGRA32 格式
            if (bitmap.Format != PixelFormats.Bgra32)
            {
                FormatConvertedBitmap converted = new();
                converted.BeginInit();
                converted.Source = bitmap;
                converted.DestinationFormat = PixelFormats.Bgra32;
                converted.EndInit();
                converted.Freeze();
                source = converted;
            }

            // 2. 提取像素数据
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = (width * source.Format.BitsPerPixel + 7) / 8;//32 bits per pixel (BGRA32)
            byte[] pixelData = new byte[height * stride];
            source.CopyPixels(pixelData, stride, 0);

            // 3. 使用 Mat.FromPixelData 创建 Mat
            Mat bgra;
            unsafe
            {
                fixed (byte* pData = pixelData)
                {
                    using Mat tmp = Mat.FromPixelData(
                        rows: height,
                        cols: width,
                        type: MatType.CV_8UC4,
                        data: (nint)pData,  // 转换为 nint 类型
                        step: stride        // 步长以字节为单位
                    );
                    bgra = tmp.Clone();     // 克隆以持久化数据（避免指针失效）
                }
            }

            //按需转换，转换时输出到新 Mat，不要就地转换
            switch (mode)
            {
                case ColorMode.Bgra: return bgra;
                case ColorMode.Bgr:
                    {
                        Mat bgr = new();
                        Cv2.CvtColor(bgra, bgr, ColorConversionCodes.BGRA2BGR);
                        bgra.Dispose();
                        return bgr;
                    }
                case ColorMode.Gray:
                default:
                    {
                        Mat gray = new();
                        Cv2.CvtColor(bgra, gray, ColorConversionCodes.BGRA2GRAY);
                        bgra.Dispose();
                        return gray;
                    }
            }
        }

        /// <summary>
        /// 颜色直方图描述的是颜色分布比例，缩放后分布基本不变，但计算量可以降低几个数量级。
        /// 如果宽或高大于256像素，则按比例缩放图像，宽或高最长不会超过256像素。
        /// </summary>
        /// <param name="src">Mat类型。</param>
        /// <param name="maxWidth">允许的最大宽度，默认256像素。</param>
        /// <param name="maxHeight">允许的最大高度，默认256像素。</param>
        /// <returns>返回一个新的Mat对象，无论是否进行了缩放。</returns>
        private static Mat ResizeForHist(Mat src, int maxWidth = 256, int maxHeight = 256)
        {
            // 原图足够小，不放大
            if (src.Width <= maxWidth && src.Height <= maxHeight)
            {
                return src.Clone();
            }

            // 计算缩放比例，保证宽高都不超过最大尺寸
            double scale = Math.Min((double)maxWidth / src.Width, (double)maxHeight / src.Height);

            int newWidth = Math.Max(1, (int)Math.Round(src.Width * scale));
            int newHeight = Math.Max(1, (int)Math.Round(src.Height * scale));

            Mat resized = new();
            Cv2.Resize(src, resized, new Size(newWidth, newHeight), 0, 0, InterpolationFlags.Area);
            return resized;
        }

        /// <summary>
        /// 结构相似性指数，比较两张图的局部结构（通常是一个 11x11 的像素块，像个小滑窗扫过全图）。
        /// 已精准对齐的视频帧/图像对比，极度惧怕平移/旋转。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="path2">第二张图片的路径。</param>
        /// <returns>
        /// <para>&gt; 0.95：几乎完美复原。</para>
        /// <para>0.90 ~ 0.95：高质量，细微差异。</para>
        /// <para>0.80 ~ 0.90：中等质量，可见差异。</para>
        /// <para>&lt; 0.80：明显失真或结构改变。</para>
        /// </returns>
        public static double CalculateSSIM(string path1, string path2)
        {
            Stopwatch st = Stopwatch.StartNew();
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = Cv2.ImRead(path2, ImreadModes.Grayscale);

            // 确保图像尺寸一致
            if (img1.Size() != img2.Size())
                Cv2.Resize(img2, img2, img1.Size());

            // 计算 SSIM
            using QualitySSIM ssimObj = QualitySSIM.Create(img1);
            Scalar ssimScalar = ssimObj.Compute(img2);
            double ssim = ssimScalar.Val0;

            Debug.WriteLine($"SSIM: {ssim}，耗时：{st.ElapsedMilliseconds}ms");
            return ssim;
        }

        /// <summary>
        /// 峰值信噪比，基于像素级的误差计算，适用于压缩质量监控、去噪评估、原图比较。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="path2">第二张图片的路径。</param>
        /// <returns>
        /// <para>&gt; 40 dB：极好（几乎无损，肉眼无法分辨）。</para>
        /// <para>30 dB ~ 40 dB：良好（肉眼可能极难察觉细微差异，常见于高质量压缩）。</para>
        /// <para>20 dB ~ 30 dB：尚可（肉眼可见轻微失真，但可接受）。</para>
        /// <para>&lt; 20 dB：较差（失真明显）。</para>
        /// </returns>
        public static double CalculatePSNR(string path1, string path2)
        {
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = Cv2.ImRead(path2, ImreadModes.Grayscale);

            // 确保图像尺寸一致
            if (img1.Size() != img2.Size())
                Cv2.Resize(img2, img2, img1.Size());

            // 计算PSNR
            double psnr = Cv2.PSNR(img1, img2);

            Debug.WriteLine($"PSNR: {psnr}");
            return psnr;
        }

        /// <summary>
        /// 使用RGB颜色直方图对比图像相似度。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="path2">第二张图片的路径。</param>
        /// <returns>返回值在 [0,1] 之间，代表两张直方图的线性相似度（如果为负数说明极端病态）。</returns>
        public static double CalculateRgbHist(string path1, string path2)
        {
            Stopwatch st = Stopwatch.StartNew();
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Color); // 必须读彩色图
            using Mat img2 = Cv2.ImRead(path2, ImreadModes.Color);
            double similarity = CalculateRgbHist(img1, img2);

            Debug.WriteLine($"RGB-Hist:{similarity}，耗时：{st.ElapsedMilliseconds}ms");
            return Math.Max(0, similarity);
        }

        /// <summary>
        /// 使用RGB颜色直方图对比图像相似度。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="bitmap">第二张图片。</param>
        /// <returns>返回值在 [0,1] 之间，代表两张直方图的线性相似度（如果为负数说明极端病态）。</returns>
        public static double CalculateRgbHist(string path1, BitmapSource bitmap)
        {
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Color);
            using Mat img2 = ConvertBitmapSourceToMat(bitmap, ColorMode.Bgr);

            return CalculateRgbHist(img1, img2);
        }

        /// <summary>
        /// RGB颜色直方图，统计图像中各种颜色出现的频率。适用于平移、旋转后的图像对比或基于主色调的图像检索。
        /// 但是完全丢失空间信息，直方图只关心“有什么颜色”，完全不关心“颜色在哪儿”。
        /// </summary>
        /// <param name="img1">OpenCv的Mat类型。</param>
        /// <param name="img2">OpenCv的Mat类型。</param>
        /// <returns>
        /// <para>&gt; 0.90：高度相似，可能有轻微光照、压缩或尺寸变化。</para>
        /// <para>0.70 ~ 0.90：中等相似，可能背景相同但前景不同，或有明显色彩偏移。</para>
        /// <para>0.50 ~ 0.70：相似度较低，颜色分布有明显不同。</para>
        /// <para>&lt; 0.50：差异很大，通常可视为不匹配。</para>
        /// </returns>
        public static double CalculateRgbHist(Mat img1, Mat img2)
        {
            using Mat resized1 = ResizeForHist(img1);
            using Mat resized2 = ResizeForHist(img2);

            // 计算直方图 (统计颜色分布)
            using Mat hist1 = new();
            using Mat hist2 = new();
            int[] histSize = [8, 8, 8];        // 3D 直方图，每通道 8 bin
            Rangef[] ranges = [new(0, 256), new(0, 256), new(0, 256)];

            Cv2.CalcHist([resized1], [0], null, hist1, 1, histSize, ranges);
            Cv2.CalcHist([resized2], [0], null, hist2, 1, histSize, ranges);

            // 使用相关性算法比较，Correl 返回值在 [-1,1] 之间
            return Cv2.CompareHist(hist1, hist2, HistCompMethods.Correl);
        }

        /// <summary>
        /// 使用HSV颜色直方图对比图像相似度。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="path2">第二张图片的路径。</param>
        /// <returns>返回值在 [0,1] 之间，代表两张直方图的线性相似度。</returns>
        public static double CalculateHsvHist(string path1, string path2)
        {
            Stopwatch st = Stopwatch.StartNew();
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Color);
            using Mat img2 = Cv2.ImRead(path2, ImreadModes.Color);
            double similarity = CalculateHsvHist(img1,img2);

            Debug.WriteLine($"HSV-Hist:{similarity}，耗时：{st.ElapsedMilliseconds}ms");
            return Math.Max(0, similarity);
        }

        /// <summary>
        /// 使用HSV颜色直方图对比图像相似度。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="bitmap">第二张图片。</param>
        /// <returns>返回值在 [0,1] 之间，代表两张直方图的线性相似度。</returns>
        public static double CalculateHsvHist(string path1, BitmapSource bitmap)
        {
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Color);
            using Mat img2 = ConvertBitmapSourceToMat(bitmap, ColorMode.Bgr);

            return CalculateHsvHist(img1, img2);
        }

        /// <summary>
        /// HSV颜色直方图，统计 Hue（色调）、Saturation（饱和度）、Value（明度）通道的频次。但一般会丢弃明度。
        /// 适用于平移、旋转后的图像对比或基于主色调的图像检索，能抵抗阴影、反光、曝光度变化带来的干扰。
        /// 但依然完全丢失空间信息。
        /// </summary>
        /// <param name="img1">OpenCv的Mat类型。</param>
        /// <param name="img2">OpenCv的Mat类型。</param>
        /// <returns>
        /// <para>&gt; 0.90：高度相似，可能有轻微光照、压缩或尺寸变化。</para>
        /// <para>0.70 ~ 0.90：中等相似，可能背景相同但前景不同，或有明显色彩偏移。</para>
        /// <para>0.50 ~ 0.70：相似度较低，颜色分布有明显不同。</para>
        /// <para>&lt; 0.50：差异很大，通常可视为不匹配。</para>
        /// </returns>
        public static double CalculateHsvHist(Mat img1, Mat img2)
        {
            using Mat resized1 = ResizeForHist(img1);
            using Mat resized2 = ResizeForHist(img2);

            // 将 BGR 图像转换为 HSV 颜色空间
            using Mat hsv1 = new();
            using Mat hsv2 = new();
            Cv2.CvtColor(resized1, hsv1, ColorConversionCodes.BGR2HSV);
            Cv2.CvtColor(resized2, hsv2, ColorConversionCodes.BGR2HSV);

            // 计算 H（色相）和 S（饱和度）通道的直方图，HSV 的 H 通道范围 0~180，S 通道 0~256
            // H 的值高：能捕捉细微差异，对噪声敏感，可能因光照、压缩等导致同一物体色相微小偏移而误判不相似
            // H 的值低：抗噪声能力越强，会丢失细节差异，特别是小面积前景颜色差异可能被忽略
            using Mat hist1 = new();
            using Mat hist2 = new();
            int[] histSize = [15, 8];
            Rangef[] ranges = [new Rangef(0, 180), new Rangef(0, 256)];

            Cv2.CalcHist([hsv1], [0, 1], null, hist1, 2, histSize, ranges);
            Cv2.CalcHist([hsv2], [0, 1], null, hist2, 2, histSize, ranges);

            // 计算相关度
            return Cv2.CompareHist(hist1, hist2, HistCompMethods.Correl);
        }

        /// <summary>
        /// 使用 Hu矩 对比图像相似度。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="path2">第二张图片的路径。</param>
        /// <returns>返回值在 [0,1] 之间，越大越相似。</returns>
        public static double CalculateHuMoment(string path1, string path2)
        {
            Stopwatch st = Stopwatch.StartNew();
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = Cv2.ImRead(path2, ImreadModes.Grayscale);
            double similarity = CalculateHuMoment(img1, img2);

            Debug.WriteLine($"HuMoment:{similarity}，耗时：{st.ElapsedMilliseconds}ms");
            return similarity;
        }

        /// <summary>
        /// 使用 Hu矩 对比图像相似度。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="bitmap">第二张图片。</param>
        /// <returns>返回值在 [0,1] 之间，越大越相似。</returns>
        public static double CalculateHuMoment(string path1, BitmapSource bitmap)
        {
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = ConvertBitmapSourceToMat(bitmap);

            return CalculateHuMoment(img1, img2);
        }

        /// <summary>
        /// Hu矩 是一组专门用于描述图像“形状”的7个不变特征量，能提取轮廓特征，并且这些特征对平移、旋转、缩放具有完全的数学不变性。
        /// 但完全忽略颜色和纹理，局部裁剪/遮挡会导致分数断崖下跌。如果缩放图像包含文字，文字的缩放会给周围带来大量的马赛克，会导致分数暴跌。
        /// </summary>
        /// <param name="img1">OpenCv的Mat类型。</param>
        /// <param name="img2">OpenCv的Mat类型。</param>
        /// <returns>
        /// <para>&gt; 0.90：轮廓特征高度相似。</para>
        /// <para>0.80 ~ 0.90：包含了原图特征。</para>
        /// <para>0.65 ~ 0.80：部分包含原图特征。</para>
        /// <para>&lt; 0.65：形状上有明显差异（比如文字被拉伸变形了）。</para>
        /// </returns>
        public static double CalculateHuMoment(Mat img1, Mat img2)
        {
            // 1. 二值化 (去除抗锯齿影响)
            using Mat binary1 = new();
            using Mat binary2 = new();
            Cv2.Threshold(img1, binary1, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            Cv2.Threshold(img2, binary2, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

            // 2. 计算矩
            Moments moments1 = Cv2.Moments(binary1);
            Moments moments2 = Cv2.Moments(binary2);

            // 3. 计算 Hu 矩
            double[] hu1 = moments1.HuMoments();
            double[] hu2 = moments2.HuMoments();

            // 4. 计算 Hu 矩之间的距离 (对数处理适应数量级差异)
            double distance = 0;
            for (int i = 0; i < 7; i++)
            {
                double a = Math.Log(Math.Abs(hu1[i]) + 1e-10);
                double b = Math.Log(Math.Abs(hu2[i]) + 1e-10);
                distance += Math.Pow(a - b, 2);
            }
            distance = Math.Sqrt(distance); // 欧氏距离

            // 5. 将距离转为 0~1 的相似度，距离越小越相似
            return 1.0 / (1.0 + distance);
        }

        /// <summary>
        /// 尺度不变特征变换，是局部特征检测，在图像被旋转、缩放、平移时有很好的识别率，部分被遮挡有较好的识别率。
        /// 但是完全不认颜色，计算时彩图会转化为灰度图，对模糊和光滑表面失效，计算量大速度慢。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="path2">第二张图片的路径。</param>
        /// <param name="ratioThreshold">Lowe's 比率测试阈值。</param>
        /// <returns>
        /// <para>&gt; 0.90：高度相似，原图级别的旋转、平移，或特征能匹配上的缩放。</para>
        /// <para>0.80 ~ 0.90：大部分相似，部分缩放、局部截图、轻微遮挡。</para>
        /// <para>0.65 ~ 0.80：局部相似，整体不相似，能召回的特征点大幅下降。</para>
        /// <para>&lt; 0.65：大幅度的拉伸、缩放等导致特征匹配度低，或完全不相似。</para>
        /// </returns>
        public static double CalculateSIFT(string path1, string path2, double ratioThreshold = 0.85)
        {
            Stopwatch st = Stopwatch.StartNew();
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = Cv2.ImRead(path2, ImreadModes.Grayscale);

            // 对小尺寸图像进行【双三次插值放大】 (解决 10% 缩放、极小微缩导致特征点丢失的问题)
            using Mat use1 = new();
            using Mat use2 = new();
            int minDim = 80; // 强制目标最小边长为 80 像素

            double scaleX1 = (img1.Width < minDim) ? (double)minDim / img1.Width : 1.0;
            double scaleY1 = (img1.Height < minDim) ? (double)minDim / img1.Height : 1.0;
            double scaleX2 = (img2.Width < minDim) ? (double)minDim / img2.Width : 1.0;
            double scaleY2 = (img2.Height < minDim) ? (double)minDim / img2.Height : 1.0;

            if (scaleX1 != 1.0 || scaleY1 != 1.0)
                Cv2.Resize(img1, use1, new Size((int)(img1.Width * scaleX1), (int)(img1.Height * scaleY1)), 0, 0, InterpolationFlags.Cubic);
            else
                img1.CopyTo(use1);

            if (scaleX2 != 1.0 || scaleY2 != 1.0)
                Cv2.Resize(img2, use2, new Size((int)(img2.Width * scaleX2), (int)(img2.Height * scaleY2)), 0, 0, InterpolationFlags.Cubic);
            else
                img2.CopyTo(use2);

            using SIFT sift = SIFT.Create();
            double result = CalculateFeatureMatchInternal(use1, use2, sift, NormTypes.L2, "SIFT", ratioThreshold);
            Debug.WriteLine($"SIFT耗时：{st.ElapsedMilliseconds}ms");
            return result;
            //return CalculateFeatureMatchInternal(img1, img2, sift, NormTypes.L2, "SIFT", ratioThreshold);
        }

        /// <summary>
        /// ORB（Oriented FAST and Rotated BRIEF），各项性能和SIFT相似，在降低了准确度的同时提升了速度。
        /// 限定了图形最小边为80像素，在极小缩放时可以比SIFT有更好的表现，平移时表现差距略大。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="path2">第二张图片的路径。</param>
        /// <param name="ratioThreshold">Lowe's 比率测试阈值。</param>
        /// <returns>
        /// <para>&gt; 0.85：高度相似，原图级别的旋转、平移，或特征能匹配上的缩放。</para>
        /// <para>0.70 ~ 0.85：大部分相似，部分缩放、局部截图、轻微遮挡。</para>
        /// <para>0.60 ~ 0.70：局部相似，能召回的特征点大幅下降，但仍可通过。</para>
        /// <para>&lt; 0.60：边缘状态、弱匹配，图形发生较大的形变或内点过少。</para>
        /// </returns>
        public static double CalculateORB(string path1, string path2, double ratioThreshold = 0.75)
        {
            Stopwatch st = Stopwatch.StartNew();
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = Cv2.ImRead(path2, ImreadModes.Grayscale);

            // 对小尺寸图像进行【双三次插值放大】 (解决 10% 缩放、极小微缩导致特征点丢失的问题)
            using Mat use1 = new();
            using Mat use2 = new();
            int minDim = 80; // 强制目标最小边长为 80 像素
            
            double scaleX1 = (img1.Width < minDim) ? (double)minDim / img1.Width : 1.0;
            double scaleY1 = (img1.Height < minDim) ? (double)minDim / img1.Height : 1.0;
            double scaleX2 = (img2.Width < minDim) ? (double)minDim / img2.Width : 1.0;
            double scaleY2 = (img2.Height < minDim) ? (double)minDim / img2.Height : 1.0;
            
            if (scaleX1 != 1.0 || scaleY1 != 1.0)
                Cv2.Resize(img1, use1, new Size((int)(img1.Width * scaleX1), (int)(img1.Height * scaleY1)), 0, 0, InterpolationFlags.Cubic);
            else
                img1.CopyTo(use1);
            
            if (scaleX2 != 1.0 || scaleY2 != 1.0)
                Cv2.Resize(img2, use2, new Size((int)(img2.Width * scaleX2), (int)(img2.Height * scaleY2)), 0, 0, InterpolationFlags.Cubic);
            else
                img2.CopyTo(use2);

            // 使用两张图的等效边长计算特殊点数量 
            double areaSqrt1 = Math.Sqrt(img1.Width * img1.Height);
            double areaSqrt2 = Math.Sqrt(img2.Width * img2.Height);
            double minArea = Math.Min(areaSqrt1, areaSqrt2);
            double maxArea = Math.Max(areaSqrt1, areaSqrt2);
            int targetFeatures = (int)Math.Min(2000, Math.Max(300, maxArea + minArea));

            using ORB orb = ORB.Create(nFeatures: targetFeatures, edgeThreshold: 15);
            double result = CalculateFeatureMatchInternal(use1, use2, orb, NormTypes.Hamming, "ORB", ratioThreshold, 2.0);
            Debug.WriteLine($"ORB耗时：{st.ElapsedMilliseconds}ms");
            return result;
            //return CalculateFeatureMatchInternal(processedImg1, processedImg2, orb, NormTypes.Hamming, "ORB", ratioThreshold, 2.0);
        }

        /// <summary>
        /// 计算内部特征匹配的相似度，即使ORB使用了更宽松的判定，在最终评分上ORB仍可以比SIFT更宽松。
        /// </summary>
        /// <param name="img1">第一张图片的Mat。</param>
        /// <param name="img2">第二张图片的Mat。</param>
        /// <param name="feature">使用的特征类。</param>
        /// <param name="normType">使用的规范类型。</param>
        /// <param name="methodName">调用方的算法名称。</param>
        /// <param name="ratioThreshold">
        /// Lowe's 比率测试阈值。
        /// <para>值越小越严格，留下的匹配点越少，适合高精度场景。</para>
        /// <para>值越大越宽松，几乎保留所有匹配点，适合跨视角、变形/缩放极其严重、需要先尽可能多捞点再靠 RANSAC 过滤的极端场景。</para>
        /// </param>
        /// <param name="ransacMultiplier">RANSAC 乘数。针对 ORB，允许增加阈值适应误差偏大的情况，推荐2.0。</param>
        /// <returns>相似度，ORB可以比SIFT更宽松。</returns>
        private static double CalculateFeatureMatchInternal(Mat img1, Mat img2, Feature2D feature, NormTypes normType, string methodName, double ratioThreshold = 0.85, double ransacMultiplier = 1.0)
        {
            using Mat descriptors1 = new();
            using Mat descriptors2 = new();

            feature.DetectAndCompute(img1, null, out KeyPoint[] keypoints1, descriptors1);
            feature.DetectAndCompute(img2, null, out KeyPoint[] keypoints2, descriptors2);

            if (descriptors1.Rows == 0 || descriptors2.Rows == 0)
            {
                Debug.WriteLine($"{methodName}: 特征点不足 (图1: {descriptors1.Rows}, 图2: {descriptors2.Rows})");
                return 0;
            }

            // 1. 暴力匹配
            using BFMatcher matcher = new(normType);
            DMatch[][] knnMatches = matcher.KnnMatch(descriptors1, descriptors2, 2);

            // 2. Lowe's 比率测试
            List<DMatch> goodMatches = [];
            foreach (DMatch[] matchPair in knnMatches)
            {
                if (matchPair.Length == 2 && matchPair[0].Distance < ratioThreshold * matchPair[1].Distance)
                {
                    goodMatches.Add(matchPair[0]);
                }
            }

            if (goodMatches.Count == 0) return 0;

            // 3. 提取匹配点对坐标
            Point2f[] srcPoints = [.. goodMatches.Select(m => keypoints1[m.QueryIdx].Pt)];
            Point2f[] dstPoints = [.. goodMatches.Select(m => keypoints2[m.TrainIdx].Pt)];

            if (srcPoints.Length < 4)
            {
                // 匹配点极度不足，说明物理位置相关性极低，给一个很低的分数即可
                double backupScore = Math.Min(1.0, goodMatches.Count / 50.0);
                Debug.WriteLine($"{methodName}: 回退得分 {backupScore} (匹配点小于4个，无法单应性计算。匹配点:{goodMatches.Count})");
                return backupScore;
            }

            // 4. 计算单应性矩阵并利用 RANSAC 获取内点
            using Mat mask = new();
            double baseRansac = 0.005 * Math.Sqrt(img1.Width * img1.Width + img1.Height * img1.Height);
            double ransacThreshold = Math.Max(3.0 * ransacMultiplier, baseRansac * ransacMultiplier);
            Mat homography = Cv2.FindHomography(InputArray.Create(srcPoints), InputArray.Create(dstPoints), HomographyMethods.Ransac, ransacThreshold, mask);

            List<DMatch> inliersMatches = [];
            for (int i = 0; i < goodMatches.Count; i++)
            {
                if (mask.At<byte>(i, 0) > 0)
                {
                    inliersMatches.Add(goodMatches[i]);
                }
            }

            // 如果 RANSAC 没找到内点，回退
            if (inliersMatches.Count < 4)
            {
                double backupScore = Math.Min(1.0, goodMatches.Count / 50.0);
                Debug.WriteLine($"{methodName}: 回退得分 {backupScore} (内点不足，内点：{inliersMatches.Count}，原始匹配点:{goodMatches.Count})");
                return backupScore;
            }

            // 5. 计算重投影误差
            Point2f[] inlierSrcPts = [.. inliersMatches.Select(m => keypoints1[m.QueryIdx].Pt)];
            Point2f[] inlierDstPts = [.. inliersMatches.Select(m => keypoints2[m.TrainIdx].Pt)];

            // 将源图内点坐标，根据单应性矩阵投影到目标图的坐标系
            using Mat projectedPtsMat = new();
            Cv2.PerspectiveTransform(InputArray.Create(inlierSrcPts), projectedPtsMat, homography);

            // 计算投影误差的平均值（欧氏距离）
            double totalError = 0;
            for (int i = 0; i < inlierSrcPts.Length; i++)
            {
                // 从 Mat 中取出投影后的坐标
                Point2f projected = projectedPtsMat.At<Point2f>(i, 0);
                Point2f actual = inlierDstPts[i];
                totalError += Math.Sqrt(Math.Pow(projected.X - actual.X, 2) + Math.Pow(projected.Y - actual.Y, 2));
            }
            double meanReprojectionError = totalError / inlierSrcPts.Length;

            // 计算召回率，内点数占原图最少特征点数量的比例，看图是否完整，越完整得分越高
            double recallRate = Math.Min(1.0, (double)inliersMatches.Count / Math.Min(keypoints1.Length, keypoints2.Length));

            // 计算绝对强度得分，抗缩放/裁剪，内点数和等效边长的比例
            double minArea = Math.Min(img1.Width * img1.Height, img2.Width * img2.Height);
            double adaptiveStrength = Math.Min(50.0, Math.Max(15.0, Math.Sqrt(minArea) / 15.0));
            double strengthRate = Math.Min(1.0, inliersMatches.Count / adaptiveStrength);

            // 匹配纯度，内点数占优质匹配点总数的比例，用于噪声/异常值的剔除
            double purityRate = Math.Min(1.0, (double)inliersMatches.Count / goodMatches.Count);

            // 计算加权平均
            double weight;
            if (methodName == "SIFT")
            {
                weight = (recallRate * 0.4) + (strengthRate * 0.4) + (purityRate * 0.2);
            }
            else
            {
                weight = (recallRate * 0.25) + (strengthRate * 0.6) + (purityRate * 0.15);
            }

            // 计算几何形变得分，SIFT 要求严苛（分母8），ORB 宽容度稍大（分母14）
            double geometryDenominator = methodName.Contains("SIFT") ? 8.0 : 14.0;
            double geometryScore = Math.Exp(-meanReprojectionError / geometryDenominator);
            double similarity = weight * geometryScore;

            Debug.WriteLine($"{methodName}:{similarity:f2},召回率:{recallRate:f2},强度:{strengthRate:f2},纯度:{purityRate:f2},重投影误差:{meanReprojectionError:F2}px,内点数:{inliersMatches.Count}/{goodMatches.Count}，{keypoints1.Length}-{keypoints2.Length}");
            return similarity;
        }


    }
}
