using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ScrcpyEX.MyDefinition.MyClass
{
    internal class OpenCv
    {
        /// <summary>
        /// 使用结构相似性指标（SSIM）比较两张图片的相似性。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="path2">第二张图片的路径。</param>
        /// <returns>SSIM值在0.0到1.0之间，值越大表示相似度越高。</returns>
        public static double CompareSSIM(string path1, string path2)
        {
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = Cv2.ImRead(path2, ImreadModes.Grayscale);

            return CompareSSIM(img1, img2);
        }

        /// <summary>
        /// 使用结构相似性指标（SSIM）比较两张图片的相似性。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="bitmapSource">CroppedBitmap类型的第二张图片。</param>
        /// <returns>SSIM值在0.0到1.0之间，值越大表示相似度越高。</returns>
        public static double CompareSSIM(string path1, BitmapSource bitmapSource)
        {
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = ConvertBitmapSourceToMat(bitmapSource);

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
        /// <param name="bitmapSource">BitmapSource类型及其派生类型的图片。</param>
        /// <returns>Mat类型。</returns>
        public static Mat ConvertBitmapSourceToMat(BitmapSource bitmapSource)
        {
            BitmapSource source = bitmapSource;

            // 1. 转换为 BGRA32 格式
            if (bitmapSource.Format != PixelFormats.Bgra32)
            {
                FormatConvertedBitmap convertedBitmap = new();
                convertedBitmap.BeginInit();
                convertedBitmap.Source = bitmapSource;
                convertedBitmap.DestinationFormat = PixelFormats.Bgra32;
                convertedBitmap.EndInit();
                source = convertedBitmap;
            }

            // 2. 提取像素数据
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = (width * source.Format.BitsPerPixel + 7) / 8;//32 bits per pixel (BGRA32)
            byte[] pixelData = new byte[height * stride];
            source.CopyPixels(pixelData, stride, 0);

            // 3. 使用 Mat.FromPixelData 创建 Mat
            Mat mat;
            unsafe
            {
                fixed (byte* pData = pixelData)
                {
                    // 关键修复点：使用 FromPixelData，并传递正确的参数
                    mat = Mat.FromPixelData(
                        rows: height,
                        cols: width,
                        type: MatType.CV_8UC4,
                        data: (nint)pData,  // 转换为 nint 类型
                        step: stride // 步长以字节为单位
                    );

                    mat = mat.Clone(); // 克隆以持久化数据（避免指针失效）
                }
            }

            // 4. 转换为灰度图，使用ColorConversionCodes.BGRA2BGR会异常
            Cv2.CvtColor(mat, mat, ColorConversionCodes.BGRA2GRAY);
            return mat;
        }


    }
}
