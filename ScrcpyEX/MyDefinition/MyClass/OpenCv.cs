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
            // 确保图像尺寸一致
            if (img1.Size() != img2.Size())
                Cv2.Resize(img2, img2, img1.Size());

            // 转换为 float 类型以提升计算精度
            img1.ConvertTo(img1, MatType.CV_32F);
            img2.ConvertTo(img2, MatType.CV_32F);

            // 高斯模糊参数
            OpenCvSharp.Size gaussianSize = new(11, 11);
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
        /// 使用结构相似性指标（SSIM）比较两张图片的相似性。
        /// </summary>
        /// <param name="path1">第一张图片的路径。</param>
        /// <param name="bitmapSource">CroppedBitmap类型的第二张图片。</param>
        /// <returns>SSIM值在0.0到1.0之间，值越大表示相似度越高。</returns>
        public static double CompareSSIM(string path1, BitmapSource bitmapSource)
        {
            using Mat img1 = Cv2.ImRead(path1, ImreadModes.Grayscale);
            using Mat img2 = ConvertBitmapSourceToMat(bitmapSource);

            // 确保图像尺寸一致
            if (img1.Size() != img2.Size())
                Cv2.Resize(img2, img2, img1.Size());

            // 转换为 float 类型以提升计算精度
            img1.ConvertTo(img1, MatType.CV_32F);
            img2.ConvertTo(img2, MatType.CV_32F);

            // 高斯模糊参数
            OpenCvSharp.Size gaussianSize = new(11, 11);
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

        /// <summary>
        /// 将CroppedBitmap类型的图片转换为OpenCv可以处理的Mat类型。
        /// </summary>
        /// <param name="croppedBitmap">CroppedBitmap类型的图片。</param>
        /// <returns>Mat类型。</returns>
        //public static Mat ConvertCroppedBitmapToMat(CroppedBitmap croppedBitmap)
        //{
        //    // 1. 转换为 BGRA32 格式
        //    var convertedBitmap = new FormatConvertedBitmap();
        //    convertedBitmap.BeginInit();
        //    convertedBitmap.Source = croppedBitmap;
        //    convertedBitmap.DestinationFormat = PixelFormats.Bgra32;
        //    convertedBitmap.EndInit();
        //
        //    // 2. 提取像素数据
        //    int width = convertedBitmap.PixelWidth;
        //    int height = convertedBitmap.PixelHeight;
        //    int stride = (width * 32 + 7) / 8; // 32 bits per pixel (BGRA32)
        //    byte[] pixelData = new byte[height * stride];
        //    convertedBitmap.CopyPixels(pixelData, stride, 0);
        //
        //    // 3. 使用 Mat.FromPixelData 创建 Mat
        //    Mat mat;
        //    unsafe
        //    {
        //        fixed (byte* pData = pixelData)
        //        {
        //            // 关键修复点：使用 FromPixelData，并传递正确的参数
        //            mat = Mat.FromPixelData(
        //                rows: height,
        //                cols: width,
        //                type: MatType.CV_8UC4,
        //                data: (nint)pData,  // 转换为 nint 类型
        //                step: stride // 步长以字节为单位
        //            );
        //
        //            mat = mat.Clone(); // 克隆以持久化数据（避免指针失效）
        //        }
        //    }
        //
        //    // 4. 转换为灰度图，使用ColorConversionCodes.BGRA2BGR会异常
        //    Cv2.CvtColor(mat, mat, ColorConversionCodes.BGRA2GRAY);
        //    return mat;
        //}

        public static Scalar Compare_SSIM(string imgFile1, string imgFile2)
        {
            var image1 = Cv2.ImRead(imgFile1);
            var image2Tmp = Cv2.ImRead(imgFile2);
            // 将两个图片处理成同样大小，否则会有错误： The operation is neither 'array op array' (where arrays have the same size and the same number of channels), nor 'array op scalar', nor 'scalar op array'
            var image2 = new Mat();
            Cv2.Resize(image2Tmp, image2, new OpenCvSharp.Size(image1.Size().Width, image1.Size().Height));
            double C1 = 6.5025, C2 = 58.5225;
            var validImage1 = new Mat();
            var validImage2 = new Mat();
            image1.ConvertTo(validImage1, MatType.CV_32F); //数据类型转换为 float,防止后续计算出现错误
            image2.ConvertTo(validImage2, MatType.CV_32F);


            Mat image1_1 = validImage1.Mul(validImage1); //图像乘积
            Mat image2_2 = validImage2.Mul(validImage2);
            Mat image1_2 = validImage1.Mul(validImage2);

            Mat gausBlur1 = new(), gausBlur2 = new(), gausBlur12 = new();
            Cv2.GaussianBlur(validImage1, gausBlur1, new OpenCvSharp.Size(11, 11), 1.5); //高斯卷积核计算图像均值
            Cv2.GaussianBlur(validImage2, gausBlur2, new OpenCvSharp.Size(11, 11), 1.5);
            Cv2.GaussianBlur(image1_2, gausBlur12, new OpenCvSharp.Size(11, 11), 1.5);

            //Mat imageAvgProduct = gausBlur1.Mul(gausBlur2); //均值乘积
            Mat u1Squre = gausBlur1.Mul(gausBlur1); //各自均值的平方
            Mat u2Squre = gausBlur2.Mul(gausBlur2);

            Mat imageConvariance, imageVariance1, imageVariance2;
            Mat squreAvg1 = new(), squreAvg2 = new();
            Cv2.GaussianBlur(image1_1, squreAvg1, new OpenCvSharp.Size(11, 11), 1.5); //图像平方的均值
            Cv2.GaussianBlur(image2_2, squreAvg2, new OpenCvSharp.Size(11, 11), 1.5);

            imageConvariance = gausBlur12 - gausBlur1.Mul(gausBlur2);// 计算协方差
            imageVariance1 = squreAvg1 - gausBlur1.Mul(gausBlur1); //计算方差
            imageVariance2 = squreAvg2 - gausBlur2.Mul(gausBlur2);

            var member = ((2 * gausBlur1.Mul(gausBlur2) + new Scalar(C1)).Mul(2 * imageConvariance + new Scalar(C2)));
            var denominator = ((u1Squre + u2Squre + new Scalar(C1)).Mul(imageVariance1 + imageVariance2 + new Scalar(C2)));

            Mat ssim = new();
            Cv2.Divide(member, denominator, ssim);

            var sclar = Cv2.Mean(ssim);
            return sclar;  // 变化率，即差异
        }


    }
}
