using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotCloudLib
{

	/// <summary>
	/// 曲线拟合
	/// </summary>
	public class CurveFit
	{
        /// <summary>
        /// 拟合后的方程系数
        /// </summary>
        public List<double> factor;

        /// <summary>
        /// 回归平方和
        /// </summary>
        public double ssr;

        /// <summary>
        /// 剩余平方和
        /// </summary>
        public double sse;

        /// <summary>
        /// RMSE均方根误差
        /// </summary>
        public double rmse;

        /// <summary>
        /// 存放拟合后的y值，在拟合时可设置为不保存节省内存
        /// </summary>
        public List<double> fitedYs;

		/// <summary>
		/// 多项式拟合
		/// </summary>
		/// <param name="x">X坐标</param>
		/// <param name="y">Y坐标</param>
		/// <param name="length">长度</param>
		/// <param name="poly_n">N</param>
		/// <param name="isSaveFitYs"></param>
		public void polyfit(List<double> x, List<double> y, int length, int poly_n, bool isSaveFitYs = true)
		{
			int i, j;
			//double *tempx,*tempy,*sumxx,*sumxy,*ata;

			List<double> tempx = new List<double>();
			for (i = 0; i < length; i++)
			{
				tempx.Add(1);
			}
			List<double> tempy = new List<double>(y);
			List<double> sumxx = new List<double>();
			for(i = 0; i < poly_n * 2 + 1; i++)
            {
				sumxx.Add(0);
            }
			List<double> ata = new List<double>();
			for(i = 0; i < (poly_n + 1) * (poly_n + 1); i++)
            {
				ata.Add(0);
            }
			List<double> sumxy = new List<double>(poly_n + 1);
			for (i = 0; i < poly_n + 1; i++)
			{
				sumxy.Add(0);
			}

			for (i = 0; i < 2 * poly_n + 1; i++)
			{
				for (sumxx[i] = 0, j = 0; j < length; j++)
				{
					sumxx[i] += tempx[j];
					tempx[j] *= x[j];
				}
			}
			for (i = 0; i < poly_n + 1; i++)
			{
				for (sumxy[i] = 0, j = 0; j < length; j++)
				{
					sumxy[i] += tempy[j];
					tempy[j] *= x[j];
				}
			}
			for (i = 0; i < poly_n + 1; i++)
			{
				for (j = 0; j < poly_n + 1; j++)
				{
					ata[i * (poly_n + 1) + j] = sumxx[i + j];
				}
			}

			gauss_solve(poly_n + 1, ata, sumxy);

			calcError(x, y, length, isSaveFitYs);
		}

        /// <summary>
        /// 计算损失
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="length"></param>
        /// <param name="isSaveFitYs"></param>
        void calcError(List<double> x, List<double> y, int length, bool isSaveFitYs = true)
		{
			this.ssr = 0;
			this.sse = 0;
			this.rmse = 0;
			fitedYs = new List<double>();

			double mean_y = (y.Max() - y.Min()) / 2;
			double yi = 0;
			for (int i = 0; i < length; ++i)
			{
				yi = getY(x[i]);
				ssr += ((yi - mean_y) * (yi - mean_y));//计算回归平方和
				sse += ((yi - y[i]) * (yi - y[i]));//残差平方和
				if (isSaveFitYs)
				{
					fitedYs.Add((double)(yi));
				}
			}
			rmse = Math.Sqrt(sse / ((double)(length)));
		}

		/// <summary>
		/// 获取Y值
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public double getY(double x)
		{
			double ans = 0;
			for (int i = 0; i < factor.Count; ++i)
			{
				ans += factor[i] * Math.Pow((double)x, (int)i);
			}
			return ans;
		}

		/// <summary>
		/// 高斯求解
		/// </summary>
		/// <param name="n"></param>
		/// <param name="A"></param>
		/// <param name="b"></param>
		void gauss_solve(int n, List<double> A, List<double> b)
		{
			this.factor = new List<double>();
			for (int ii = 0; ii < n; ii++)
			{
				factor.Add(0);
			}

			int i, j, k, r;
			double max;
			for (k = 0; k < n - 1; k++)
			{
				max = Math.Abs(A[k * n + k]); /*find maxmum*/
				r = k;
				for (i = k + 1; i < n - 1; i++)
				{
					if (max < Math.Abs(A[i * n + i]))
					{
						max = Math.Abs(A[i * n + i]);
						r = i;
					}
				}
				if (r != k)
				{
					for (i = 0; i < n; i++)         /*change array:A[k]&A[r] */
					{
						max = A[k * n + i];
						A[k * n + i] = A[r * n + i];
						A[r * n + i] = max;
					}
				}
				max = b[k];                    /*change array:b[k]&b[r]     */
				b[k] = b[r];
				b[r] = max;
				for (i = k + 1; i < n; i++)
				{
					for (j = k + 1; j < n; j++)
					{
						A[i * n + j] -= A[i * n + k] * A[k * n + j] / A[k * n + k];
					}
					b[i] -= A[i * n + k] * b[k] / A[k * n + k];
				}
			}

			for (i = n - 1; i >= 0; factor[i] /= A[i * n + i], i--)
			{
				for (j = i + 1, factor[i] = b[i]; j < n; j++)
				{
					factor[i] -= A[i * n + j] * factor[j];
				}
			}
		}
	}
	
}
